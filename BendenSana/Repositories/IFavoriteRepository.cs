using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{

    public interface IFavoriteRepository
    {
        Task<List<Favorite>> GetUserFavoritesAsync(string userId);
        Task<Favorite?> GetFavoriteAsync(string userId, int productId);
        Task AddAsync(Favorite favorite);
        Task RemoveAsync(Favorite favorite);
        Task<bool> IsFavoriteAsync(string userId, int productId);
        Task SaveChangesAsync();
    }

    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
        {
            return await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.Images)
                .Where(f => f.UserId == userId)
                .AsNoTracking() // Sadece listeleme için performans artırır
                .ToListAsync();
        }

        public async Task<Favorite?> GetFavoriteAsync(string userId, int productId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task AddAsync(Favorite favorite)
        {
            await _context.Favorites.AddAsync(favorite);
        }

        public async Task RemoveAsync(Favorite favorite)
        {
            _context.Favorites.Remove(favorite);
            await Task.CompletedTask;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
