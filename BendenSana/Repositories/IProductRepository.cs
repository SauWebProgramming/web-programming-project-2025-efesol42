using BendenSana.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> SearchProductsAsync(SearchViewModel model);
        Task<List<Product>> GetMyProductsAsync(string userId);
        Task<Product?> GetDetailsAsync(int id);
        Task<List<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count);
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task AddImageAsync(ProductImage image);
        Task AddReviewAsync(Review review);
        Task AddReportAsync(ProductReport report);
        Task<bool> IsFavoriteAsync(string userId, int productId);
        Task SaveChangesAsync();
    }

    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> SearchProductsAsync(SearchViewModel model)
        {
            var query = _context.Products
                .Include(p => p.Category).Include(p => p.Images).Include(p => p.Color)
                .Where(p => p.Status == ProductStatus.published).AsQueryable();

            if (!string.IsNullOrEmpty(model.SearchQuery))
            {
                var s = model.SearchQuery.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
            }

            if (model.SelectedCategoryIds?.Any() == true)
                query = query.Where(p => model.SelectedCategoryIds.Contains(p.CategoryId));

            if (model.SelectedColorIds?.Any() == true)
                query = query.Where(p => p.ColorId.HasValue && model.SelectedColorIds.Contains(p.ColorId.Value));

            if (model.SelectedGenders?.Any() == true)
                query = query.Where(p => p.Gender.HasValue && model.SelectedGenders.Contains(p.Gender.Value));

            if (model.MinPrice.HasValue) query = query.Where(p => p.Price >= model.MinPrice.Value);
            if (model.MaxPrice.HasValue) query = query.Where(p => p.Price <= model.MaxPrice.Value);

            query = model.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await query.ToListAsync();
        }

        public async Task<List<Product>> GetMyProductsAsync(string userId) =>
            await _context.Products.Include(p => p.Category).Include(p => p.Images)
                .Where(p => p.SellerId == userId).OrderByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<Product?> GetDetailsAsync(int id) =>
            await _context.Products.Include(p => p.Images).Include(p => p.Category).Include(p => p.Color)
                .Include(p => p.Seller).Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count) =>
            await _context.Products.Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.Id != currentProductId && p.Status == ProductStatus.published)
                .OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();

        public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);
        public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);
        public async Task UpdateAsync(Product product) { _context.Products.Update(product); await Task.CompletedTask; }
        public async Task DeleteAsync(Product product) { _context.Products.Remove(product); await Task.CompletedTask; }
        public async Task AddImageAsync(ProductImage image) => await _context.ProductImages.AddAsync(image);
        public async Task AddReviewAsync(Review review) => await _context.Reviews.AddAsync(review);
        public async Task AddReportAsync(ProductReport report) => await _context.ProductReports.AddAsync(report);
        public async Task<bool> IsFavoriteAsync(string userId, int productId) =>
            await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
