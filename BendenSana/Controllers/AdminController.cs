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
            // 1. TEMEL İSTATİSTİKLER
            // SQLite decimal Sum() desteklemediği için veriyi belleğe çekip topluyoruz
            var allOrderPrices = await _context.Set<Order>()
                                               .Select(o => o.TotalPrice)
                                               .ToListAsync();

            ViewBag.TotalSales = allOrderPrices.Sum();
            ViewBag.TotalOrders = await _context.Set<Order>().CountAsync();
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Set<Product>().CountAsync();

            // 2. GRAFİK VERİSİ (Son 12 Günün Satış Faaliyetleri)
            var startDateLimit = DateTime.Now.AddDays(-11);

            // Verileri ham halde çekiyoruz (SQLite kısıtlamaları nedeniyle gruplama bellekte yapılacak)
            var chartOrders = await _context.Set<Order>()
                .Select(o => new { o.CreatedAt, o.TotalPrice })
                .ToListAsync();

            var chartData = chartOrders
            .GroupBy(o => o.CreatedAt.Date) // CreatedAt zaten DateTime olduğu için direkt .Date kullanıyoruz
            .Select(g => new {
                Date = g.Key,
                Total = g.Sum(o => o.TotalPrice)
            })
            .Where(d => d.Date >= startDateLimit.Date)
            .OrderBy(g => g.Date)
            .ToList();

            ViewBag.ChartLabels = chartData.Select(d => d.Date.ToString("dd MMM")).ToList();
            ViewBag.ChartValues = chartData.Select(d => d.Total).ToList();

            // 3. EN ÇOK SATAN SATICILAR (Görseldeki 3'lü yapı için)
            // Önce ham veriyi çekip sonra ViewModel'e dönüştürmek SQLite için daha güvenlidir
            var topSellersData = await _userManager.Users
                .Select(u => new {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    u.Email,
                    ProductCount = _context.Set<Product>().Count(p => p.SellerId == u.Id)
                })
                .OrderByDescending(s => s.ProductCount)
                .Take(3)
                .ToListAsync();

            ViewBag.TopSellers = topSellersData.Select(s => new SellerListViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                ProductCount = s.ProductCount
            }).ToList();

            // 4. SON SİPARİŞLER (Alt kısımdaki tablo için)
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
        // AdminController.cs
        public async Task<IActionResult> Sellers(string search, int page = 1)
        {
            int pageSize = 10; // Her sayfada 10 satıcı
            var query = _userManager.Users.AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
            }

            var totalSellers = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new List<SellerListViewModel>();

            foreach (var user in users)
            {
                model.Add(new SellerListViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Address = "İstanbul, Türkiye", // Gerçek adres verisi varsa u.Address yazılabilir
                    ProductCount = _context.Set<Product>().Count(p => p.SellerId == user.Id)
                });
            }

            // View tarafında sayfalama için gerekli veriler
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalSellers / pageSize);
            ViewBag.SearchTerm = search;

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
        public async Task<IActionResult> Category()
        {
            var categories = await _context.Categories
                                           .Include(c => c.Children)
                                           .Where(c => c.ParentId == null)
                                           .AsNoTracking() // Sadece okuma yaptığımız için performans artırır
                                           .ToListAsync();
            return View(categories);
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