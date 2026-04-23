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
    public class UserServiceUnitTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceUnitTests()
        {
            _mockUserRepository  = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _userService = new UserService(
                _mockUserRepository.Object,
                _mockPasswordService.Object,
                _mapper,
                NullLogger<UserService>.Instance
            );
        }

        #region AddUserToFile Tests

        /// <summary>
        /// בדיקה: רישום משתמש חדש עם מייל תקין
        /// Path: HAPPY - צריך להחזיר את המשתמש שנוצר
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithValidUserName_ReturnsUser()
        {
            // Arrange
            var userDto = new UserRequestDTO("test@example.com", "Israel", "Israeli", "Pass1234!", "Tel Aviv", "050-0000000");
            var userEntity = new User
            {
                UserId    = 1,
                UserName  = "test@example.com",
                FirstName = "Israel",
                LastName  = "Israeli",
                Password  = "Pass1234!",
                Address   = "Tel Aviv",
                Phone     = "050-0000000"
            };
            _mockPasswordService.Setup(x => x.PasswordHardness(userDto.Password))
                .Returns(new Password { Level = 3 });
            _mockUserRepository.Setup(x => x.AddUserToFile(It.IsAny<User>())).ReturnsAsync(userEntity);

            // Act
            var result = await _userService.AddUserToFile(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.UserName);
        }

        /// <summary>
        /// בדיקה: רישום משתמש עם מייל כפול
        /// Path: UNHAPPY - הrepository מחזיר null, השירות צריך לזרוק exception
        /// </summary>
        [Fact]
        public async Task AddUserToFile_WithDuplicateUserName_ThrowsException()
        {
            // Arrange
            var userDto = new UserRequestDTO("exist@example.com", "Israel", "Israeli", "Pass1234!", "Tel Aviv", "050-0000000");
            _mockPasswordService.Setup(x => x.PasswordHardness(userDto.Password))
                .Returns(new Password { Level = 3 });
            _mockUserRepository.Setup(x => x.AddUserToFile(It.IsAny<User>())).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.AddUserToFile(userDto)
            );
        }

        #endregion

        #region Loginto Tests

        /// <summary>
        /// בדיקה: התחברות עם פרטים נכונים
        /// Path: HAPPY - צריך להחזיר את המשתמש
        /// </summary>
        [Fact]
        public async Task Loginto_WithCorrectCredentials_ReturnsUser()
        {
            // Arrange
            var loginDTO   = new UserLoginDTO("user@example.com", "Pass1234!");
            var userEntity = new User { UserId = 1, UserName = "user@example.com", Password = "Pass1234!" };
            _mockUserRepository.Setup(x => x.Loginto(It.IsAny<User>())).ReturnsAsync(userEntity);

            // Act
            var result = await _userService.Loginto(loginDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user@example.com", result.UserName);
        }

        /// <summary>
        /// בדיקה: התחברות עם סיסמה שגויה
        /// Path: UNHAPPY - הrepository מחזיר null, השירות צריך לזרוק exception
        /// </summary>
        [Fact]
        public async Task Loginto_WithWrongPassword_ThrowsException()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("user@example.com", "WrongPass!");
            _mockUserRepository.Setup(x => x.Loginto(It.IsAny<User>())).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _userService.Loginto(loginDTO)
            );
        }

        #endregion

        #region UpdateUserDetails Tests

        /// <summary>
        /// בדיקה: עדכון פרטי משתמש קיים
        /// Path: HAPPY - צריך להחזיר את המשתמש המעודכן
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithExistingUser_ReturnsUpdatedUser()
        {
            // Arrange
            var updatedUserDto = new UserRequestDTO("updated@example.com", "Updated", "User", "NewPass1234!", "Jerusalem", "052-1111111");
            var updatedEntity  = new User
            {
                UserId    = 1,
                UserName  = "updated@example.com",
                FirstName = "Updated",
                LastName  = "User",
                Password  = "NewPass1234!",
                Address   = "Jerusalem",
                Phone     = "052-1111111"
            };
            _mockPasswordService.Setup(x => x.PasswordHardness(updatedUserDto.Password))
                .Returns(new Password { Level = 3 });
            _mockUserRepository.Setup(x => x.UpdateUserDetails(1, It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(x => x.GetUserById(1)).ReturnsAsync(updatedEntity);

            // Act
            var result = await _userService.UpdateUserDetails(1, updatedUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("updated@example.com", result.UserName);
        }

        /// <summary>
        /// בדיקה: עדכון משתמש שאינו קיים
        /// Path: UNHAPPY - צריך לזרוק exception
        /// </summary>
        [Fact]
        public async Task UpdateUserDetails_WithNonExistingUser_ThrowsException()
        {
            // Arrange
            var userDto = new UserRequestDTO("ghost@example.com", "Ghost", "User", "Pass1234!", "Nowhere", "000-0000000");
            _mockPasswordService.Setup(x => x.PasswordHardness(userDto.Password))
                .Returns(new Password { Level = 3 });
            _mockUserRepository.Setup(x => x.UpdateUserDetails(999, It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(x => x.GetUserById(999)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.UpdateUserDetails(999, userDto)
            );
        }

        #endregion
    }
}
