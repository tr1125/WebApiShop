using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using StackExchange.Redis;



namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly WebApiShopContext _webApiShopContext;
        private readonly IDistributedCache _cache;
        private readonly int _ttlMinutes;
        private readonly IConnectionMultiplexer _redis;


        public ProductRepository(WebApiShopContext webApiShopContext, IDistributedCache cache, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _webApiShopContext = webApiShopContext;
            _cache = cache;
            _redis = redis;
            _ttlMinutes = configuration.GetValue<int>("Redis:TTLMinutes");
        }
        public async Task<(List<Product> Items, int TotalCount)> GetProductsByConditions(int position, int skip,
            double? minPrice, double? maxPrice,
            string? name, string? desc, int?[] categoryIds, string? color)
        {
            string cacheKey = $"products_{position}_{skip}_{minPrice}_{maxPrice}_{name}_{desc}_{string.Join(",", categoryIds)}_{color}";

            var jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            };

            var cachedItems = await _cache.GetStringAsync(cacheKey + "_items");
            var cachedTotal = await _cache.GetStringAsync(cacheKey + "_total");

            if (cachedItems != null && cachedTotal != null)
            {
                return (
                    JsonSerializer.Deserialize<List<Product>>(cachedItems, jsonOptions),
                    JsonSerializer.Deserialize<int>(cachedTotal)
                );
            }

            var query = _webApiShopContext.Products.Where(product =>
                !product.IsDeleted
                && (desc == null || (product.Description != null && product.Description.Contains(desc)))
                && (minPrice == null || product.Price >= minPrice)
                && (name == null || (product.ProductName != null && product.ProductName.Contains(name)))
                && (maxPrice == null || product.Price <= maxPrice)
                && (categoryIds.Length == 0 || categoryIds.Contains(product.CategoryId))
                && (color == null || product.Color == color))
                .OrderBy(product => product.Price);

            List<Product> products = await query.Skip((position - 1) * skip)
                .Take(skip)
                .Include(product => product.Category).ToListAsync();
            int total = await query.CountAsync();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_ttlMinutes)
            };

            await _cache.SetStringAsync(cacheKey + "_items", JsonSerializer.Serialize(products, jsonOptions), cacheOptions);
            await _cache.SetStringAsync(cacheKey + "_total", JsonSerializer.Serialize(total), cacheOptions);

            return (products, total);
        }

        public async Task<Product?> GetProductById(int id)
        {
            string cacheKey = $"product_{id}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                return JsonSerializer.Deserialize<Product>(cached);
            }

            var product = await _webApiShopContext.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_ttlMinutes)
                });
            }

            return product;
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
            await InvalidateProductCaches(id); // הוסף שורה זו

            return existingProduct;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existingProduct = await _webApiShopContext.Products.FindAsync(id);
            if (existingProduct == null) return false;

            // Soft delete
            existingProduct.IsDeleted = true;
            await _webApiShopContext.SaveChangesAsync();
            await InvalidateProductCaches(id); // הוסף שורה זו

            return true;
        }

        private async Task InvalidateProductCaches(int id)
        {
            // מוחק את המוצר הספציפי
            await _cache.RemoveAsync($"product_{id}");

            // מוחק את כל הרשימות שמתחילות ב-products_
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "products_*").ToArray();
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key!);
            }
        }

    }
}
