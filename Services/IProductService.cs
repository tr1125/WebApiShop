using Entities;
using DTOs;

namespace Services
{
    public interface IProductService
    {
        Task<(List<ProductDTO> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
                   double? minPrice, double? maxPrice, 
                   string? name, string? desc, int?[] categoryIds);
    }
}