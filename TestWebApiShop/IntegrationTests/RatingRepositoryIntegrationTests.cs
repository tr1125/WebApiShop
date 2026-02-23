using Xunit;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities;

namespace TestWebApiShop.IntegrationTests
{
    public class RatingRepositoryIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly RatingRepository _ratingRepository;

        public RatingRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new WebApiShopContext(options);
            _ratingRepository = new RatingRepository(_dbContext);
        }

        private void CleanUp() => _dbContext?.Dispose();

        #region AddRating Tests

        /// <summary>
        /// בדיקה: הוספת דירוג חדש למסד הנתונים
        /// Path: HAPPY - צריך להוסיף את הדירוג ולהחזיר אותו
        /// </summary>
        [Fact]
        public async Task AddRating_ValidRating_ReturnsAddedRating()
        {
            // Arrange
            var rating = new Rating
            {
                Host = "localhost",
                Method = "GET",
                Path = "/api/products",
                Referer = "http://localhost:3000",
                UserAgent = "Mozilla/5.0",
                RecordDate = DateTime.Now
            };

            // Act
            var result = await _ratingRepository.AddRating(rating);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("localhost", result.Host);
            Assert.Equal("GET", result.Method);
            Assert.Equal("/api/products", result.Path);
            
            // Verify it was added to the database
            var dbRating = await _dbContext.Rating.FirstOrDefaultAsync(r => r.RatingId == result.RatingId);
            Assert.NotNull(dbRating);
            Assert.Equal("localhost", dbRating.Host);
            
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת מספר דירוגים למסד הנתונים
        /// Path: HAPPY - צריך לשמור את כל הדירוגים בהצלחה
        /// </summary>
        [Fact]
        public async Task AddRating_MultipleRatings_SavesAllToDatabase()
        {
            // Arrange
            var rating1 = new Rating { Host = "host1", Method = "GET", Path = "/api/1", Referer = "ref1", UserAgent = "ua1", RecordDate = DateTime.Now };
            var rating2 = new Rating { Host = "host2", Method = "POST", Path = "/api/2", Referer = "ref2", UserAgent = "ua2", RecordDate = DateTime.Now };

            // Act
            await _ratingRepository.AddRating(rating1);
            await _ratingRepository.AddRating(rating2);

            // Assert
            var count = await _dbContext.Rating.CountAsync();
            Assert.Equal(2, count);

            CleanUp();
        }

        #endregion
    }
}
