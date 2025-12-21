using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface IPaymentRepository
    {
        Task<UserCard?> GetCardByUserIdAsync(string userId);
        Task AddCardAsync(UserCard card);
        Task UpdateCardAsync(UserCard card);
        Task SaveChangesAsync();
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserCard?> GetCardByUserIdAsync(string userId)
        {
            return await _context.Set<UserCard>().FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddCardAsync(UserCard card)
        {
            await _context.Set<UserCard>().AddAsync(card);
        }

        public async Task UpdateCardAsync(UserCard card)
        {
            _context.Set<UserCard>().Update(card);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
