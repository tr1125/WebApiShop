using Xunit;
using Services;
using Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestWebApiShop.UnitTests
{
    public class PasswordServiceUnitTests
    {
        private readonly PasswordService _passwordService;

        public PasswordServiceUnitTests()
        {
            _passwordService = new PasswordService(NullLogger<PasswordService>.Instance);
        }

        #region CheckPasswordStrength Tests

        /// <summary>
        /// בדיקה: סיסמה חזקה עם אותיות גדולות, מספרים ותווים מיוחדים
        /// Path: HAPPY - צריך להחזיר Password עם Level גבוה
        /// </summary>
        [Fact]
        public void CheckPasswordStrength_WithStrongPassword_ReturnsHighLevel()
        {
            // Arrange
            var password = "Secure1234!";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Level >= 3);
        }

        /// <summary>
        /// בדיקה: סיסמה קצרה מדי (פחות מ-8 תווים)
        /// Path: UNHAPPY - צריך להחזיר Password עם Level נמוך
        /// </summary>
        [Fact]
        public void CheckPasswordStrength_WithShortPassword_ReturnsLowLevel()
        {
            // Arrange
            var password = "Ab1!";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Level < 3);
        }

        /// <summary>
        /// בדיקה: סיסמה ללא תווים מיוחדים
        /// Path: UNHAPPY - צריך להחזיר Password עם Level נמוך
        /// </summary>
        [Fact]
        public void CheckPasswordStrength_WithNoSpecialChars_ReturnsLowLevel()
        {
            // Arrange
            var password = "Password123";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Level < 3);
        }

        /// <summary>
        /// בדיקה: סיסמה ללא מספרים
        /// Path: UNHAPPY - צריך להחזיר Password עם Level נמוך
        /// </summary>
        [Fact]
        public void CheckPasswordStrength_WithNoNumbers_ReturnsLowLevel()
        {
            // Arrange
            var password = "Password!@#";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Level < 3);
        }

        /// <summary>
        /// בדיקה: סיסמה ריקה
        /// Path: UNHAPPY - צריך להחזיר Password עם Level הכי נמוך
        /// </summary>
        [Fact]
        public void CheckPasswordStrength_WithEmptyPassword_ReturnsLowestLevel()
        {
            // Arrange
            var password = "";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Level);
        }

        #endregion
    }
}
