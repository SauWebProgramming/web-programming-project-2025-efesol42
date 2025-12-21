using BendenSana.Models;
using BendenSana.Models.Repositories;
using BendenSana.Repositories;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    // [Authorize(Roles = "Admin")] // Canlıya alırken mutlaka açın
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly AppDbContext _context; // Bazı özel Include işlemleri için geçici kalabilir

        public AdminController(IAdminRepository adminRepo, AppDbContext context)
        {
            _adminRepo = adminRepo;
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD / ANA SAYFA 
        // ==========================================
        public async Task<IActionResult> Index()
        {
            // İstatistikler Repository'den geliyor
            ViewBag.TotalSales = await _adminRepo.GetTotalSalesAsync();
            ViewBag.TotalOrders = await _adminRepo.GetTotalOrdersCountAsync();
            ViewBag.TotalUsers = await _adminRepo.GetTotalUsersCountAsync();
            ViewBag.TotalProducts = await _adminRepo.GetTotalProductsCountAsync();

            // Grafik Verisi (Son 12 Gün)
            var chartData = await _adminRepo.GetDailySalesAsync(12);
            ViewBag.ChartLabels = chartData.Select(d => d.Date.ToString("dd MMM")).ToList();
            ViewBag.ChartValues = chartData.Select(d => d.Total).ToList();

            // En Çok Satan 3 Satıcı
            ViewBag.TopSellers = await _adminRepo.GetTopSellersAsync(3);

            // Son 5 Sipariş (Doğrudan Order tablosundan çekilebilir)
            var recentOrders = await _context.Set<Order>()
                                             .Include(o => o.Buyer)
                                             .OrderByDescending(o => o.CreatedAt)
                                             .Take(5)
                                             .ToListAsync();

            return View(recentOrders);
        }

        // ==========================================
        // 2. SATICI / KULLANICI YÖNETİMİ
        // ==========================================
        public async Task<IActionResult> Sellers(string search, int page = 1)
        {
            int pageSize = 10;
            var (users, totalCount) = await _adminRepo.GetPagedSellersAsync(search, page, pageSize);

            var model = new List<SellerListViewModel>();
            foreach (var user in users)
            {
                model.Add(new SellerListViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Address = "Türkiye", // Opsiyonel: user.Addresses.FirstOrDefault()?.City
                    ProductCount = await _context.Set<Product>().CountAsync(p => p.SellerId == user.Id)
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.SearchTerm = search;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Tüm ilişkili verilerle birlikte silme işlemi Repository içindeki Transaction ile yapılıyor
            var result = await _adminRepo.DeleteUserWithAllDataAsync(id);

            if (result)
                TempData["Success"] = "Kullanıcı ve ilişkili tüm verileri başarıyla silindi.";
            else
                TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";

            return RedirectToAction("Sellers");
        }

        // ==========================================
        // 3. ŞİKAYET VE ÜRÜN YÖNETİMİ
        // ==========================================
        public async Task<IActionResult> Reports()
        {
            var reports = await _adminRepo.GetAllReportsAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> DismissReport(int id)
        {
            await _adminRepo.DismissReportAsync(id);
            return RedirectToAction("Reports");
        }

        [HttpPost]
        public async Task<IActionResult> BanProduct(int reportId)
        {
            await _adminRepo.BanProductAsync(reportId);
            return RedirectToAction("Reports");
        }

        // ==========================================
        // 4. DİĞER (Kategori vb.)
        // ==========================================
        public async Task<IActionResult> Category(string search, int page = 1)
        {
            int pageSize = 5; // Sayfa başına kategori kartı sayısı
            var (categories, totalCount) = await _adminRepo.GetPagedCategoriesAsync(search, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.SearchTerm = search;

            return View(categories);
        }
    }
}