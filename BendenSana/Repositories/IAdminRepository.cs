using BendenSana.Models.ViewModels;
using BendenSana.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Repositories
{
    public interface IAdminRepository
    {
        // Dashboard İstatistikleri
        Task<decimal> GetTotalSalesAsync();
        Task<int> GetTotalOrdersCountAsync();
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetTotalUsersCountAsync();
        Task<List<(DateTime Date, decimal Total)>> GetDailySalesAsync(int days);

        // Satıcı ve Kullanıcı İşlemleri
        Task<List<SellerListViewModel>> GetTopSellersAsync(int count);
        Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedSellersAsync(string search, int page, int pageSize);
        Task<bool> DeleteUserWithAllDataAsync(string userId);

        // Şikayet ve Kategori
        Task<List<ProductReport>> GetAllReportsAsync();
        Task<bool> DismissReportAsync(int id);
        Task<bool> BanProductAsync(int reportId);
        Task<(List<Category> Categories, int TotalCount)> GetPagedCategoriesAsync(string search, int page, int pageSize);
        Task<SellerDetailsViewModel?> GetSellerDetailsAsync(string userId);
        Task<List<CountryStatViewModel>> GetCountryStatisticsAsync();
    }

    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminRepository(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            // SQLite kısıtlaması nedeniyle Sum işlemi bellekte yapılır
            var prices = await _context.Set<Order>().Select(o => o.TotalPrice).ToListAsync();
            return prices.Sum();
        }

        public async Task<int> GetTotalOrdersCountAsync() => await _context.Set<Order>().CountAsync();
        public async Task<int> GetTotalProductsCountAsync() => await _context.Set<Product>().CountAsync();
        public async Task<int> GetTotalUsersCountAsync() => await _userManager.Users.CountAsync();

        public async Task<List<(DateTime Date, decimal Total)>> GetDailySalesAsync(int days)
        {
            var limitDate = DateTime.Now.AddDays(-(days - 1)).Date;
            var orders = await _context.Set<Order>()
                .Where(o => o.CreatedAt >= limitDate)
                .Select(o => new { o.CreatedAt, o.TotalPrice })
                .ToListAsync();

            return orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => (g.Key, g.Sum(x => x.TotalPrice)))
                .OrderBy(x => x.Key)
                .ToList();
        }

        public async Task<List<SellerListViewModel>> GetTopSellersAsync(int count)
        {
            var topSellersData = await _userManager.Users
                .Select(u => new {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    u.Email,
                    ProductCount = _context.Set<Product>().Count(p => p.SellerId == u.Id)
                })
                .OrderByDescending(s => s.ProductCount)
                .Take(count)
                .ToListAsync();

            return topSellersData.Select(s => new SellerListViewModel
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                ProductCount = s.ProductCount
            }).ToList();
        }
        public async Task<List<CountryStatViewModel>> GetCountryStatisticsAsync()
        {
            // 1. Ülke bazlı gruplandırma ve sayım
            var rawStats = await _context.Set<Address>()
                .Where(a => !string.IsNullOrEmpty(a.Country))
                .GroupBy(a => a.Country)
                .Select(g => new
                {
                    Country = g.Key!,
                    Count = g.Count()
                })
                .ToListAsync();

            // 2. Toplam kayıtlı adres sayısını bul (Oran hesaplamak için)
            int totalAddresses = rawStats.Sum(x => x.Count);

            if (totalAddresses == 0) return new List<CountryStatViewModel>();

            // 3. Oranları hesapla ve ViewModel'e dönüştür
            return rawStats.Select(s => new CountryStatViewModel
            {
                Name = s.Country,
                Value = s.Count,
                // SEO sütunu için yüzde hesaplama: (Ülke Sayısı / Toplam) * 100
                SEO = $"%{Math.Round((double)s.Count / totalAddresses * 100, 1)}"
            })
            .OrderByDescending(x => x.Value) // En çok kullanıcısı olan ülke üstte
            .ToList();
        }
        public async Task<(List<ApplicationUser> Users, int TotalCount)> GetPagedSellersAsync(string search, int page, int pageSize)
        {
            var query = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
            }

            var total = await query.CountAsync();
            var users = await query.OrderBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (users, total);
        }

        public async Task<bool> DeleteUserWithAllDataAsync(string userId)
        {
            // Veri bütünlüğü için Transaction başlatıyoruz
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var id = userId;

                // İlişkili tüm verileri temizle
                _context.Set<TradeOffer>().RemoveRange(_context.Set<TradeOffer>().Where(t => t.OffererId == id || t.ReceiverId == id));
                _context.Set<Message>().RemoveRange(_context.Set<Message>().Where(m => _context.Set<Conversation>().Where(c => c.BuyerId == id || c.SellerId == id).Select(c => c.Id).Contains(m.ConversationId)));
                _context.Set<Conversation>().RemoveRange(_context.Set<Conversation>().Where(c => c.BuyerId == id || c.SellerId == id));
                _context.Set<Favorite>().RemoveRange(_context.Set<Favorite>().Where(f => f.UserId == id));
                _context.Set<Review>().RemoveRange(_context.Set<Review>().Where(r => r.UserId == id));
                _context.Set<Order>().RemoveRange(_context.Set<Order>().Where(o => o.BuyerId == id));

                var cart = await _context.Set<Cart>().FirstOrDefaultAsync(c => c.UserId == id);
                if (cart != null) _context.Set<Cart>().Remove(cart);

                _context.Set<Product>().RemoveRange(_context.Set<Product>().Where(p => p.SellerId == id));

                await _context.SaveChangesAsync();

                // Kullanıcıyı sil
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded) throw new Exception("Kullanıcı silinemedi.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<ProductReport>> GetAllReportsAsync() =>
            await _context.Set<ProductReport>().Include(r => r.Product).ThenInclude(p => p.Seller).Include(r => r.Reporter).ToListAsync();

        public async Task<bool> DismissReportAsync(int id)
        {
            var report = await _context.Set<ProductReport>().FindAsync(id);
            if (report == null) return false;
            _context.Set<ProductReport>().Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BanProductAsync(int reportId)
        {
            var report = await _context.Set<ProductReport>().Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return false;
            if (report.Product != null) _context.Set<Product>().Remove(report.Product);
            _context.Set<ProductReport>().Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(List<Category> Categories, int TotalCount)> GetPagedCategoriesAsync(string search, int page, int pageSize)
        {
            // Sadece ana kategorileri (ParentId == null) baz alıyoruz
            var query = _context.Set<Category>()
                .Include(c => c.Children)
                .Where(c => c.ParentId == null)
                .AsNoTracking();

            // Arama Filtresi (Ana kategori adı veya alt kategori adlarında arama yapar)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) ||
                                         c.Children.Any(child => child.Name.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }
        public async Task<SellerDetailsViewModel?> GetSellerDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // Satıcının ürünlerini detaylarıyla getiriyoruz
            var products = await _context.Set<Product>()
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Satıcının toplam kaç satış yaptığını (OrderItem üzerinden) hesaplıyoruz
            // Not: SQLite Sum kısıtlaması nedeniyle CountAsync veya SumAsync kontrolü yapılır
            var totalSalesCount = await _context.Set<OrderItem>()
                .Where(oi => oi.SellerId == userId)
                .CountAsync();

            return new SellerDetailsViewModel
            {
                User = user,
                Products = products,
                TotalSalesCount = totalSalesCount
            };
        }
    }
}