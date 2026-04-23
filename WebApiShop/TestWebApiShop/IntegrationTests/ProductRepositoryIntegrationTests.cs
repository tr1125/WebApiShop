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

        private Category CreateTestCategory(int id = 1) =>
            new Category { CetegoryId = id, CategoryName = $"Category{id}" };

        private Product CreateTestProduct(int id, string name, double price, int categoryId = 1) =>
            new Product
            {
                ProductId   = id,
                ProductName = name,
                Price       = price,
                CategoryId  = categoryId,
                Description = $"Description for {name}",
                ImageURL    = $"https://example.com/{name}.jpg",
                Color       = "Red",
                IsDeleted   = false
            };

        #region GetProductsByConditions - GetAll Tests

        /// <summary>
        /// בדיקה: קבלת כל המוצרים כשיש מוצרים ב-DB
        /// Path: HAPPY - צריך להחזיר רשימת כל המוצרים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithProducts_ReturnsAll()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, "Cola",  10),
                CreateTestProduct(2, "Water", 5)
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: null, desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            Assert.Equal(2, total);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת כל המוצרים כשאין מוצרים ב-DB
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNoProducts_ReturnsEmpty()
        {
            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: null, desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
            Assert.Equal(0, total);
            CleanUp();
        }

        #endregion

        #region GetProductsByConditions - Category Filter Tests

        /// <summary>
        /// בדיקה: סינון מוצרים לפי קטגוריה קיימת
        /// Path: HAPPY - צריך להחזיר רק מוצרים מהקטגוריה הנבחרת
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByCategory_ReturnsFilteredList()
        {
            // Arrange
            await _dbContext.Categories.AddRangeAsync(CreateTestCategory(1), CreateTestCategory(2));
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, "Cola",  10, categoryId: 1),
                CreateTestProduct(2, "Chips", 8,  categoryId: 2)
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: null, desc: null,
                categoryIds: new int?[] { 1 }, color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.Equal("Cola", items[0].ProductName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: סינון מוצרים לפי קטגוריה שאינה קיימת
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByNonExistingCategory_ReturnsEmpty()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddAsync(CreateTestProduct(1, "Cola", 10));
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: null, desc: null,
                categoryIds: new int?[] { 999 }, color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
            CleanUp();
        }

        #endregion

        #region GetProductsByConditions - Price Range Filter Tests

        /// <summary>
        /// בדיקה: סינון מוצרים לפי טווח מחירים תקין
        /// Path: HAPPY - צריך להחזיר רק מוצרים בטווח
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByPriceRange_ReturnsFilteredList()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, "Cheap",     5),
                CreateTestProduct(2, "Mid",       50),
                CreateTestProduct(3, "Expensive", 200)
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: 10, maxPrice: 100,
                name: null, desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.Equal("Mid", items[0].ProductName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: סינון לפי טווח מחירים שאין בו מוצרים
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByPriceRangeNoResults_ReturnsEmpty()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddAsync(CreateTestProduct(1, "Cheap", 5));
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: 100, maxPrice: 500,
                name: null, desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
            CleanUp();
        }

        #endregion

        #region GetProductsByConditions - Name Filter Tests

        /// <summary>
        /// בדיקה: חיפוש מוצר לפי שם חלקי קיים
        /// Path: HAPPY - צריך להחזיר מוצרים שמכילים את השם
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByName_ReturnsMatchingProducts()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, "Apple Juice", 12),
                CreateTestProduct(2, "Apple Cider", 18),
                CreateTestProduct(3, "Orange Juice", 10)
            );
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: "Apple", desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            Assert.All(items, p => Assert.Contains("Apple", p.ProductName));
            CleanUp();
        }

        /// <summary>
        /// בדיקה: חיפוש מוצר לפי שם שלא קיים
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByNonExistingName_ReturnsEmpty()
        {
            // Arrange
            await _dbContext.Categories.AddAsync(CreateTestCategory());
            await _dbContext.Products.AddAsync(CreateTestProduct(1, "Cola", 10));
            await _dbContext.SaveChangesAsync();

            // Act
            var (items, total) = await _productRepository.GetProductsByConditions(
                position: 1, skip: int.MaxValue,
                minPrice: null, maxPrice: null,
                name: "XYZXYZ", desc: null,
                categoryIds: Array.Empty<int?>(), color: null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
            CleanUp();
        }

        #endregion
    }
}
