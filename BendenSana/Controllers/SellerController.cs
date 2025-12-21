using BendenSana.Models.Repositories;
using BendenSana.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ISellerRepository _sellerRepo;
        private readonly ICategoryRepository _categoryRepo; // Daha önce yazdığımız
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerController(ISellerRepository sellerRepo, ICategoryRepository categoryRepo, UserManager<ApplicationUser> userManager)
        {
            _sellerRepo = sellerRepo;
            _categoryRepo = categoryRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.TotalOrders = await _sellerRepo.GetTotalOrdersCountAsync();
            ViewBag.TotalUsers = await _sellerRepo.GetTotalUsersCountAsync();
            ViewBag.TotalProducts = await _sellerRepo.GetTotalProductsCountAsync();

            var orderData = await _sellerRepo.GetOrderCountDataAsync(10);
            ViewBag.OrderLabels = orderData.Select(x => x.Date).ToList();
            ViewBag.OrderCounts = orderData.Select(x => x.Count).ToList();

            var revenueData = await _sellerRepo.GetRevenueDataAsync(10);
            ViewBag.RevenueValues = revenueData.Select(x => x.Total).ToList();
            ViewBag.TotalRevenue = revenueData.Sum(x => x.Total).ToString("N2");

            ViewBag.TodaySales = await _sellerRepo.GetTodaySalesAsync();

            var latestProducts = await _sellerRepo.GetLatestProductsAsync(user.Id, 3);
            return View(latestProducts);
        }

        public async Task<IActionResult> MyProducts(string search, int? categoryId, string status, string gender, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            var (products, totalItems) = await _sellerRepo.GetPagedSellerProductsAsync(user.Id, search, categoryId, status, gender, page, 5);

            ViewBag.Categories = await _categoryRepo.GetAllAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / 5);
            ViewBag.CurrentPage = page;
            return View(products);
        }


        [Authorize]
        public async Task<IActionResult> Orders(string search, string sortBy, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int pageSize = 5;

            // Repository üzerinden filtrelenmiş ve sayfalanmış veriyi çekiyoruz
            var (orders, totalCount) = await _sellerRepo.GetPagedOrdersAsync(user.Id, search, sortBy, page, pageSize);

            // View tarafı için gerekli bilgileri ViewBag'e aktarıyoruz
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalOrders = totalCount;

            return View(orders);
        }
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _sellerRepo.GetOrderWithDetailsAsync(id);
            if (order == null) return NotFound();

            ViewBag.OrderItems = await _sellerRepo.GetOrderProductsAsync(id, user.Id);
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _sellerRepo.GetOrderWithDetailsAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _sellerRepo.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OrderDetails), new { id = id });
        }

        [Authorize]
        public async Task<IActionResult> Trades(string search, string sortBy, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int pageSize = 5;

            // Repository'deki tam halini yazdığımız metodu çağırıyoruz
            var (trades, totalCount) = await _sellerRepo.GetPagedTradesAsync(user.Id, search, sortBy, page, pageSize);

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalTrades = totalCount;

            return View(trades);
        }

        [Authorize]
        public async Task<IActionResult> TradeDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Takasın temel bilgilerini ve katılımcılarını getir
            var trade = await _sellerRepo.GetTradeWithParticipantsAsync(id);
            if (trade == null) return NotFound();

            // Güvenlik: Kullanıcı bu takasın bir tarafı mı?
            if (trade.OffererId != user.Id && trade.ReceiverId != user.Id) return Forbid();

            // 2. Takas edilen ana ürünü (Requested Product) getir
            ViewBag.RequestedProduct = await _sellerRepo.GetTradeMainProductAsync(trade.ProductId);

            // 3. Karşı tarafın teklif ettiği ürünleri listele
            ViewBag.OfferedItems = await _sellerRepo.GetTradeOfferedItemsAsync(id);

            return View(trade);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTradeState(int id, TradeOfferStatus status)
        {
            var trade = await _sellerRepo.GetTradeWithParticipantsAsync(id);
            if (trade == null) return NotFound();

            // Sadece takasın muhatabı olan kullanıcılar durumu değiştirebilir
            var user = await _userManager.GetUserAsync(User);
            if (trade.OffererId != user.Id && trade.ReceiverId != user.Id) return Forbid();

            trade.Status = status;
            trade.UpdatedAt = DateTime.UtcNow;

            await _sellerRepo.SaveChangesAsync();
            TempData["Success"] = $"Takas durumu başarıyla '{status}' olarak güncellendi.";

            return RedirectToAction(nameof(TradeDetails), new { id = id });
        }
    }
}
