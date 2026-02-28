using Xunit;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop.UnitTests
{
    public class UserServiceUnitTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceUnitTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _userService = new UserService(_mockUserRepository.Object, _mockPasswordService.Object, _mapper);
        }

        #region GetUserById Tests

        /// <summary>
        /// בדיקה: קבלת משתמש עם ID תקין
        /// Path: HAPPY - צריך להחזיר את המשתמש
        /// </summary>
        [Fact]
        public async Task GetUserById_WithValidId_ReturnsUserDTO()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "pass" };
            _mockUserRepository.Setup(x => x.GetUserById(1)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
        }

        /// <summary>
        /// בדיקה: קבלת משתמש עם ID לא קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetUserById(999)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserById(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddUserToFile Tests

        /// <summary>
        /// בדיקה: הוספת משתמש חדש עם סיסמה חזקה
        /// Path: HAPPY - צריך להוסיף את המשתמש בהצלחה
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithStrongPassword_AddsUserSuccessfully()
        {
            // Arrange
            var userDTO = new UserDTO(0, "jane@test.com", "Jane", "Doe", "StrongPass123!");
            var user = new User { UserId = 1, UserName = "jane@test.com", FirstName = "Jane", LastName = "Doe", Password = "StrongPass123!" };
            _mockPasswordService.Setup(x => x.PasswordHardness("StrongPass123!")).Returns(new Password { Level = 4 });
            _mockUserRepository.Setup(x => x.AddUserToFile(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.AddUserToFile(userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane", result.FirstName);
        }

        /// <summary>
        /// בדיקה: ניסיון הוספת משתמש עם סיסמה חלשה (Level < 3)
        /// Path: UNHAPPY - צריך להחזיר null כי הסיסמה לא מספיק חזקה
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithWeakPassword_ReturnsNull()
        {
            // Arrange
            var userDTO = new UserDTO(0, "bob@test.com", "Bob", "Smith", "weak");
            _mockPasswordService.Setup(x => x.PasswordHardness("weak")).Returns(new Password { Level = 0 });

            // Act
            var result = await _userService.AddUserToFile(userDTO);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Loginto Tests

        /// <summary>
        /// בדיקה: התחברות עם שם משתמש וסיסמה נכונים
        /// Path: HAPPY - צריך להחזיר את פרטי המשתמש
        /// </summary>
        [Fact]
        public async Task Loginto_WithValidCredentials_ReturnsUserDTO()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("john@test.com", "password");
            var user = new User { UserId = 1, UserName = "john@test.com", FirstName = "John", LastName = "Doe", Password = "password" };
            _mockUserRepository.Setup(x => x.Loginto(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.Loginto(loginDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
        }

        /// <summary>
        /// בדיקה: ניסיון התחברות עם פרטים לא נכונים
        /// Path: UNHAPPY - צריך להחזיר null כשלא קיים משתמש או הסיסמה לא נכונה
        /// </summary>
        [Fact]
        public async Task Loginto_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("invalid@test.com", "wrong");
            _mockUserRepository.Setup(x => x.Loginto(It.IsAny<User>())).ReturnsAsync((User)null);

            // Act
            var result = await _userService.Loginto(loginDTO);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAllUsers Tests

        /// <summary>
        /// בדיקה: קבלת כל המשתמשים כשיש מספר משתמשים בבסיס
        /// Path: HAPPY - צריך להחזיר רשימה עם כל המשתמשים
        /// </summary>
        [Fact]
        public async Task GetAllUsers_WithMultipleUsers_ReturnsList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, UserName = "user1@test.com", FirstName = "User1", LastName = "One", Password = "pass" },
                new User { UserId = 2, UserName = "user2@test.com", FirstName = "User2", LastName = "Two", Password = "pass" }
            };
            _mockUserRepository.Setup(x => x.GetAllUsers()).ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// בדיקה: קבלת כל המשתמשים כשאין משתמשים בבסיס
        /// Path: UNHAPPY - צריך להחזיר רשימה ריקה
        /// </summary>
        [Fact]
        public async Task GetAllUsers_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            _mockUserRepository.Setup(x => x.GetAllUsers()).ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region UpdateUserDetails Tests

        /// <summary>
        /// בדיקה: עדכון פרטי משתמש עם סיסמה חזקה
        /// Path: HAPPY - צריך לעדכן את המשתמש בהצלחה
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithStrongPassword_UpdatesSuccessfully()
        {
            // Arrange
            var userDTO = new UserDTO(1, "updated@test.com", "Updated", "User", "NewPass123!");
            _mockPasswordService.Setup(x => x.PasswordHardness("NewPass123!")).Returns(new Password { Level = 4 });
            _mockUserRepository.Setup(x => x.UpdateUserDetails(1, It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserDetails(1, userDTO);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// בדיקה: ניסיון עדכון משתמש עם סיסמה חלשה
        /// Path: UNHAPPY - צריך להחזיר null כי הסיסמה לא מספיק חזקה
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithWeakPassword_ReturnsNull()
        {
            // Arrange
            var userDTO = new UserDTO(1, "updated@test.com", "Updated", "User", "weak");
            _mockPasswordService.Setup(x => x.PasswordHardness("weak")).Returns(new Password { Level = 0 });

            // Act
            var result = await _userService.UpdateUserDetails(1, userDTO);

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
