using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetParentCategoriesAsync();
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetParentCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Children)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllAsync() => await _context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) => await _context.Categories.FindAsync(id);

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Categories.AnyAsync(e => e.Id == id);
    }
}