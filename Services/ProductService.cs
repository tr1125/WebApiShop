using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories;
using Entities;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Product>> GetProductsByConditions(double? minPrice, double? maxPrice,
            List<Category>? categories, string? name)
        {
            return await _repository.GetProductsByConditions(minPrice, maxPrice, categories, name);

        }
    }
}
