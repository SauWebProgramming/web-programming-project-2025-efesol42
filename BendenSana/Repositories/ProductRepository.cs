using BendenSana.Models;
using Microsoft.EntityFrameworkCore;

namespace BendenSana.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

      
        public IEnumerable<Product> GetProductsWithCategories()
        {
            return _context.Set<Product>()
                            .Include(x => x.Category)
                            .Include(x => x.Color)
                            .Include(x => x.Seller)
                            .Include(x => x.Images)
                            .ToList();
        }

       
        public Product GetProductDetailsWithReviews(int id)
        {
            return _context.Set<Product>()
                .Include(x => x.Category)
                .Include(x => x.Color)
                .Include(x => x.Seller)
                .Include(x => x.Images)
               
                .Include(x => x.Reviews)
                    .ThenInclude(r => r.User) 
                .FirstOrDefault(x => x.Id == id);
        }

        

        public Product GetProductDetails(int id)
        {
           

            return _context.Set<Product>()
                           .Include(p => p.Images)      
                           .Include(p => p.Seller)      
                           .Include(p => p.Category)    
                           .Include(p => p.Reviews)     
                                .ThenInclude(r => r.User) 
                           .FirstOrDefault(p => p.Id == id);
        }
    }
}