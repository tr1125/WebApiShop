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

        #region GetUserById Tests

        /// <summary>
        /// בדיקה: קבלת משתמש מהדטבש עם ID תקין
        /// Path: HAPPY - צריך להחזיר את המשתמש מהדטבש
        /// </summary>
        [Fact]
        public async Task GetUserById_WithValidId_ReturnsUser()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("john@test.com", result.UserName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת משתמש ישד דטבש עם ID שאט קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _userRepository.GetUserById(999);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region AddUserToFile Tests

        /// <summary>
        /// בדיקה: הוספת משתמש חדש לדטבש בהצלחה
        /// Path: HAPPY - צריך להוסיף חדש אחסנו יכולו בדטבש
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithNewUser_AddsUserSuccessfully()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "newuser@test.com", FirstName = "New", LastName = "User", Password = "password" };

            // Act
            var result = await _userRepository.AddUserToFile(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.UserName);
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "newuser@test.com");
            Assert.NotNull(dbUser);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: ניסיון הוספת משתמש עם טטיח שם כשכבר קיים
        /// Path: UNHAPPY - צריך להחזיר null כי זה אפנטזה
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithDuplicateUsername_ReturnsNull()
        {
            // Arrange
            var user1 = new User { UserId = 1, UserName = "duplicate@test.com", FirstName = "User1", LastName = "One", Password = "pass" };
            var user2 = new User { UserId = 2, UserName = "duplicate@test.com", FirstName = "User2", LastName = "Two", Password = "pass" };
            await _dbContext.Users.AddAsync(user1);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.AddUserToFile(user2);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region Loginto Tests

        /// <summary>
        /// בדיקה: התחברות עט שום וסיסמה קטנטיים
        /// Path: HAPPY - צריך להחזיר את המשתמש הנכון
        /// </summary>
        [Fact]
        public async Task Loginto_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password123" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var loginUser = new User { UserName = "john@test.com", Password = "password123" };

            // Act
            var result = await _userRepository.Loginto(loginUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("john@test.com", result.UserName);
            Assert.Equal("John", result.FirstName);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: התחברות עט טטיח שם (שאט קיימה)
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task Loginto_WithInvalidUsername_ReturnsNull()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password123" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var loginUser = new User { UserName = "wrong@test.com", Password = "password123" };

            // Act
            var result = await _userRepository.Loginto(loginUser);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: התחברות עט טטיח סיסמה (שטטיח כאן שם קטנטטט)
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task Loginto_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password123" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var loginUser = new User { UserName = "john@test.com", Password = "wrongpassword" };

            // Act
            var result = await _userRepository.Loginto(loginUser);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region GetAllUsers Tests

        /// <summary>
        /// בדיקה: קבלת כל המשתמשים מדטבש בכתי דטטיחו עדר קטנט
        /// Path: HAPPY - צריכה רשימה של כל המשתמשים
        /// </summary>
        [Fact]
        public async Task GetAllUsers_WithMultipleUsers_ReturnsList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, UserName = "user1@test.com", FirstName = "User", LastName = "One", Password = "pass" },
                new User { UserId = 2, UserName = "user2@test.com", FirstName = "User", LastName = "Two", Password = "pass" }
            };
            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת כל המשתמשים כשאין משתמשים
        /// Path: UNHAPPY - צריכה רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetAllUsers_WithNoUsers_ReturnsEmptyList()
        {
            // Act
            var result = await _userRepository.GetAllUsers();

            // Assert
            Assert.Empty(result);
            CleanUp();
        }

        #endregion

        #region UpdateUserDetails Tests

        /// <summary>
        /// בדיקה: עדכון פרטי משתמש ישד עם ID וטה אחסן
        /// Path: HAPPY - צריך שהפרטים אומנם בדטבש
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithValidId_UpdatesUser()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var updatedUser = new User { UserName = "johnupdated@test.com", FirstName = "Jonathan", LastName = "Smith", Password = "newpass" };

            // Act
            await _userRepository.UpdateUserDetails(1, updatedUser);

            // Assert
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == 1);
            Assert.NotNull(dbUser);
            Assert.Equal("Jonathan", dbUser.FirstName);
            Assert.Equal("newpass", dbUser.Password);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: עדכון פרטי משתמש עם ID דאט קיים
        /// Path: UNHAPPY - צריכה שיכולים בארע אט פטור טים
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithInvalidId_DoesNotUpdateAnything()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            var updatedUser = new User { UserName = "newname@test.com", FirstName = "NewName", LastName = "Smith", Password = "newpass" };

            // Act
            await _userRepository.UpdateUserDetails(999, updatedUser);

            // Assert
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == 1);
            Assert.NotNull(dbUser);
            Assert.Equal("John", dbUser.FirstName);
            Assert.Equal("password", dbUser.Password);
            CleanUp();
        }

        #endregion
    }
}
