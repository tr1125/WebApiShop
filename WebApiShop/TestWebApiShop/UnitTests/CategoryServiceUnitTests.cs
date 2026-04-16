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
    public class CategoryServiceUnitTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly IMapper _mapper;
        private readonly CategoryService _categoryService;

        public CategoryServiceUnitTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _categoryService = new CategoryService(_mockCategoryRepository.Object, _mapper, NullLogger<CategoryService>.Instance);
        }

        #region GetAllCategories Tests

        /// <summary>
        /// בדיקה: קבלת כל ההזמנות כשיש מספר קטגוריות
        /// Path: HAPPY - צריך להחזיר רשימה של הקטגוריות
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithMultipleCategories_ReturnsList()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Beverages" },
                new Category { CetegoryId = 2, CategoryName = "Snacks" },
                new Category { CetegoryId = 3, CategoryName = "Desserts" }
            };
            _mockCategoryRepository.Setup(x => x.GetAllCategories()).ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategories();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Beverages", result[0].CategoryName);
        }

        /// <summary>
        /// בדיקה: קבלת כל ההזמנות כשיש קטגוריה אחת
        /// Path: HAPPY - צריך להחזיר קטגוריה אחת
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithSingleCategory_ReturnsSingleItem()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Electronics" }
            };
            _mockCategoryRepository.Setup(x => x.GetAllCategories()).ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategories();

            // Assert
            Assert.Single(result);
            Assert.Equal("Electronics", result[0].CategoryName);
        }

        /// <summary>
        /// בדיקה: קבלת כל הקטגוריות כשאין קטגוריות
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange
            _mockCategoryRepository.Setup(x => x.GetAllCategories()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _categoryService.GetAllCategories();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// בדיקה: המרת ישויות ל-DTOs
        /// Path: HAPPY - צריך שכל ישות תמופה ל-DTO נכון
        /// </summary>
        [Fact]
        public async Task GetAllCategories_MapsEntitiesToDTOs()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CetegoryId = 1, CategoryName = "Books" },
                new Category { CetegoryId = 2, CategoryName = "Toys" }
            };
            _mockCategoryRepository.Setup(x => x.GetAllCategories()).ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllCategories();

            // Assert
            Assert.All(result, dto => Assert.IsType<CategoryDTO>(dto));
            Assert.All(result, dto => Assert.NotNull(dto.CategoryName));
        }

        #endregion
    }
}
