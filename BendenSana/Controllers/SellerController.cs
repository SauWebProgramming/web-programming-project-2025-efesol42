using BendenSana.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Controllers
{
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            // 1. Üst Kartlar İçin İstatistikler
            ViewBag.TotalOrders = await _context.Set<Order>().CountAsync(); 
             ViewBag.TotalUsers = await _userManager.Users.CountAsync(); 
             ViewBag.TotalProducts = await _context.Set<Product>().CountAsync(); 

            // 2. Grafik Verileri (Sipariş Sayısı Analizi)
            var orderData = await _context.Set<Order>()
                .GroupBy(o => o.CreatedAt)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date).Take(10).ToListAsync();

            ViewBag.OrderLabels = orderData.Select(x => x.Date).ToList();
            ViewBag.OrderCounts = orderData.Select(x => x.Count).ToList();

            // 3. Kazanç Analizi (Revenue)
            var revenueData = await _context.Set<Order>()
                .GroupBy(o => o.CreatedAt)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => (double)o.TotalPrice) })
                .OrderBy(g => g.Date).Take(10).ToListAsync();

            ViewBag.RevenueValues = revenueData.Select(x => x.Total).ToList();
            ViewBag.TotalRevenue = revenueData.Sum(x => x.Total).ToString("N2");

            // 4. Latest Products (Son 3 Ürün)
            var latestProducts = await _context.Products
                .Where(p => p.SellerId == user.Id)
                .Where(p => p.Status == ProductStatus.published)
                .Include(p => p.Images)
               .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();
            // Today's Stats için veriler
            var today = DateTime.Now.Date;
            ViewBag.TodaySales = await _context.Set<Order>()
                .Where(o => o.CreatedAt >= today)
                .SumAsync(o => (double?)o.TotalPrice) ?? 0;

            // Ülke bazlı satışlar (Şablonun dolması için örnek veri)
            ViewBag.CountryStats = new List<dynamic> {
        new { Name = "United States", Value = "31,200", SEO = "40%" },
        new { Name = "United Kingdom", Value = "12,700", SEO = "47%" }
            };

            return View(latestProducts);
        }


        [HttpGet]
        public async Task<IActionResult> MyProducts(string search, int? categoryId, string status, string gender, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.SellerId == user.Id)
                .AsQueryable();

            // 1. Arama Filtresi
            if (!string.IsNullOrEmpty(search))
            {
                var keywords = search.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyword in keywords)
                {
                    query = query.Where(p => p.Title.ToLower().Contains(keyword) ||
                                             p.Description.ToLower().Contains(keyword) ||
                                             p.Category.Name.ToLower().Contains(keyword));
                }
            }

            // 2. Kategori Filtresi
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);

            // 3. Status Filtresi (Hatanın düzeltildiği yer)
            if (!string.IsNullOrEmpty(status))
            {
                // Gelen string değeri veritabanındaki Enum tipiyle karşılaştırıyoruz
                if (status.ToLower() == "published")
                    query = query.Where(p => p.Status == ProductStatus.published); // Veritabanındaki Enum karşılığı 
                else if (status.ToLower() == "blocked")
                     query = query.Where(p => p.Status == ProductStatus.blocked); 
    }

            // 4. Gender Filtresi
            if (!string.IsNullOrEmpty(gender)) {
                if (gender.ToLower() == "male")
                    query = query.Where(p => p.Gender == ProductGender.Male); // Veritabanındaki Enum karşılığı 
                else if (gender.ToLower() == "female")
                    query = query.Where(p => p.Gender == ProductGender.Female);
                else if (gender.ToLower() == "unisex")
                    query = query.Where(p => p.Gender == ProductGender.Unisex);
                else if (gender.ToLower() == "kids")
                    query = query.Where(p => p.Gender == ProductGender.Kids);
                else if (gender.ToLower() == "unisex")
                    query = query.Where(p => p.Gender == ProductGender.Unisex);
            }

    // Sayfalama işlemleri
    var totalItems = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Orders(string search, string sortBy, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int pageSize = 5;

            // 1. AŞAMA: Ana sorguyu oluştur (Join ve Filtreleme)
            var query = from o in _context.Set<Order>()
                        join oi in _context.Set<OrderItem>() on o.Id equals oi.OrderId
                        where oi.SellerId == user.Id
                        select new { o, oi };

            // Arama Filtresi (ToString hatasını engellemek için sadece OrderCode üzerinden veya Enum karşılaştırmasıyla yapılır)
            if (!string.IsNullOrEmpty(search))
            {
                var keyword = search.ToLower();
                query = query.Where(x => x.o.OrderCode.ToLower().Contains(keyword));
            }

            // Tarih Filtresi
            if (sortBy == "last_day")
                query = query.Where(x => x.o.CreatedAt >= DateTime.UtcNow.AddDays(-1));
            else if (sortBy == "last_week")
                query = query.Where(x => x.o.CreatedAt >= DateTime.UtcNow.AddDays(-7));

            // 2. AŞAMA: Gruplama ve Veriyi Belleğe Çekme
            // SQLite decimal Sum desteklemediği için hesaplanacak değerleri (Price, Quantity) ham liste olarak alıyoruz
            var groupedData = await query
                .GroupBy(x => x.o.Id)
                .Select(g => new
                {
                    Id = g.Key,
                    OrderCode = g.FirstOrDefault().o.OrderCode,
                    CreatedAt = g.FirstOrDefault().o.CreatedAt,
                    Status = g.FirstOrDefault().o.Status,
                    // Toplama işlemini C# tarafında yapmak için elemanları seçiyoruz
                    ItemsForCalc = g.Select(i => new { i.oi.Price, i.oi.Quantity }).ToList()
                })
                .ToListAsync();

            // 3. AŞAMA: C# Tarafında (In-Memory) Hesaplama ve Sayfalama
            var allOrders = groupedData.Select(x => new OrderViewModel
            {
                Id = x.Id,
                OrderCode = x.OrderCode,
                CreatedAt = x.CreatedAt,
                Status = x.Status?.ToString() ?? "Unknown", // ToString işlemi burada güvenli
                                                            // Decimal toplama işlemi burada (LINQ to Objects) sorunsuz çalışır
                SellerTotal = x.ItemsForCalc.Sum(i => i.Price * i.Quantity)
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

            // Sayfalama hesaplamaları
            var totalOrders = allOrders.Count;
            var pagedOrders = allOrders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.TotalOrders = totalOrders;

            return View(pagedOrders);
        }


        [Authorize]
        public async Task<IActionResult> Trades(string search, string sortBy, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int pageSize = 5;

            // Kullanıcının teklif veren VEYA teklif alan olduğu takasları çekiyoruz
            var query = _context.Set<TradeOffer>()
                .Include(t => t.Offerer)
                .Include(t => t.Receiver)
                .Include(t => t.Items)
                .Where(t => t.OffererId == user.Id || t.ReceiverId == user.Id)
                .AsQueryable();

            // 1. Arama Filtresi (Kod üzerinden)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TradeCode.ToLower().Contains(search.ToLower()));
            }

            // 2. Tarih Filtresi
            if (sortBy == "last_day")
                query = query.Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-1));
            else if (sortBy == "last_week")
                query = query.Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7));

            // 3. Veriyi Belleğe Çekme ve ViewModel'e Dönüştürme
            var tradeList = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            var model = tradeList.Select(t => new TradeViewModel
            {
                Id = t.Id,
                TradeCode = t.TradeCode,
                CreatedAt = t.CreatedAt,
                Status = t.Status.ToString(),
                CashAmount = t.OfferedCashAmount,
                ItemCount = t.Items.Count,
                // Karşı tarafın adını belirliyoruz
                PartnerName = t.OffererId == user.Id
                    ? $"{t.Receiver.FirstName} {t.Receiver.LastName}"
                    : $"{t.Offerer.FirstName} {t.Offerer.LastName}"
            }).ToList();

            // Sayfalama
            var totalTrades = model.Count;
            var pagedTrades = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTrades / pageSize);
            ViewBag.TotalTrades = totalTrades;

            return View(pagedTrades);
        }



        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // 1. Siparişin temel bilgilerini ve Alıcı (Buyer) verisini çekiyoruz
            var order = await _context.Set<Order>()
                .Include(o => o.Buyer)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // 2. Join İşlemi: Siparişe ait ürünleri ProductViewModel formatında çekiyoruz
            var orderItems = await (from oi in _context.Set<OrderItem>()
                                    join p in _context.Products on oi.ProductId equals p.Id
                                    join c in _context.Categories on p.CategoryId equals c.Id
                                    where oi.OrderId == id && oi.SellerId == user.Id // Sadece bu satıcının ürünleri
                                    select new ProductViewModel
                                    {
                                        Id = p.Id,
                                        Title = p.Title,
                                        Description = p.Description,
                                        Price = oi.Price, // Sipariş anındaki fiyat
                                        CategoryName = c.Name,
                                        CoverImageUrl = _context.ProductImages
                                                        .Where(img => img.ProductId == p.Id)
                                                        .Select(img => img.ImageUrl)
                                                        .FirstOrDefault() ?? "/images/no-image.png"
                                    }).ToListAsync();

            ViewBag.OrderItems = orderItems;
            return View(order);
        }

        // Durum güncelleme için (Tasarımındaki Save butonu için)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Set<Order>().FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OrderDetails), new { id = id });
        }

        [Authorize]
        public async Task<IActionResult> TradeDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // 1. TradeOffer bilgilerini, tarafları ve üstteki ana ürünü (Requested Product) çekiyoruz
            var trade = await _context.Set<TradeOffer>()
                .Include(t => t.Offerer)
                .Include(t => t.Receiver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trade == null) return NotFound();

            // 2. Üst Liste: TradeOffer üzerindeki ProductID ile eşleşen ana ürünü çekiyoruz
            // (Not: Modelinizde TradeOffer içinde ProductId olduğunu varsayıyoruz)
            var requestedProduct = await (from p in _context.Products
                                          join c in _context.Categories on p.CategoryId equals c.Id
                                          where p.Id == trade.ProductId // TradeOffer'daki ana ürün
                                          select new ProductViewModel
                                          {
                                              Id = p.Id,
                                              Title = p.Title,
                                              Description = p.Description,
                                              Price = p.Price,
                                              CategoryName = c.Name,
                                              CoverImageUrl = _context.ProductImages
                                                              .Where(img => img.ProductId == p.Id)
                                                              .Select(img => img.ImageUrl)
                                                              .FirstOrDefault() ?? "/images/no-image.png"
                                          }).FirstOrDefaultAsync();

            // 3. Alt Liste: Trade_Items tablosundaki tüm takas ürünlerini çekiyoruz
            var offeredItems = await (from ti in _context.Set<TradeItem>()
                                      join p in _context.Products on ti.ProductId equals p.Id
                                      join c in _context.Categories on p.CategoryId equals c.Id
                                      where ti.TradeId == id
                                      select new ProductViewModel
                                      {
                                          Id = p.Id,
                                          Title = p.Title,
                                          Description = p.Description,
                                          Price = p.Price,
                                          CategoryName = c.Name,
                                          CoverImageUrl = _context.ProductImages
                                                          .Where(img => img.ProductId == p.Id)
                                                          .Select(img => img.ImageUrl)
                                                          .FirstOrDefault() ?? "/images/no-image.png"
                                      }).ToListAsync();

            ViewBag.RequestedProduct = requestedProduct;
            ViewBag.OfferedItems = offeredItems;

            return View(trade);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTradeState(int id, TradeOfferStatus status)
        {
            var trade = await _context.Set<TradeOffer>().FindAsync(id);
            if (trade == null) return NotFound();

            trade.Status = status;
            trade.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Takas durumu güncellendi.";

            return RedirectToAction(nameof(TradeDetails), new { id = id });
        }

    }
}
