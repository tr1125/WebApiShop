using Entities;

namespace Repositories
{
    public interface IProductRepository
    {
        Task<(List<Product> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
            double? minPrice, double? maxPrice,
            string? name, string? desc, int?[] categoryIds);
    }
}