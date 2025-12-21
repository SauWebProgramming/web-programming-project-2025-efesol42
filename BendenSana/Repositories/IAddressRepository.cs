using Microsoft.EntityFrameworkCore;

namespace BendenSana.Models.Repositories
{
    public interface IAddressRepository
    {
        Task<Address?> GetByUserIdAsync(string userId);
        Task<Address?> GetByIdAsync(int id, string userId);
        Task AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task DeleteAsync(Address address);
        Task SaveChangesAsync();
    }

    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Address?> GetByUserIdAsync(string userId)
        {
            return await _context.Set<Address>().FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Address?> GetByIdAsync(int id, string userId)
        {
            return await _context.Set<Address>().FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task AddAsync(Address address)
        {
            await _context.Set<Address>().AddAsync(address);
        }

        public async Task UpdateAsync(Address address)
        {
            _context.Set<Address>().Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Address address)
        {
            _context.Set<Address>().Remove(address);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
