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


        public async Task<ProductDTO?> GetProductById(int id)
        {
            _logger.LogInformation("GetProductById called with id={Id}", id);
            Product? product = await _repository.GetProductById(id);
            if (product == null)
                _logger.LogWarning("Product not found for id={Id}", id);
            return product != null ? _mapper.Map<Product, ProductDTO>(product) : null;
        }

        public async Task<ProductDTO> AddProduct(ProductDTO productDto)
        {
            _logger.LogInformation("AddProduct called with name={Name}", productDto?.ProductName);
            var product = _mapper.Map<ProductDTO, Product>(productDto);
            var addedProduct = await _repository.AddProduct(product);
            _logger.LogInformation("Product added with id={Id}", addedProduct?.ProductId);
            return _mapper.Map<Product, ProductDTO>(addedProduct);
        }

        public async Task<ProductDTO> UpdateProduct(int id, ProductDTO productDto)
        {
            _logger.LogInformation("UpdateProduct called for id={Id}", id);
            var product = _mapper.Map<ProductDTO, Product>(productDto);
            var updatedProduct = await _repository.UpdateProduct(id, product);
            if (updatedProduct == null)
                _logger.LogWarning("UpdateProduct: product not found for id={Id}", id);
            else
                _logger.LogInformation("Product updated for id={Id}", id);
            return updatedProduct != null ? _mapper.Map<Product, ProductDTO>(updatedProduct) : null;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            _logger.LogInformation("DeleteProduct called for id={Id}", id);
            bool result = await _repository.DeleteProduct(id);
            if (!result)
                _logger.LogWarning("DeleteProduct: product not found for id={Id}", id);
            else
                _logger.LogInformation("Product deleted for id={Id}", id);
            return result;
        }
    }
}
