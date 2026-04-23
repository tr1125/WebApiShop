using Xunit;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities;

namespace TestWebApiShop.IntegrationTests
{
    public class UserRepositoryIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly UserRepository _userRepository;

        public UserRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new WebApiShopContext(options);
            _userRepository = new UserRepository(_dbContext);
        }

        private void CleanUp() => _dbContext?.Dispose();

        private User CreateTestUser(int id = 1, string userName = "test@example.com") => new User
        {
            UserId    = id,
            UserName  = userName,
            FirstName = "Israel",
            LastName  = "Israeli",
            Password  = "Pass1234!",
            Address   = "Tel Aviv",
            Phone     = "050-0000000",
            IsAdmin   = false
        };

        #region AddUserToFile Tests

        /// <summary>
        /// בדיקה: הוספת משתמש חדש עם נתונים תקינים
        /// Path: HAPPY - צריך לשמור ולהחזיר את המשתמש
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithValidData_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var result = await _userRepository.AddUserToFile(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.UserName);
            Assert.Equal("Israel", result.FirstName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת שני משתמשים עם אותו UserName (מייל)
        /// Path: UNHAPPY - הrepository מחזיר null (לא זורק exception)
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithDuplicateUserName_ReturnsNull()
        {
            // Arrange
            var user1 = CreateTestUser(1, "same@example.com");
            var user2 = CreateTestUser(2, "same@example.com");
            await _userRepository.AddUserToFile(user1);

            // Act
            var result = await _userRepository.AddUserToFile(user2);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region Loginto Tests

        /// <summary>
        /// בדיקה: התחברות עם UserName וסיסמה נכונים
        /// Path: HAPPY - צריך להחזיר את המשתמש
        /// </summary>
        [Fact]
        public async Task Loginto_WithCorrectCredentials_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser(1, "login@example.com");
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var loginUser = new User { UserName = "login@example.com", Password = "Pass1234!" };

            // Act
            var result = await _userRepository.Loginto(loginUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("login@example.com", result.UserName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: התחברות עם UserName שאינו קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task Loginto_WithNonExistingUserName_ReturnsNull()
        {
            // Act
            var result = await _userRepository.Loginto(new User { UserName = "ghost@example.com", Password = "Pass1234!" });

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: התחברות עם סיסמה שגויה
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task Loginto_WithWrongPassword_ReturnsNull()
        {
            // Arrange
            var user = CreateTestUser(1, "login@example.com");
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var loginUser = new User { UserName = "login@example.com", Password = "WrongPass!" };

            // Act
            var result = await _userRepository.Loginto(loginUser);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region UpdateUserDetails Tests

        /// <summary>
        /// בדיקה: עדכון פרטי משתמש קיים
        /// Path: HAPPY - הנתונים מתעדכנים ב-DB
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithExistingUser_UpdatesData()
        {
            // Arrange
            var user = CreateTestUser();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var updatedData = new User
            {
                FirstName = "Updated",
                LastName  = "Israeli",
                Password  = "NewPass1234!",
                Address   = "Jerusalem",
                Phone     = "052-9999999"
            };

            // Act
            await _userRepository.UpdateUserDetails(user.UserId, updatedData);
            var result = await _userRepository.GetUserById(user.UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated",      result.FirstName);
            Assert.Equal("Jerusalem",    result.Address);
            Assert.Equal("NewPass1234!", result.Password);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: עדכון משתמש שאינו קיים
        /// Path: UNHAPPY - הפעולה לא זורקת exception (מתעלמת בשקט)
        /// הערה: התנהגות זו היא בעיה - מומלץ שהשירות יזרוק exception
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithNonExistingUser_DoesNotThrow()
        {
            // Arrange
            var nonExistingUser = new User
            {
                FirstName = "Fake",
                Password  = "Fake1234!"
            };

            // Act & Assert - הrepository לא זורק exception, השירות צריך לטפל בזה
            var exception = await Record.ExceptionAsync(
                () => _userRepository.UpdateUserDetails(999, nonExistingUser)
            );
            Assert.Null(exception);
            CleanUp();
        }

        #endregion
    }
}
