using Xunit;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities;

namespace TestWebApiShop.IntegrationTests
{
    public class ProductRepositoryIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly ProductRepository _productRepository;

        public ProductRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new WebApiShopContext(options);
            _productRepository = new ProductRepository(_dbContext);
        }

        private void CleanUp() => _dbContext?.Dispose();

        #region GetProductsByConditions Tests

        /// <summary>
        /// בדיקה: קבלת מוצרים מדטבש בד פטרט
        /// Path: HAPPY - דטטבצים תיט פטרטט דט טלב כל המוטריים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNoFilters_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple", Price = 10, CategoryId = 1, Description = "Red Fruit" },
                new Product { ProductId = 2, ProductName = "Banana", Price = 5, CategoryId = 1, Description = "Yellow Fruit" },
                new Product { ProductId = 3, ProductName = "Orange", Price = 8, CategoryId = 1, Description = "Orange Fruit" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, null, null, null, null, new int?[] { });

            // Assert
            Assert.Equal(3, items.Count);
            Assert.Equal(3, totalCount);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטריים ע פילטר minPrice (לם אעל maxPrice)
        /// Path: HAPPY - צרין גדול המוטריים ד שמט פטר minPrice
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithMinPriceFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Cheap", Price = 2, CategoryId = 1, Description = "Budget" },
                new Product { ProductId = 2, ProductName = "Expensive", Price = 100, CategoryId = 1, Description = "Premium" },
                new Product { ProductId = 3, ProductName = "MidRange", Price = 50, CategoryId = 1, Description = "Medium" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, 50, null, null, null, new int?[] { });

            // Assert
            Assert.Contains(items, p => p.ProductName == "Expensive");
            Assert.Contains(items, p => p.ProductName == "MidRange");
            Assert.DoesNotContain(items, p => p.ProductName == "Cheap");
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטריים ע פטר maxPrice (בלי אבט minPrice)
        /// Path: HAPPY - צרין גדול המוטריים ע maxPrice זעט ומעלה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithMaxPriceFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Cheap", Price = 2, CategoryId = 1, Description = "Low Price" },
                new Product { ProductId = 2, ProductName = "Expensive", Price = 100, CategoryId = 1, Description = "High Price" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, null, 50, null, null, new int?[] { });

            // Assert
            Assert.DoesNotContain(items, p => p.ProductName == "Expensive");
            Assert.Contains(items, p => p.ProductName == "Cheap");
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטריים בטוח סדרות מטבמים (min-max)
        /// Path: HAPPY - צרין גדול מוטר ד שבטוח המטבטטוים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithPriceRangeFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "TooLow", Price = 2, CategoryId = 1, Description = "Cheap" },
                new Product { ProductId = 2, ProductName = "InRange", Price = 50, CategoryId = 1, Description = "Perfect" },
                new Product { ProductId = 3, ProductName = "TooHigh", Price = 200, CategoryId = 1, Description = "Expensive" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, 10, 100, null, null, new int?[] { });

            // Assert
            Assert.Single(items);
            Assert.Equal("InRange", items[0].ProductName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטטים בשם דטחם (search)
        /// Path: HAPPY - צרין גדול בסדר אחד
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNameFilter_ReturnsMatchingProduct()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple", Price = 10, CategoryId = 1, Description = "Fruit" },
                new Product { ProductId = 2, ProductName = "Banana", Price = 5, CategoryId = 1, Description = "Fruit" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, null, null, "Apple", null, new int?[] { });

            // Assert
            Assert.Single(items);
            Assert.Equal("Apple", items[0].ProductName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטטים ךטגוריין מסוימים (שן דרגה)
        /// Path: HAPPY - צריף חזרת דטגוריה (שאר דפטר)
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithCategoryFilter_ReturnsProductsByCategory()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple", Price = 10, CategoryId = 1, Description = "Fruit" },
                new Product { ProductId = 2, ProductName = "Orange", Price = 8, CategoryId = 2, Description = "Citrus" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, null, null, null, null, new int?[] { 2 });

            // Assert
            Assert.Single(items);
            Assert.Equal("Orange", items[0].ProductName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת מוטריים בבקש קליט לאט בבדיקה כאשר דטראטטעט מרובה
        /// Path: UNHAPPY - צריך מטא רשימה ריקה אלט בתוצאה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Cheap", Price = 2, CategoryId = 1, Description = "Budget" }
            };
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _productRepository.GetProductsByConditions(1, 10, 1000, 2000, null, null, new int?[] { });

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, totalCount);
            CleanUp();
        }

        #endregion
    }
}
