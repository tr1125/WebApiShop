using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using DTOs;
using WebApiShop.Controllers;

namespace TestWebApiShop
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
        }

        #region Get (All Users) Tests

        [Fact]
        public async Task Get_WithMultipleUsers_ReturnsOkWithList()
        {
            // Arrange
            var users = new List<UserDTO>
            {
                new UserDTO(1, "user1@example.com", "John", "Doe", "pass123"),
                new UserDTO(2, "user2@example.com", "Jane", "Smith", "pass456")
            };
            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Get_WithNoUsers_ReturnsEmptyList()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(new List<UserDTO>());

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Get (By ID) Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = 1;
            var userDTO = new UserDTO(userId, "test@example.com", "John", "Doe", "password");
            _mockUserService.Setup(s => s.GetUserById(userId))
                .ReturnsAsync(userDTO);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult?.Value.Should().Be(userDTO);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var userId = 999;
            _mockUserService.Setup(s => s.GetUserById(userId))
                .ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_WithNegativeId_ReturnsNotFound()
        {
            // Arrange
            var userId = -1;
            _mockUserService.Setup(s => s.GetUserById(userId))
                .ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Get(userId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Post (Register) Tests

        [Fact]
        public async Task Post_WithValidUser_ReturnsCreatedAtAction()
        {
            // Arrange
            var userDTO = new UserDTO(0, "newuser@example.com", "John", "Doe", "SecurePass123!");
            var createdUser = new UserDTO(1, "newuser@example.com", "John", "Doe", "SecurePass123!");
            _mockUserService.Setup(s => s.AddUserToFile(userDTO))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Post(userDTO);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult?.ActionName.Should().Be(nameof(UsersController.Get));
            createdResult?.RouteValues.Should().ContainKey("id");
            createdResult?.RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public async Task Post_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var userDTO = new UserDTO(0, "newuser@example.com", "John", "Doe", "weak");
            _mockUserService.Setup(s => s.AddUserToFile(userDTO))
                .ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Post(userDTO);

            // Assert
            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task Post_WithNullUser_ReturnsBadRequest()
        {
            // Arrange
            _mockUserService.Setup(s => s.AddUserToFile(null))
                .ReturnsAsync((UserDTO)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.Post(null));
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("user@example.com", "validpass");
            var userDTO = new UserDTO(1, "user@example.com", "John", "Doe", "validpass");
            _mockUserService.Setup(s => s.Loginto(loginDTO))
                .ReturnsAsync(userDTO);

            // Act
            var result = await _controller.Login(loginDTO);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("user@example.com", "wrongpass");
            _mockUserService.Setup(s => s.Loginto(loginDTO))
                .ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Login(loginDTO);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task Login_LogsAttendedLogin()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("user@example.com", "pass");
            _mockUserService.Setup(s => s.Loginto(loginDTO))
                .ReturnsAsync(new UserDTO(1, "user@example.com", "John", "Doe", "pass"));

            // Act
            await _controller.Login(loginDTO);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("login attempted")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Put (Update) Tests

        [Fact]
        public async Task Put_WithValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            var userDTO = new UserDTO(userId, "user@example.com", "John", "Updated", "NewPass123!");
            _mockUserService.Setup(s => s.UpdateUserDetails(userId, userDTO))
                .ReturnsAsync(userDTO);

            // Act
            var result = await _controller.Put(userId, userDTO);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Put_WithWeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var userDTO = new UserDTO(userId, "user@example.com", "John", "Doe", "weak");
            _mockUserService.Setup(s => s.UpdateUserDetails(userId, userDTO))
                .ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.Put(userId, userDTO);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badResult = result as BadRequestObjectResult;
            badResult?.Value.Should().Be("Password isn't hard enough");
        }

        #endregion
    }
}
