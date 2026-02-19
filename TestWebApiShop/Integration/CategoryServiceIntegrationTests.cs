using Xunit;
using FluentAssertions;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;
using Microsoft.EntityFrameworkCore;

namespace TestWebApiShop.Integration
{
    public class CategoryServiceIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly IMapper _mapper;
        private readonly CategoryRepository _repository;
        private readonly CategoryService _service;

        public CategoryServiceIntegrationTests()
        {
            // Setup In-Memory Database
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new WebApiShopContext(options);
            _mapper = TestHelper.CreateMapper();
            _repository = new CategoryRepository(_dbContext);
            _service = new CategoryService(_repository, _mapper);
        }

        private void Dispose()
        {
            _dbContext?.Dispose();
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

            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.CategoryName == "Beverages");
            result.Should().Contain(c => c.CategoryName == "Snacks");
            result.Should().Contain(c => c.CategoryName == "Sweets");

            Dispose();
        }

        [Fact]
        public async Task GetAllCategories_WithOneCategory_ReturnsSingleItem()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Beverages" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryName.Should().Be("Beverages");

            Dispose();
        }

        [Fact]
        public async Task GetAllCategories_CorrectlyMapsEntitiesToDTOs()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Electronics" },
                new Category { CetegoryId = 2, CategoryName = "Books" }
            };

            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().AllSatisfy(dto =>
            {
                dto.Should().BeOfType<CategoryDTO>();
                dto.CategoryName.Should().NotBeNullOrEmpty();
            });

            Dispose();
        }

        #endregion

        #region GetAllCategories Unhappy Path Tests

        [Fact]
        public async Task GetAllCategories_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange - No categories added to database

            // Act
            var result = await _service.GetAllCategories();

            // Assert
            result.Should().BeEmpty();
            result.Should().NotBeNull();

            Dispose();
        }

        [Fact]
        public async Task GetAllCategories_DataPersistsAcrossMultipleCalls()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Persistent" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var firstCall = await _service.GetAllCategories();
            var secondCall = await _service.GetAllCategories();

            // Assert
            firstCall.Should().HaveCount(1);
            secondCall.Should().HaveCount(1);
            firstCall.First().CategoryName.Should().Be(secondCall.First().CategoryName);

            Dispose();
        }

        #endregion

        #region Repository Direct Tests

        [Fact]
        public async Task Repository_SavesAndRetrieves_Category()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Direct Test" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var categories = await _repository.GetAllCategories();

            // Assert
            categories.Should().HaveCount(1);
            categories.First().CategoryName.Should().Be("Direct Test");

            Dispose();
        }

        [Fact]
        public async Task Repository_HandlesEmptyDatabase_Correctly()
        {
            // Arrange - Empty database

            // Act
            var categories = await _repository.GetAllCategories();

            // Assert
            categories.Should().BeEmpty();

            Dispose();
        }

        #endregion
    }
}
