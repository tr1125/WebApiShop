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
            double? minPrice, double? maxPrice, List<Category>? categories,
            string? name, string? desc, int?[] categoryIds)
        {
            var query = _webApiShopContext.Products.Where(product =>
            (desc == null ? (true) : (product.Description.Contains(desc)))
            && ((minPrice == null) ? (true) : (product.Price >= minPrice))
            && ((maxPrice == null) ? (true) : (product.Price <= maxPrice))
            && ((categoryIds.Length == 0) ? (true) : (categoryIds.Contains(product.CategoryId))))
            .OrderBy(product => product.Price);
            //var res = _webApiShopContext.Products;
            //return await _webApiShopContext.Products.ToListAsync();
            List<Product> products = await query.Skip((position -1)*skip)
                .Take(skip)
                .Include(product=>product.Category).ToListAsync();
            var total = await query.CountAsync();
            return (products, total);

            //return res.ToList();

        }
    }
}
