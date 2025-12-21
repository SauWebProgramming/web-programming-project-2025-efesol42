using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{

    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task DeleteAsync(Review review);
        Task<List<Review>> GetByProductIdAsync(int productId);
        Task SaveChangesAsync();
    }


    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Set<Review>().FindAsync(id);
        }

        public async Task AddAsync(Review review)
        {
            await _context.Set<Review>().AddAsync(review);
        }

        public async Task DeleteAsync(Review review)
        {
            _context.Set<Review>().Remove(review);
            await Task.CompletedTask;
        }

        public async Task<List<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Set<Review>()
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
