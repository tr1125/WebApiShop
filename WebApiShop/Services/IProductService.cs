using Entities;
using DTOs;

namespace Services
{
    public interface IProductService
    {
        Task<(List<ProductDTO> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
                   double? minPrice, double? maxPrice, 
                   string? name, string? desc, int?[] categoryIds, string? color);
                   
        Task<ProductDTO?> GetProductById(int id);
        Task<ProductDTO> AddProduct(ProductDTO productDto);
        Task<ProductDTO> UpdateProduct(int id, ProductDTO productDto);
        Task<bool> DeleteProduct(int id);
    }
}