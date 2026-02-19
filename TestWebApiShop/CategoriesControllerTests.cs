using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using WebApiShop.Controllers;

namespace TestWebApiShop
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoriesController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task Get_WithMultipleCategories_ReturnsOkWithList()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO(1, "Beverages"),
                new CategoryDTO(2, "Snacks"),
                new CategoryDTO(3, "Sweets")
            };
            _mockCategoryService.Setup(s => s.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.CategoryName == "Beverages");
            result.Should().Contain(c => c.CategoryName == "Snacks");
        }

        [Fact]
        public async Task Get_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange
            _mockCategoryService.Setup(s => s.GetAllCategories())
                .ReturnsAsync(new List<CategoryDTO>());

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().BeEmpty();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_WithSingleCategory_ReturnsListWithOneItem()
        {
            // Arrange
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO(1, "Beverages")
            };
            _mockCategoryService.Setup(s => s.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryName.Should().Be("Beverages");
        }

        [Fact]
        public async Task Get_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            _mockCategoryService.Setup(s => s.GetAllCategories())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Get());
        }
    }
}
