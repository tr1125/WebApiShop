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
        public async Task<List<Product>> GetProductsByConditions(double? minPrice, double? maxPrice,
            List<Category>? categories, string? name)
        {
            //var res = _webApiShopContext.Products;
            return await _webApiShopContext.Products.ToListAsync();


            //return res.ToList();

        }
    }
}
