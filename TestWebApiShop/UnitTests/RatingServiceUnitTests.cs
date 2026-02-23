using Xunit;
using Moq;
using Services;
using Repositories;
using Entities;

namespace TestWebApiShop.UnitTests
{
    public class RatingServiceUnitTests
    {
        private readonly Mock<IRatingRepository> _mockRatingRepository;
        private readonly RatingService _ratingService;

        public RatingServiceUnitTests()
        {
            _mockRatingRepository = new Mock<IRatingRepository>();
            _ratingService = new RatingService(_mockRatingRepository.Object);
        }

        #region AddRating Tests

        /// <summary>
        /// בדיקה: הוספת דירוג דרך השירות
        /// Path: HAPPY - צריך לקרוא ל-Repository ולהחזיר את הדירוג
        /// </summary>
        [Fact]
        public async Task AddRating_ValidRating_ReturnsAddedRating()
        {
            // Arrange
            var rating = new Rating
            {
                RatingId = 1,
                Host = "localhost",
                Method = "GET",
                Path = "/api/categories",
                Referer = "",
                UserAgent = "PostmanRuntime/7.28.4",
                RecordDate = DateTime.Now
            };

            _mockRatingRepository.Setup(x => x.AddRating(It.IsAny<Rating>())).ReturnsAsync(rating);

            // Act
            var result = await _ratingService.AddRating(rating);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.RatingId);
            Assert.Equal("localhost", result.Host);
            Assert.Equal("GET", result.Method);
            
            // Verify repository was called exactly once
            _mockRatingRepository.Verify(x => x.AddRating(It.Is<Rating>(r => r.Host == "localhost")), Times.Once);
        }

        /// <summary>
        /// בדיקה: הוספת דירוג ריק
        /// Path: HAPPY - צריך להעביר את הדירוג ל-Repository ולהחזיר את התוצאה
        /// </summary>
        [Fact]
        public async Task AddRating_EmptyRating_ReturnsAddedRating()
        {
            // Arrange
            var rating = new Rating();
            
            _mockRatingRepository.Setup(x => x.AddRating(It.IsAny<Rating>())).ReturnsAsync(rating);

            // Act
            var result = await _ratingService.AddRating(rating);

            // Assert
            Assert.NotNull(result);
            _mockRatingRepository.Verify(x => x.AddRating(It.IsAny<Rating>()), Times.Once);
        }

        #endregion
    }
}
