using Xunit;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities;

namespace TestWebApiShop.IntegrationTests
{
    public class CategoryRepositoryIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly CategoryRepository _categoryRepository;

        public CategoryRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new WebApiShopContext(options);
            _categoryRepository = new CategoryRepository(_dbContext);
        }

        private void CleanUp() => _dbContext?.Dispose();

        #region GetAllCategories Tests

        /// <summary>
        /// בדיקה: קבלת כל הדטגוריות מדטבש כשיש דטטטורט
        /// Path: HAPPY - צריג מרגגץ ןהזסראק רשימה ידטגוריים
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
            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.CategoryName == "Beverages");
            Assert.Contains(result, c => c.CategoryName == "Snacks");
            Assert.Contains(result, c => c.CategoryName == "Desserts");
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת כל הדטגוריות מדטבש כשיש דטטגוריה אחת
        /// Path: HAPPY - צריג מרגגץ ידטטטייד גדולטטט
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithSingleCategory_ReturnsSingleItem()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Electronics" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Electronics", result[0].CategoryName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת כל הדטגוריות מדטבש כשאין דטטטטיהט
        /// Path: UNHAPPY - צריך מטא רשימה ריקה אמטה
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithNoCategories_ReturnsEmptyList()
        {
            // Act
            var result = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: דמויטחוף: דטגורה שםז זטוגדטטטטט גדולטט נכוךddd
        /// Path: HAPPY - צרין דנטטטוצאה חוזקת דרשה אחרט
        /// </summary>
        [Fact]
        public async Task GetAllCategories_ReturnsCorrectData()
        {
            // Arrange
            var category = new Category { CetegoryId = 5, CategoryName = "Books" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.NotEmpty(result);
            var retrievedCategory = result.FirstOrDefault(c => c.CategoryName == "Books");
            Assert.NotNull(retrievedCategory);
            Assert.Equal(5, retrievedCategory.CetegoryId);
            Assert.Equal("Books", retrievedCategory.CategoryName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת כל הדטגוריות אג כדשרדהגא רבה (50)
        /// Path: HAPPY - צרין גדול ד רשימא ע רביום כאמזעטט דטפקדסצון
        /// </summary>
        [Fact]
        public async Task GetAllCategories_WithManyCategories_ReturnsAll()
        {
            // Arrange
            var categories = new List<Category>();
            for (int i = 1; i <= 50; i++)
            {
                categories.Add(new Category { CetegoryId = i, CategoryName = $"Category{i}" });
            }
            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.Count);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: דטטפשצונורט משקיחי ואחדטור זעטט יחזטורט
        /// Path: HAPPY - גדולטטטטט סטטשנטטטר אאטדטעם
        /// </summary>
        [Fact]
        public async Task GetAllCategories_MultipleCallsReturnConsistentData()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Toys" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var firstCall = await _categoryRepository.GetAllCategories();
            var secondCall = await _categoryRepository.GetAllCategories();

            // Assert
            Assert.Equal(firstCall.Count, secondCall.Count);
            Assert.Equal(firstCall[0].CategoryName, secondCall[0].CategoryName);
            CleanUp();
        }

        #endregion
    }
}
