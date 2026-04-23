using Xunit;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestWebApiShop.UnitTests
{
    public class ProductServiceUnitTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly IMapper _mapper;
        private readonly ProductService _productService;

        public ProductServiceUnitTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _productService = new ProductService(_mockProductRepository.Object, _mapper, NullLogger<ProductService>.Instance);
        }

        // עוזר: mock ל-GetProductsByConditions עם ברירות מחדל
        private void SetupGetByConditions(List<Product> products,
            double? minPrice = null, double? maxPrice = null,
            string? name = null, int?[]? categoryIds = null)
        {
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    minPrice, maxPrice,
                    name, null,
                    It.IsAny<int?[]>(), null))
                .ReturnsAsync((products, products.Count));
        }

        #region GetProductsByConditions - GetAll Tests

        /// <summary>
        /// בדיקה: קבלת כל המוצרים כשיש לפחות 20
        /// Path: HAPPY - צריך להחזיר את כל המוצרים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithTwentyOrMoreProducts_ReturnsAll()
        {
            // Arrange
            var products = new List<Product>();
            for (int i = 1; i <= 20; i++)
                products.Add(new Product
                {
                    ProductId   = i,
                    ProductName = $"Product{i}",
                    Price       = i * 10,
                    CategoryId  = 1,
                    IsDeleted   = false
                });

            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((products, products.Count));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, null, null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(20, items.Count);
            Assert.Equal(20, total);
        }

        /// <summary>
        /// בדיקה: קבלת כל המוצרים כשאין מוצרים
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNoProducts_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, null, null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        #endregion

        #region GetProductsByConditions - Category Filter Tests

        /// <summary>
        /// בדיקה: סינון מוצרים לפי קטגוריה קיימת
        /// Path: HAPPY - צריך להחזיר רק מוצרים מהקטגוריה הנבחרת
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByCategory_ReturnsList()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Cola",  Price = 10, CategoryId = 1, IsDeleted = false },
                new Product { ProductId = 2, ProductName = "Fanta", Price = 8,  CategoryId = 1, IsDeleted = false }
            };
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((products, products.Count));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, null, null, new int?[] { 1 }, null);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            Assert.All(items, p => Assert.Equal(1, p.CategoryId));
        }

        /// <summary>
        /// בדיקה: סינון מוצרים לפי קטגוריה שאינה קיימת
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByNonExistingCategory_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, null, null, new int?[] { 999 }, null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        #endregion

        #region GetProductsByConditions - Price Range Filter Tests

        /// <summary>
        /// בדיקה: סינון מוצרים לפי טווח מחירים תקין
        /// Path: HAPPY - צריך להחזיר מוצרים בטווח המחירים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByPriceRange_ReturnsFilteredList()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Mid Item", Price = 50, CategoryId = 1, IsDeleted = false }
            };
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    10, 60, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((products, products.Count));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, 10, 60, null, null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.All(items, p => Assert.InRange(p.Price, 10, 60));
        }

        /// <summary>
        /// בדיקה: סינון לפי טווח מחירים שאין בו מוצרים
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByPriceRangeNoResults_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    500, 1000, null, null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, 500, 1000, null, null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
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
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple Juice", Price = 12, CategoryId = 1, IsDeleted = false },
                new Product { ProductId = 2, ProductName = "Apple Cider", Price = 18, CategoryId = 1, IsDeleted = false }
            };
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, "Apple", null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((products, products.Count));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, "Apple", null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            Assert.All(items, p => Assert.Contains("Apple", p.ProductName));
        }

        /// <summary>
        /// בדיקה: חיפוש מוצר לפי שם שלא קיים
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_FilterByNonExistingName_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository
                .Setup(x => x.GetProductsByConditions(
                    It.IsAny<int>(), It.IsAny<int>(),
                    null, null, "XYZXYZ", null, It.IsAny<int?[]>(), null))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(1, int.MaxValue, null, null, "XYZXYZ", null, Array.Empty<int?>(), null);

            // Assert
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        #endregion
    }
}
