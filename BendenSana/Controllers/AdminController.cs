using BendenSana.Models;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    // [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD / ANA SAYFA 
        // ==========================================
        public async Task<IActionResult> Index()
        {
           
            

            var allPrices = await _context.Set<Order>()
                                          .Select(o => o.TotalPrice)
                                          .ToListAsync();

            ViewBag.TotalSales = allPrices.Sum(); 

           
            ViewBag.TotalOrders = await _context.Set<Order>().CountAsync();
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Set<Product>().CountAsync();

           
            var recentOrders = await _context.Set<Order>()
                                             .Include(o => o.Buyer)
                                             .OrderByDescending(o => o.CreatedAt)
                                             .Take(5)
                                             .ToListAsync();

            return View(recentOrders);
        }

        // ========================
        // SATICILARI LİSTELEME
        // ========================
        public async Task<IActionResult> Sellers()
        {
            // Tüm kullanıcıları getir
            var users = await _userManager.Users.ToListAsync();

            var model = new List<SellerListViewModel>();

            foreach (var user in users)
            {
                model.Add(new SellerListViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    // Şimdilik statik, ileride Address tablosundan çekilebilir
                    Address = "İstanbul, Türkiye",
                    ProductCount = _context.Set<Product>().Count(p => p.SellerId == user.Id)
                });
            }

            return View(model);
        }

        // ===================
        //  KULLANICI SİLME 
        // ==================
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // TAKAS TEKLİFLERİNİ TEMİZLE
            var tradeOffers = await _context.Set<TradeOffer>()
                                            .Where(t => t.OffererId == id || t.ReceiverId == id)
                                            .ToListAsync();
            if (tradeOffers.Any())
            {
                _context.Set<TradeOffer>().RemoveRange(tradeOffers);
            }

            //  KONUŞMALARI VE MESAJLARI TEMİZLE
            var conversations = await _context.Set<Conversation>()
                                              .Where(c => c.BuyerId == id || c.SellerId == id)
                                              .ToListAsync();
            if (conversations.Any())
            {
                foreach (var conv in conversations)
                {
                    var messages = await _context.Set<Message>()
                                                 .Where(m => m.ConversationId == conv.Id)
                                                 .ToListAsync();
                    _context.Set<Message>().RemoveRange(messages);
                }
                _context.Set<Conversation>().RemoveRange(conversations);
            }

            //  FAVORİLERİ TEMİZLE
            var favorites = await _context.Set<Favorite>()
                                          .Where(f => f.UserId == id)
                                          .ToListAsync();
            if (favorites.Any())
            {
                _context.Set<Favorite>().RemoveRange(favorites);
            }

            // YORUMLARI TEMİZLE
            var reviews = await _context.Set<Review>()
                                        .Where(r => r.UserId == id)
                                        .ToListAsync();
            if (reviews.Any())
            {
                _context.Set<Review>().RemoveRange(reviews);
            }

            // SİPARİŞLERİ TEMİZLE
            var userOrders = await _context.Set<Order>()
                                           .Where(o => o.BuyerId == id)
                                           .ToListAsync();
            if (userOrders.Any())
            {
                _context.Set<Order>().RemoveRange(userOrders);
            }

            //  SEPETİ TEMİZLE
            var userCart = await _context.Set<Cart>()
                                         .FirstOrDefaultAsync(c => c.UserId == id);
            if (userCart != null)
            {
                _context.Set<Cart>().Remove(userCart);
            }

            // ÜRÜNLERİ TEMİZLE
            var userProducts = await _context.Set<Product>()
                                             .Where(p => p.SellerId == id)
                                             .ToListAsync();
            if (userProducts.Any())
            {
                _context.Set<Product>().RemoveRange(userProducts);
            }

            // --- ARA KAYIT ---
            await _context.SaveChangesAsync();

            // KULLANICIYI SİL
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Sellers");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return RedirectToAction("Sellers");
            }
        }

        // ========================
        //  SATICI DETAY SAYFASI
        // ========================
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var products = await _context.Set<Product>()
                                         .Include(p => p.Images)
                                         .Include(p => p.Category)
                                         .Where(p => p.SellerId == id)
                                         .OrderByDescending(p => p.CreatedAt)
                                         .ToListAsync();

            var model = new SellerDetailsViewModel
            {
                User = user,
                Products = products,
                TotalSalesCount = 0
            };

            return View(model);
        }

        // ==========================================
        // ŞİKAYET YÖNETİMİ (REPORTS)
        // ==========================================
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.Set<ProductReport>()
                                        .Include(r => r.Product)
                                            .ThenInclude(p => p.Seller)
                                        .Include(r => r.Reporter)
                                        .OrderByDescending(r => r.CreatedAt)
                                        .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> DismissReport(int id)
        {
            var report = await _context.Set<ProductReport>().FindAsync(id);
            if (report != null)
            {
                _context.Set<ProductReport>().Remove(report);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Reports");
        }

        [HttpPost]
        public async Task<IActionResult> BanProduct(int reportId)
        {
            var report = await _context.Set<ProductReport>()
                                       .Include(r => r.Product)
                                       .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report != null)
            {
                if (report.Product != null)
                {
                    _context.Set<Product>().Remove(report.Product);
                }

                _context.Set<ProductReport>().Remove(report);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Reports");
        }


    }
}