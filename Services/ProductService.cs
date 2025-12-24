using AutoMapper;
using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProductDTO>> GetProductsByConditions(double? minPrice, double? maxPrice,
            List<CategoryDTO>? categories, string? name)
        {
            List<Category>? categories2 = _mapper.Map<List<CategoryDTO>, List<Category>>(categories);
            List<Product> product= await _repository.GetProductsByConditions(minPrice, maxPrice, categories2, name);
            List<ProductDTO> dto=_mapper.Map<List<Product>, List<ProductDTO>>(product);
            return dto;

        }
    }
}
