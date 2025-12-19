using BendenSana.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize]
    public class TradeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TradeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 1. TAKAS TEKLİFİ OLUŞTURMA EKRANI (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create(int targetProductId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Hedef ürünü getir (Satıcı ve Resimler dahil)
            var targetProduct = await _context.Set<Product>()
                                              .Include(p => p.Seller)
                                              .Include(p => p.Images)
                                              .FirstOrDefaultAsync(p => p.Id == targetProductId);

            if (targetProduct == null) return NotFound("Ürün bulunamadı.");

            if (targetProduct.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ürününüze takas teklif edemezsiniz.";
                return RedirectToAction("Details", "Product", new { id = targetProductId });
            }

            // View'da kullanılacak Hedef Ürün verileri
            ViewBag.TargetProductId = targetProductId;
            ViewBag.TargetProductName = targetProduct.Title;
            ViewBag.SellerName = targetProduct.Seller.UserName;
            ViewBag.TargetProductImage = targetProduct.Images?.FirstOrDefault()?.ImageUrl;
            ViewBag.TargetProductPrice = targetProduct.Price;

            // Kullanıcının kendi ürünlerini getir (Çoklu seçim listesi için)
            // Sadece 'Available' (Müsait) olanları ve resimleriyle birlikte çekiyoruz
            var myProducts = await _context.Set<Product>()
                                           .Include(p => p.Images)
                                           .Where(p => p.SellerId == user.Id && p.Status == ProductStatus.available)
                                           .OrderByDescending(p => p.CreatedAt)
                                           .ToListAsync();

            // Modeli direkt liste olarak gönderiyoruz
            return View(myProducts);
        }

        // ==========================================
        // 2. TAKAS TEKLİFİ GÖNDERME (POST)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create(int targetProductId, List<int> offeredProductIds, decimal? offeredCash, string message)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetProduct = await _context.Set<Product>().FindAsync(targetProductId);

            if (targetProduct == null) return NotFound();

            // Güvenlik: Kendi ürününe teklif atamaz
            if (targetProduct.SellerId == user.Id)
                return RedirectToAction("Details", "Product", new { id = targetProductId });

            // Doğrulama: Hiç ürün seçilmediyse VE para teklif edilmediyse hata ver
            bool hasProducts = offeredProductIds != null && offeredProductIds.Any();
            bool hasCash = offeredCash != null && offeredCash > 0;

            if (!hasProducts && !hasCash)
            {
                TempData["Error"] = "Lütfen takas için en az bir ürün seçin veya nakit para teklif edin.";
                return RedirectToAction("Create", new { targetProductId = targetProductId });
            }

            // Ana Teklif Kaydı
            var offer = new TradeOffer
            {
                OffererId = user.Id,
                ReceiverId = targetProduct.SellerId,
                Status = TradeOfferStatus.Pending,
                OffererMessage = message,
                OfferedCashAmount = offeredCash,
                CreatedAt = DateTime.UtcNow,
            };

            // 1. İstenen Ürünü Ekle (Target)
            offer.Items.Add(new TradeItem
            {
                ProductId = targetProductId,
                ItemType = TradeItemType.requested
            });

            // 2. Teklif Edilen Ürünleri Ekle (Offered - Çoklu Seçim)
            if (hasProducts)
            {
                foreach (var prodId in offeredProductIds)
                {
                    offer.Items.Add(new TradeItem
                    {
                        ProductId = prodId,
                        ItemType = TradeItemType.offered
                    });
                }
            }

            _context.TradeOffers.Add(offer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Takas teklifi başarıyla gönderildi!";
            return RedirectToAction("Details", "Product", new { id = targetProductId });
        }

        // ==========================================
        // 3. TAKAS TEKLİFLERİM LİSTESİ (INDEX)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var trades = await _context.TradeOffers
                .Include(t => t.Offerer)
                .Include(t => t.Receiver)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Where(t => t.OffererId == userId || t.ReceiverId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(trades);
        }

        // ==========================================
        // 4. KABUL ET (ACCEPT)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = _userManager.GetUserId(User);

            var offer = await _context.TradeOffers
                .Include(t => t.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (offer == null) return NotFound();

            // Sadece alıcı kabul edebilir
            if (offer.ReceiverId != userId) return Forbid();

            offer.Status = TradeOfferStatus.Accepted;

            // Kabul edilen ürünleri 'Satıldı' olarak işaretle
            foreach (var item in offer.Items)
            {
                if (item.Product != null)
                {
                    item.Product.Status = ProductStatus.sold;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Teklif kabul edildi ve ürünler satıştan kaldırıldı! 🎉";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. REDDET (REJECT)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = _userManager.GetUserId(User);
            var offer = await _context.TradeOffers.FindAsync(id);

            if (offer == null) return NotFound();

            if (offer.ReceiverId != userId) return Forbid();

            offer.Status = TradeOfferStatus.Rejected;

            await _context.SaveChangesAsync();
            TempData["Info"] = "Teklifi reddettiniz.";

            return RedirectToAction(nameof(Index));
        }
    }
}