using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly IMapper _mapper;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mapper = TestHelper.CreateMapper();
            _service = new ProductService(_mockProductRepository.Object, _mapper);
        }

        #region GetProductsByConditions Happy Path Tests

        [Fact]
        public async Task GetProductsByConditions_WithValidFilters_ReturnsProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Coffee", Price = 5.99, CategoryId = 1, Description = "Arabica" },
                new Product { ProductId = 2, ProductName = "Tea", Price = 3.99, CategoryId = 2, Description = "Green tea" }
            };
            var totalCount = 2;

            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?[]>()))
                .ReturnsAsync((products, totalCount));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, null, null, null, null, null);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.Should().Contain(p => p.ProductName == "Coffee");
            result.Items.Should().Contain(p => p.ProductName == "Tea");
        }

        [Fact]
        public async Task GetProductsByConditions_WithPriceFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Coffee", Price = 5.99, CategoryId = 1, Description = "Arabica" }
            };

            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                0, 10, 5.0, 10.0, null, null, null))
                .ReturnsAsync((products, 1));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, 5.0, 10.0, null, null, null);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductName.Should().Be("Coffee");
            result.Items.First().Price.Should().Be(5.99);
        }

        [Fact]
        public async Task GetProductsByConditions_WithNameFilter_ReturnsMatchingProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Coffee", Price = 5.99, CategoryId = 1, Description = "Arabica" }
            };

            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                0, 10, null, null, "Coffee", null, null))
                .ReturnsAsync((products, 1));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, null, null, "Coffee", null, null);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductName.Should().Be("Coffee");
        }

        [Fact]
        public async Task GetProductsByConditions_WithCategoryFilter_ReturnsProductsByCategory()
        {
            // Arrange
            var categoryIds = new int?[] { 1 };
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Coffee", Price = 5.99, CategoryId = 1, Description = "Arabica" }
            };

            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                0, 10, null, null, null, null, categoryIds))
                .ReturnsAsync((products, 1));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, null, null, null, null, categoryIds);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().CategoryId.Should().Be(1);
        }

        #endregion

        #region GetProductsByConditions Unhappy Path Tests

        [Fact]
        public async Task GetProductsByConditions_NoMatchingProducts_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<double?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?[]>()))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, null, null, "Nonexistent", null, null);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetProductsByConditions_WithNoPriceMatch_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                0, 10, 100.0, 200.0, null, null, null))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, 100.0, 200.0, null, null, null);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetProductsByConditions_WithInvalidCategoryId_ReturnsEmptyList()
        {
            // Arrange
            var categoryIds = new int?[] { 999 };
            _mockProductRepository.Setup(pr => pr.GetProductsByConditions(
                0, 10, null, null, null, null, categoryIds))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var result = await _service.GetProductsByConditions(0, 10, null, null, null, null, categoryIds);

            // Assert
            result.Items.Should().BeEmpty();
        }

        #endregion
    }
}
