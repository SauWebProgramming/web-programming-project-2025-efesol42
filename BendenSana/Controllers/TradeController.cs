using BendenSana.Models;
using BendenSana.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize(Roles ="User, Seller")]
    public class TradeController : Controller
    {
        private readonly ITradeRepository _tradeRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public TradeController(ITradeRepository tradeRepo, UserManager<ApplicationUser> userManager)
        {
            _tradeRepo = tradeRepo;
            _userManager = userManager;
        }

        [Authorize(Roles ="User")]
        [HttpGet]
        public async Task<IActionResult> Create(int targetProductId)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetProduct = await _tradeRepo.GetTargetProductAsync(targetProductId);

            if (targetProduct == null) return NotFound("Ürün bulunamadı.");

            if (targetProduct.SellerId == user.Id)
            {
                TempData["Error"] = "Kendi ürününüze takas teklif edemezsiniz.";
                return RedirectToAction("Details", "Product", new { id = targetProductId });
            }

            ViewBag.TargetProductId = targetProductId;
            ViewBag.TargetProductName = targetProduct.Title;
            ViewBag.SellerName = targetProduct.Seller.UserName;
            ViewBag.TargetProductImage = targetProduct.Images?.FirstOrDefault()?.ImageUrl;
            ViewBag.TargetProductPrice = targetProduct.Price;

            var myProducts = await _tradeRepo.GetUserAvailableProductsAsync(user.Id);
            return View(myProducts);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create(int targetProductId, List<int> offeredProductIds, decimal? offeredCash, string message)
        {
            var user = await _userManager.GetUserAsync(User);
            var targetProduct = await _tradeRepo.GetTargetProductAsync(targetProductId);

            if (targetProduct == null) return NotFound();
            if (targetProduct.SellerId == user.Id)
                return RedirectToAction("Details", "Product", new { id = targetProductId });

            bool hasProducts = offeredProductIds != null && offeredProductIds.Any();
            bool hasCash = offeredCash != null && offeredCash > 0;

            if (!hasProducts && !hasCash)
            {
                TempData["Error"] = "Lütfen takas için en az bir ürün seçin veya nakit para teklif edin.";
                return RedirectToAction("Create", new { targetProductId = targetProductId });
            }

            var offer = new TradeOffer
            {
                OffererId = user.Id,
                ReceiverId = targetProduct.SellerId,
                ProductId = targetProductId, // Önceki hatayı önlemek için eklendi
                Status = TradeOfferStatus.Pending,
                OffererMessage = message,
                OfferedCashAmount = offeredCash,
                CreatedAt = DateTime.UtcNow,
            };

            // İstenen Ürün
            offer.Items.Add(new TradeItem { ProductId = targetProductId, ItemType = TradeItemType.requested });

            // Teklif Edilen Ürünler
            if (hasProducts)
            {
                foreach (var prodId in offeredProductIds)
                {
                    offer.Items.Add(new TradeItem { ProductId = prodId, ItemType = TradeItemType.offered });
                }
            }

            await _tradeRepo.AddTradeOfferAsync(offer);
            await _tradeRepo.SaveChangesAsync();

            TempData["Success"] = "Takas teklifi başarıyla gönderildi!";
            return RedirectToAction("Details", "Product", new { id = targetProductId });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var trades = await _tradeRepo.GetUserTradeOffersAsync(user.Id);
            return View(trades);
        }

        [Authorize(Roles="Seller")]
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var offer = await _tradeRepo.GetTradeOfferWithDetailsAsync(id);

            if (offer == null) return NotFound();
            if (offer.ReceiverId != user.Id) return Forbid();

            offer.Status = TradeOfferStatus.Accepted;
            offer.UpdatedAt = DateTime.UtcNow;

            foreach (var item in offer.Items)
            {
                if (item.Product != null)
                {
                    item.Product.Status = ProductStatus.sold;
                }
            }

            await _tradeRepo.SaveChangesAsync();
            TempData["Success"] = "Teklif kabul edildi ve ürünler satıştan kaldırıldı! 🎉";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles="Seller")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var offer = await _tradeRepo.GetTradeOfferWithDetailsAsync(id); // Detaylı çekmeye gerek yok ama tutarlılık için

            if (offer == null) return NotFound();
            if (offer.ReceiverId != user.Id) return Forbid();

            offer.Status = TradeOfferStatus.Rejected;
            offer.UpdatedAt = DateTime.UtcNow;

            await _tradeRepo.SaveChangesAsync();
            TempData["Info"] = "Teklifi reddettiniz.";
            return RedirectToAction(nameof(Index));
        }
    }
}