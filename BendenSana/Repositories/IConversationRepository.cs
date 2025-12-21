using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface IConversationRepository
    {
        Task<List<Conversation>> GetUserConversationsAsync(string userId);
        Task<Conversation?> GetConversationWithDetailsAsync(int id);
        Task<Conversation?> FindExistingConversationAsync(string currentUserId, string targetUserId, int productId);
        Task AddConversationAsync(Conversation conversation);
        Task AddMessageAsync(Message message);
        Task<Conversation?> GetByIdAsync(int id);
        Task SaveChangesAsync();
    }


    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _context;

        public ConversationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            return await _context.Set<Conversation>()
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Product)
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == userId || c.SellerId == userId)
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationWithDetailsAsync(int id)
        {
            return await _context.Set<Conversation>()
                .Include(c => c.Messages)
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Conversation?> FindExistingConversationAsync(string currentUserId, string targetUserId, int productId)
        {
            return await _context.Set<Conversation>()
                .FirstOrDefaultAsync(c => c.ProductId == productId &&
                    ((c.BuyerId == currentUserId && c.SellerId == targetUserId) ||
                     (c.BuyerId == targetUserId && c.SellerId == currentUserId)));
        }

        public async Task AddConversationAsync(Conversation conversation)
        {
            await _context.Set<Conversation>().AddAsync(conversation);
        }

        public async Task AddMessageAsync(Message message)
        {
            await _context.Set<Message>().AddAsync(message);
        }

        public async Task<Conversation?> GetByIdAsync(int id)
        {
            return await _context.Set<Conversation>().FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
