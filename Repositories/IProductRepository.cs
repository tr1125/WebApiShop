using Entities;

namespace Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsByConditions(double? minPrice, double? maxPrice, List<Category>? categories, string? name);
    }
}