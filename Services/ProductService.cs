using AutoMapper;
using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Extensions.Logging;
namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        public ProductService(IProductRepository repository, IMapper mapper, ILogger<ProductService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(List<ProductDTO> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
            double? minPrice, double? maxPrice,
            string? name, string? desc, int?[] categoryIds, string? color)
        {
            _logger.LogInformation("GetProductsByConditions called: position={Position}, skip={Skip}, minPrice={Min}, maxPrice={Max}, name={Name}", position, skip, minPrice, maxPrice, name);
            (List<Product> product, int total) = await _repository.GetProductsByConditions(position, skip, minPrice, maxPrice,  name, desc, categoryIds, color);
            if (product == null || product.Count == 0)
                _logger.LogWarning("GetProductsByConditions returned no results");
            else
                _logger.LogInformation("GetProductsByConditions found {Count} products (total={Total})", product.Count, total);
            List<ProductDTO> dto=_mapper.Map<List<Product>, List<ProductDTO>>(product);
            return (dto, total);
        }
    }
}
