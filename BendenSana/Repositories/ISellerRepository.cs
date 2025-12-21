using BendenSana.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface ISellerRepository
    {
        // Dashboard İstatistikleri
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalProductsCountAsync();
        Task<List<(DateTime Date, int Count)>> GetOrderCountDataAsync(int take);
        Task<List<(DateTime Date, double Total)>> GetRevenueDataAsync(int take);
        Task<double> GetTodaySalesAsync();

        // Ürün İşlemleri
        Task<List<Product>> GetLatestProductsAsync(string userId, int take);
        Task<(List<Product> Products, int TotalCount)> GetPagedSellerProductsAsync(string userId, string search, int? categoryId, string status, string gender, int page, int pageSize);

        // Sipariş ve Takas İşlemleri
        Task<(List<OrderViewModel> Orders, int TotalCount)> GetPagedOrdersAsync(string userId, string search, string sortBy, int page, int pageSize);
        Task<(List<TradeViewModel> Trades, int TotalCount)> GetPagedTradesAsync(string userId, string search, string sortBy, int page, int pageSize);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<List<ProductViewModel>> GetOrderProductsAsync(int orderId, string sellerId);
        Task<TradeOffer?> GetTradeWithParticipantsAsync(int tradeId);
        Task<ProductViewModel?> GetTradeMainProductAsync(int productId);
        Task<List<ProductViewModel>> GetTradeOfferedItemsAsync(int tradeId);

        Task SaveChangesAsync();
    }

    public class SellerRepository : ISellerRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // İstatistik Metotları
        public async Task<int> GetTotalOrdersCountAsync() => await _context.Set<Order>().CountAsync();
        public async Task<int> GetTotalUsersCountAsync() => await _userManager.Users.CountAsync();
        public async Task<int> GetTotalProductsCountAsync() => await _context.Set<Product>().CountAsync();

        public async Task<List<(DateTime Date, int Count)>> GetOrderCountDataAsync(int take)
        {
            var data = await _context.Set<Order>().GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date).Take(take).ToListAsync();
            return data.Select(x => (x.Date, x.Count)).ToList();
        }

        public async Task<List<(DateTime Date, double Total)>> GetRevenueDataAsync(int take)
        {
            var data = await _context.Set<Order>().GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(o => (double)o.TotalPrice) })
                .OrderBy(g => g.Date).Take(take).ToListAsync();
            return data.Select(x => (x.Date, x.Total)).ToList();
        }

        public async Task<double> GetTodaySalesAsync()
        {
            var today = DateTime.Now.Date;
            return await _context.Set<Order>().Where(o => o.CreatedAt >= today).SumAsync(o => (double?)o.TotalPrice) ?? 0;
        }

        // Ürün Metotları
        public async Task<List<Product>> GetLatestProductsAsync(string userId, int take) =>
            await _context.Products.Include(p => p.Images).Include(p => p.Category)
                .Where(p => p.SellerId == userId && p.Status == ProductStatus.published)
                .OrderByDescending(p => p.CreatedAt).Take(take).ToListAsync();

        public async Task<(List<Product> Products, int TotalCount)> GetPagedSellerProductsAsync(string userId, string search, int? categoryId, string status, string gender, int page, int pageSize)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Images).Where(p => p.SellerId == userId).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var k = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(k) || p.Category.Name.ToLower().Contains(k));
            }
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ProductStatus>(status, true, out var s)) query = query.Where(p => p.Status == s);
            }
            if (!string.IsNullOrEmpty(gender))
            {
                if (Enum.TryParse<ProductGender>(gender, true, out var g)) query = query.Where(p => p.Gender == g);
            }

            var total = await query.CountAsync();
            var data = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (data, total);
        }

        // Sipariş ve Takas Detay Sorguları
        public async Task<(List<OrderViewModel> Orders, int TotalCount)> GetPagedOrdersAsync(string userId, string search, string sortBy, int page, int pageSize)
        {
            var query = from o in _context.Set<Order>()
                        join oi in _context.Set<OrderItem>() on o.Id equals oi.OrderId
                        where oi.SellerId == userId
                        select new { o, oi };

            if (!string.IsNullOrEmpty(search)) query = query.Where(x => x.o.OrderCode.ToLower().Contains(search.ToLower()));
            if (sortBy == "last_day") query = query.Where(x => x.o.CreatedAt >= DateTime.UtcNow.AddDays(-1));

            var grouped = await query.GroupBy(x => x.o.Id).Select(g => new {
                Id = g.Key,
                OrderCode = g.FirstOrDefault().o.OrderCode,
                CreatedAt = g.FirstOrDefault().o.CreatedAt,
                Status = g.FirstOrDefault().o.Status,
                Calc = g.Select(i => new { i.oi.Price, i.oi.Quantity }).ToList()
            }).ToListAsync();

            var all = grouped.Select(x => new OrderViewModel
            {
                Id = x.Id,
                OrderCode = x.OrderCode,
                CreatedAt = x.CreatedAt,
                Status = x.Status.ToString(),
                SellerTotal = x.Calc.Sum(i => i.Price * i.Quantity)
            }).OrderByDescending(o => o.CreatedAt).ToList();

            return (all.Skip((page - 1) * pageSize).Take(pageSize).ToList(), all.Count);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId) =>
            await _context.Set<Order>().Include(o => o.Buyer).Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);

        public async Task<List<ProductViewModel>> GetOrderProductsAsync(int orderId, string sellerId) =>
            await (from oi in _context.Set<OrderItem>()
                   join p in _context.Products on oi.ProductId equals p.Id
                   join c in _context.Categories on p.CategoryId equals c.Id
                   where oi.OrderId == orderId && oi.SellerId == sellerId
                   select new ProductViewModel
                   {
                       Id = p.Id,
                       Title = p.Title,
                       Price = oi.Price,
                       CategoryName = c.Name,
                       CoverImageUrl = _context.ProductImages.Where(img => img.ProductId == p.Id).Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/no-image.png"
                   }).ToListAsync();

        // Takas Metotları (Örnekleme)
        public async Task<(List<TradeViewModel> Trades, int TotalCount)> GetPagedTradesAsync(string userId, string search, string sortBy, int page, int pageSize)
        {
            // 1. Temel sorguyu oluştur ve ilişkili tabloları dahil et
            var query = _context.Set<TradeOffer>()
                .Include(t => t.Offerer)
                .Include(t => t.Receiver)
                .Include(t => t.Items)
                .Where(t => t.OffererId == userId || t.ReceiverId == userId)
                .AsQueryable();

            // 2. Arama Filtresi (TradeCode üzerinden arama yapar)
            if (!string.IsNullOrEmpty(search))
            {
                var keyword = search.ToLower();
                query = query.Where(t => t.TradeCode.ToLower().Contains(keyword));
            }

            // 3. Sıralama ve Tarih Filtreleri
            if (sortBy == "last_day")
            {
                query = query.Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-1));
            }
            else if (sortBy == "last_week")
            {
                query = query.Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-7));
            }

            // 4. Veriyi veritabanından çek
            var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            // 5. Veriyi ViewModel'e Map et (Dönüştür)
            var model = list.Select(t => new TradeViewModel
            {
                Id = t.Id,
                TradeCode = t.TradeCode,
                CreatedAt = t.CreatedAt,
                Status = t.Status.ToString(), // Enum'ı string'e çevirir
                CashAmount = t.OfferedCashAmount,
                ItemCount = t.Items.Count,
                // Giriş yapan kullanıcının karşı tarafındaki kişinin adını bul
                PartnerName = t.OffererId == userId
                    ? $"{t.Receiver.FirstName} {t.Receiver.LastName}"
                    : $"{t.Offerer.FirstName} {t.Offerer.LastName}"
            }).ToList();

            // 6. Sayfalama işlemini bellekte (In-Memory) yap ve toplam sayıyı dön
            var totalCount = model.Count;
            var pagedData = model
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalCount);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        // Diğer Trade metodları (GetTradeWithParticipantsAsync vb.) benzer şekilde AppDbContext üzerinden doldurulur.
        public async Task<TradeOffer?> GetTradeWithParticipantsAsync(int tradeId) =>
            await _context.Set<TradeOffer>().Include(t => t.Offerer).Include(t => t.Receiver).FirstOrDefaultAsync(t => t.Id == tradeId);

        public async Task<ProductViewModel?> GetTradeMainProductAsync(int productId) =>
            await (from p in _context.Products
                   join c in _context.Categories on p.CategoryId equals c.Id
                   where p.Id == productId
                   select new ProductViewModel
                   {
                       Id = p.Id, // BURASI EKSİK OLABİLİR
                       Title = p.Title,
                       Price = p.Price,
                       CategoryName = c.Name,
                       CoverImageUrl = _context.ProductImages
                                .Where(img => img.ProductId == p.Id)
                                .Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/no-image.png"
                   }).FirstOrDefaultAsync();

        public async Task<List<ProductViewModel>> GetTradeOfferedItemsAsync(int tradeId) =>
            await (from ti in _context.Set<TradeItem>()
                   join p in _context.Products on ti.ProductId equals p.Id
                   join c in _context.Categories on p.CategoryId equals c.Id
                   where ti.TradeId == tradeId
                   select new ProductViewModel
                   {
                       Id = p.Id, // BURASI EKSİK OLABİLİR
                       Title = p.Title,
                       Price = p.Price,
                       CategoryName = c.Name,
                       CoverImageUrl = _context.ProductImages
                                .Where(img => img.ProductId == p.Id)
                                .Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/no-image.png"
                   }).ToListAsync();
    }
}
