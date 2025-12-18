using BendenSana.Models;

namespace BendenSana.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        IEnumerable<Product> GetProductsWithCategories();

        Product GetProductDetails(int id);
    }
}