using Xunit;
using Services;
using Entities;

namespace TestWebApiShop.UnitTests
{
    public class PasswordServiceUnitTests
    {
        private readonly PasswordService _passwordService;

        public PasswordServiceUnitTests()
        {
            _passwordService = new PasswordService();
        }

        #region PasswordHardness Tests

        /// <summary>
        /// בדיקה: בדיקט ניסט חזסטם לסיסמה חזקה
        /// Path: HAPPY - צריך מטא Level גבוה (סל עדיפ)
        /// </summary>
        [Fact]
        public void PasswordHardness_WithStrongPassword_ReturnsHighLevel()
        {
            // Arrange
            string password = "StrongPassword123!@#";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.True(result.Level >= 3);
        }

        /// <summary>
        /// בדיקה: בדיקט ניסט חזקט לסיסמה בטיחה
        /// Path: HAPPY - צריך מטא Level בתחום 1-3
        /// </summary>
        [Fact]
        public void PasswordHardness_WithMediumPassword_ReturnsMediumLevel()
        {
            // Arrange
            string password = "Medium123";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.True(result.Level >= 1 && result.Level <= 3);
        }

        /// <summary>
        /// בדיקה: בדיקט ניסט קל סיסמה חדשה
        /// Path: UNHAPPY - צריך מטא Level נמוך < 3
        /// </summary>
        [Fact]
        public void PasswordHardness_WithWeakPassword_ReturnsLowLevel()
        {
            // Arrange
            string password = "weak";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.True(result.Level < 3);
        }

        /// <summary>
        /// בדיקה: בדיקט ניסט עדין בד אחדיל סיסמה
        /// Path: UNHAPPY - צריכ מטא Level = 0
        /// </summary>
        [Fact]
        public void PasswordHardness_WithVeryWeakPassword_ReturnsZeroLevel()
        {
            // Arrange
            string password = "a";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.Equal(0, result.Level);
        }

        /// <summary>
        /// בדיקה: בדיקט ניסט עם סיסמה ריקה (דריש)
        /// Path: UNHAPPY - צריכ מטא Level = 0
        /// </summary>
        [Fact]
        public void PasswordHardness_WithEmptyPassword_ReturnsZeroLevel()
        {
            // Arrange
            string password = "";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.Equal(0, result.Level);
        }

        /// <summary>
        /// בדיקה: בדיקט ניסט עם זט ודטעה בסיסמה
        /// Path: HAPPY - צריכ מטא Level גבוה (ג־3)
        /// </summary>
        [Fact]
        public void PasswordHardness_WithSpecialCharacters_ReturnsGoodLevel()
        {
            // Arrange
            string password = "MyStr0ng!@#Password";

            // Act
            var result = _passwordService.PasswordHardness(password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(password, result.Name);
            Assert.True(result.Level >= 3);
        }

        #endregion
    }
}
