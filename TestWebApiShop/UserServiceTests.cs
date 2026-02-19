using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly IMapper _mapper;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            
            _mapper = TestHelper.CreateMapper();
            
            _service = new UserService(_mockUserRepository.Object, _mockPasswordService.Object, _mapper);
        }

        #region AddUserToFile Tests

        [Fact]
        public async Task AddUserToFile_WithStrongPassword_ReturnsUserDTO()
        {
            // Arrange
            var userDTO = new UserDTO(0, "test@example.com", "John", "Doe", "P@ssw0rd123!");
            var passwordEntity = new Password { Name = "P@ssw0rd123!", Level = 4 };
            var userEntity = new User { UserId = 1, UserName = "test@example.com", FirstName = "John", LastName = "Doe", Password = "P@ssw0rd123!" };

            _mockPasswordService.Setup(ps => ps.PasswordHardness(userDTO.Password))
                .Returns(passwordEntity);
            _mockUserRepository.Setup(ur => ur.AddUserToFile(It.IsAny<User>()))
                .ReturnsAsync(userEntity);

            // Act
            var result = await _service.AddUserToFile(userDTO);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("test@example.com");
            result.FirstName.Should().Be("John");
            _mockUserRepository.Verify(ur => ur.AddUserToFile(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AddUserToFile_WithWeakPassword_ReturnsNull()
        {
            // Arrange
            var userDTO = new UserDTO(0, "test@example.com", "John", "Doe", "12345");
            var passwordEntity = new Password { Name = "12345", Level = 1 };

            _mockPasswordService.Setup(ps => ps.PasswordHardness("12345"))
                .Returns(passwordEntity);

            // Act
            var result = await _service.AddUserToFile(userDTO);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(ur => ur.AddUserToFile(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsUserDTO()
        {
            // Arrange
            var userId = 1;
            var userEntity = new User { UserId = userId, UserName = "test@example.com", FirstName = "John", LastName = "Doe" };
            
            _mockUserRepository.Setup(ur => ur.GetUserById(userId))
                .ReturnsAsync(userEntity);

            // Act
            var result = await _service.GetUserById(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.UserName.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(ur => ur.GetUserById(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _service.GetUserById(userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Loginto Tests

        [Fact]
        public async Task Loginto_WithValidCredentials_ReturnsUserDTO()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("test@example.com", "P@ssw0rd123!");
            var userEntity = new User { UserId = 1, UserName = "test@example.com", FirstName = "John", LastName = "Doe" };

            _mockUserRepository.Setup(ur => ur.Loginto(It.IsAny<User>()))
                .ReturnsAsync(userEntity);

            // Act
            var result = await _service.Loginto(loginDTO);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("test@example.com");
        }

        [Fact]
        public async Task Loginto_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("invalid@example.com", "wrongpassword");

            _mockUserRepository.Setup(ur => ur.Loginto(It.IsAny<User>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _service.Loginto(loginDTO);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_WithMultipleUsers_ReturnsList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, UserName = "user1@example.com", FirstName = "User", LastName = "One" },
                new User { UserId = 2, UserName = "user2@example.com", FirstName = "User", LastName = "Two" }
            };

            _mockUserRepository.Setup(ur => ur.GetAllUsers())
                .ReturnsAsync(users);

            // Act
            var result = await _service.GetAllUsers();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.UserName == "user1@example.com");
            result.Should().Contain(u => u.UserName == "user2@example.com");
        }

        [Fact]
        public async Task GetAllUsers_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            _mockUserRepository.Setup(ur => ur.GetAllUsers())
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _service.GetAllUsers();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region UpdateUserDetails Tests

        [Fact]
        public async Task UpdateUserDetails_WithStrongPassword_ReturnsUpdatedUserDTO()
        {
            // Arrange
            var userId = 1;
            var userDTO = new UserDTO(userId, "test@example.com", "John", "Updated", "NewP@ss123!");
            var passwordEntity = new Password { Name = "NewP@ss123!", Level = 4 };

            _mockPasswordService.Setup(ps => ps.PasswordHardness("NewP@ss123!"))
                .Returns(passwordEntity);
            _mockUserRepository.Setup(ur => ur.UpdateUserDetails(userId, It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateUserDetails(userId, userDTO);

            // Assert
            result.Should().NotBeNull();
            result.LastName.Should().Be("Updated");
            _mockUserRepository.Verify(ur => ur.UpdateUserDetails(userId, It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateUserDetails_WithWeakPassword_ReturnsNull()
        {
            // Arrange
            var userId = 1;
            var userDTO = new UserDTO(userId, "test@example.com", "John", "Doe", "weak");
            var passwordEntity = new Password { Name = "weak", Level = 0 };

            _mockPasswordService.Setup(ps => ps.PasswordHardness("weak"))
                .Returns(passwordEntity);

            // Act
            var result = await _service.UpdateUserDetails(userId, userDTO);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(ur => ur.UpdateUserDetails(It.IsAny<int>(), It.IsAny<User>()), Times.Never);
        }

        #endregion
    }
}
