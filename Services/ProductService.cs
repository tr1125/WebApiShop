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

        public async Task<(List<ProductDTO> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
            double? minPrice, double? maxPrice,
            string? name, string? desc, int?[] categoryIds)
        {
            (List<Product> product, int total) = await _repository.GetProductsByConditions(position, skip, minPrice, maxPrice,  name, desc, categoryIds);
            List<ProductDTO> dto=_mapper.Map<List<Product>, List<ProductDTO>>(product);
            return (dto, total);

        }
    }
}
