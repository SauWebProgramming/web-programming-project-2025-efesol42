using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{

    public interface IOrderRepository
    {
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderDetailsAsync(int orderId, string userId);
        Task<List<Address>> GetUserAddressesAsync(string userId);
        Task<UserCard?> GetUserSavedCardAsync(string userId);
        Task<Cart?> GetCartWithItemsAsync(string userId);
        Task CreateOrderAsync(Order order, List<OrderItem> items);
        Task<int> CreateAddressAsync(Address address);
        Task ClearCartAsync(int cartId);
        Task SaveChangesAsync();
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Set<Order>()
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, string userId)
        {
            return await _context.Set<Order>()
                .Include(o => o.Address)
                .Include(o => o.Items).ThenInclude(oi => oi.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == userId);
        }

        public async Task<List<Address>> GetUserAddressesAsync(string userId) =>
            await _context.Set<Address>().Where(a => a.UserId == userId).ToListAsync();

        public async Task<UserCard?> GetUserSavedCardAsync(string userId) =>
            await _context.Set<UserCard>().FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<Cart?> GetCartWithItemsAsync(string userId)
        {
            return await _context.Set<Cart>()
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task CreateOrderAsync(Order order, List<OrderItem> items)
        {
            await _context.Set<Order>().AddAsync(order);
            await _context.SaveChangesAsync(); // OrderId oluşması için

            foreach (var item in items)
            {
                item.OrderId = order.Id;
                await _context.Set<OrderItem>().AddAsync(item);
            }
        }

        public async Task<int> CreateAddressAsync(Address address)
        {
            await _context.Set<Address>().AddAsync(address);
            await _context.SaveChangesAsync();
            return address.Id;
        }

        public async Task ClearCartAsync(int cartId)
        {
            var items = _context.Set<CartItem>().Where(ci => ci.CartId == cartId);
            _context.Set<CartItem>().RemoveRange(items);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
