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
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly IMapper _mapper;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mapper = TestHelper.CreateMapper();
            _service = new CategoryService(_mockCategoryRepository.Object, _mapper);
        }

        #region GetAllCategories Happy Path Tests

        [Fact]
        public async Task GetAllCategories_WithMultipleCategories_ReturnsList()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Beverages" },
                new Category { CetegoryId = 2, CategoryName = "Snacks" },
                new Category { CetegoryId = 3, CategoryName = "Sweets" }
            };

            _mockCategoryRepository.Setup(cr => cr.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.CategoryName == "Beverages");
            result.Should().Contain(c => c.CategoryName == "Snacks");
            result.Should().Contain(c => c.CategoryName == "Sweets");
        }

        [Fact]
        public async Task GetAllCategories_WithOneCategory_ReturnsSingleItem()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Beverages" }
            };

            _mockCategoryRepository.Setup(cr => cr.GetAllCategories())
                .ReturnsAsync(categories);

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryName.Should().Be("Beverages");
        }

        #endregion

        #region GetAllCategories Unhappy Path Tests

        [Fact]
        public async Task GetAllCategories_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange
            _mockCategoryRepository.Setup(cr => cr.GetAllCategories())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().BeEmpty();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllCategories_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockCategoryRepository.Setup(cr => cr.GetAllCategories())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllCategories());
        }

        #endregion
    }
}
