using Entities;

namespace Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsByConditions(double? minPrice, double? maxPrice, List<Category>? categories, string? name);
    }
}