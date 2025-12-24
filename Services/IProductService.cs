using Entities;
using DTOs;

namespace Services
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetProductsByConditions(double? minPrice, double? maxPrice, List<CategoryDTO>? categories, string? name);
    }
}