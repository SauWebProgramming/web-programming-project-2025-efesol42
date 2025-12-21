using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface ITradeRepository
    {
        Task<Product?> GetTargetProductAsync(int productId);
        Task<List<Product>> GetUserAvailableProductsAsync(string userId);
        Task<TradeOffer?> GetTradeOfferWithDetailsAsync(int id);
        Task<List<TradeOffer>> GetUserTradeOffersAsync(string userId);
        Task AddTradeOfferAsync(TradeOffer offer);
        Task SaveChangesAsync();
    }
    public class TradeRepository : ITradeRepository
    {
        private readonly AppDbContext _context;

        public TradeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetTargetProductAsync(int productId)
        {
            return await _context.Set<Product>()
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<List<Product>> GetUserAvailableProductsAsync(string userId)
        {
            return await _context.Set<Product>()
                .Include(p => p.Images)
                .Where(p => p.SellerId == userId && p.Status == ProductStatus.available)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<TradeOffer?> GetTradeOfferWithDetailsAsync(int id)
        {
            return await _context.Set<TradeOffer>()
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TradeOffer>> GetUserTradeOffersAsync(string userId)
        {
            return await _context.Set<TradeOffer>()
                .Include(t => t.Offerer)
                .Include(t => t.Receiver)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Where(t => t.OffererId == userId || t.ReceiverId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddTradeOfferAsync(TradeOffer offer)
        {
            await _context.Set<TradeOffer>().AddAsync(offer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }


}
