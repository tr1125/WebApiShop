using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly WebApiShopContext _webApiShopContext;

        public ProductRepository(WebApiShopContext webApiShopContext)
        {
            _webApiShopContext = webApiShopContext;
        }
        public async Task<(List<Product> Items, int TotalCount)> GetProductsByConditions(int position,int skip,
            double? minPrice, double? maxPrice,
            string? name, string? desc, int?[] categoryIds, string? color)
        {
            var query = _webApiShopContext.Products.Where(product =>
            !product.IsDeleted // Filter out soft-deleted products
            && (desc == null || (product.Description != null && product.Description.Contains(desc)))
            && (minPrice == null || product.Price >= minPrice)
            && (name == null || (product.ProductName != null && product.ProductName.Contains(name)))
            && (maxPrice == null || product.Price <= maxPrice)
            && (categoryIds.Length == 0 || categoryIds.Contains(product.CategoryId))
            && (color == null || product.Color == color))
            .OrderBy(product => product.Price);

            List<Product> products = await query.Skip((position -1)*skip)
                .Take(skip)
                .Include(product=>product.Category).ToListAsync();
            int total = await query.CountAsync();
            return (products, total);

        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _webApiShopContext.Products.FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);
        }

        public async Task<Product> AddProduct(Product product)
        {
            await _webApiShopContext.Products.AddAsync(product);
            await _webApiShopContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProduct(int id, Product product)
        {
            var existingProduct = await _webApiShopContext.Products.FindAsync(id);
            if (existingProduct == null) return null;

            existingProduct.ProductName = product.ProductName;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Description = product.Description;
            existingProduct.ImageURL = product.ImageURL;
            existingProduct.Color = product.Color;
            existingProduct.IsDeleted = product.IsDeleted;

            await _webApiShopContext.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existingProduct = await _webApiShopContext.Products.FindAsync(id);
            if (existingProduct == null) return false;

            // Soft delete
            existingProduct.IsDeleted = true;
            await _webApiShopContext.SaveChangesAsync();
            return true;
        }
    }
}
