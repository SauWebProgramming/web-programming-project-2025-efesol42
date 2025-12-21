using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task<Cart> CreateCartAsync(string userId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task<CartItem?> GetCartItemByIdAsync(int id);
        Task AddCartItemAsync(CartItem item);
        Task RemoveCartItemAsync(CartItem item);
        Task SaveChangesAsync();
    }
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            return await _context.Set<Cart>()
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> CreateCartAsync(string userId)
        {
            var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            await _context.Set<Cart>().AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.Set<CartItem>()
                .FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int id)
        {
            return await _context.Set<CartItem>().FindAsync(id);
        }

        public async Task AddCartItemAsync(CartItem item)
        {
            await _context.Set<CartItem>().AddAsync(item);
        }

        public async Task RemoveCartItemAsync(CartItem item)
        {
            _context.Set<CartItem>().Remove(item);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}