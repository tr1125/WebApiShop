using Xunit;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

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
            _productService = new ProductService(_mockProductRepository.Object, _mapper);
        }

        #region GetProductsByConditions Tests

        /// <summary>
        /// בדיקה: קבלת מוצרים בדפס ניטר (ללא פילטר)
        /// Path: HAPPY - צריך להחזיר את כל המוצרים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithValidFilters_ReturnsProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple", Price = 10, CategoryId = 1, Description = "Red Apple" },
                new Product { ProductId = 2, ProductName = "Banana", Price = 5, CategoryId = 1, Description = "Yellow Banana" }
            };
            _mockProductRepository.Setup(x => x.GetProductsByConditions(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?[]>()
            )).ReturnsAsync((products, 2));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(0, 10, null, null, null, null, null);

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Equal(2, total);
        }

        /// <summary>
        /// בדיקה: קבלת מוצרים עם פילטר מחיר (min price)
        /// Path: HAPPY - צריך להחזיר את המוצרים מעל המחיר
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithPriceFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Expensive", Price = 100, CategoryId = 1, Description = "High Price" }
            };
            _mockProductRepository.Setup(x => x.GetProductsByConditions(0, 10, 50, 150, null, null, null))
                .ReturnsAsync((products, 1));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(0, 10, 50, 150, null, null, null);

            // Assert
            Assert.Single(items);
            Assert.Equal(100, items[0].Price);
        }

        /// <summary>
        /// בדיקה: קבלת מוטריים עב שם
        /// Path: HAPPY - צריך להחזיר את המוטרים הטבה דפטנט בשם
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNameFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Apple", Price = 10, CategoryId = 1, Description = "Fruit" }
            };
            _mockProductRepository.Setup(x => x.GetProductsByConditions(0, 10, null, null, "Apple", null, null))
                .ReturnsAsync((products, 1));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(0, 10, null, null, "Apple", null, null);

            // Assert
            Assert.Single(items);
            Assert.Equal("Apple", items[0].ProductName);
        }

        /// <summary>
        /// בדיקה: קבלת מוצרים עב הדרגה
        /// Path: HAPPY - צריך להחזיר את המוטריים בדרגה מנא
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithCategoryFilter_ReturnsByCategory()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Orange", Price = 8, CategoryId = 2, Description = "Citrus" }
            };
            var categoryIds = new int?[] { 2 };
            _mockProductRepository.Setup(x => x.GetProductsByConditions(0, 10, null, null, null, null, categoryIds))
                .ReturnsAsync((products, 1));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(0, 10, null, null, null, null, categoryIds);

            // Assert
            Assert.Single(items);
            Assert.Equal(2, items[0].CategoryId);
        }

        /// <summary>
        /// בדיקה: קבלת מוצרים שאט אט קיימה לטטיחה
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithNoResults_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository.Setup(x => x.GetProductsByConditions(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?[]>()
            )).ReturnsAsync((new List<Product>(), 0));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(0, 10, 1000, 2000, null, null, null);

            // Assert
            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        /// <summary>
        /// בדיקה: קבלת מוצרים עם ביצוע (offset ו limit)
        /// Path: HAPPY - צריך להחזיר את הדף השניה של המוצרים
        /// </summary>
        [Fact]
        public async Task GetProductsByConditions_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 11, ProductName = "Product11", Price = 50, CategoryId = 1, Description = "Page2" }
            };
            _mockProductRepository.Setup(x => x.GetProductsByConditions(10, 10, null, null, null, null, null))
                .ReturnsAsync((products, 100));

            // Act
            var (items, total) = await _productService.GetProductsByConditions(10, 10, null, null, null, null, null);

            // Assert
            Assert.Single(items);
            Assert.Equal(100, total);
        }

        #endregion
    }
}
