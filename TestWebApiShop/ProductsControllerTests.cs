using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using WebApiShop.Controllers;

namespace TestWebApiShop
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductsController(_mockProductService.Object);
        }

        [Fact]
        public async Task Get_WithValidFilters_ReturnsProductList()
        {
            // Arrange
            var products = new List<ProductDTO>
            {
                new ProductDTO(1, "Coffee", 5.99, 1, "Arabica"),
                new ProductDTO(2, "Tea", 3.99, 2, "Green tea")
            };
            _mockProductService.Setup(s => s.GetProductsByConditions(0, 10, null, null, null, null, null))
                .ReturnsAsync((products, 2));

            // Act
            var result = await _controller.Get(0, null, null, null, null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Get_WithNoMatchingProducts_ReturnsEmptyList()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetProductsByConditions(0, 10, null, null, "Nonexistent", null, null))
                .ReturnsAsync((new List<ProductDTO>(), 0));

            // Act
            var result = await _controller.Get(0, null, null, "Nonexistent", null, null);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Get_WithPriceFilterMinMax_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<ProductDTO>
            {
                new ProductDTO(1, "Coffee", 5.99, 1, "Arabica")
            };
            _mockProductService.Setup(s => s.GetProductsByConditions(0, 10, 5.0, 10.0, null, null, null))
                .ReturnsAsync((products, 1));

            // Act
            var result = await _controller.Get(0, 5.0, 10.0, null, null, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Price.Should().BeInRange(5.0, 10.0);
        }

        [Fact]
        public async Task Get_WithCategoryFilter_ReturnsProductsByCategory()
        {
            // Arrange
            var categoryIds = new int?[] { 1 };
            var products = new List<ProductDTO>
            {
                new ProductDTO(1, "Coffee", 5.99, 1, "Arabica")
            };
            _mockProductService.Setup(s => s.GetProductsByConditions(0, 10, null, null, null, null, categoryIds))
                .ReturnsAsync((products, 1));

            // Act
            var result = await _controller.Get(0, null, null, null, null, categoryIds);

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task Get_WithInvalidPriceRange_ReturnsEmptyList()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetProductsByConditions(0, 10, 100.0, 200.0, null, null, null))
                .ReturnsAsync((new List<ProductDTO>(), 0));

            // Act
            var result = await _controller.Get(0, 100.0, 200.0, null, null, null);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
