using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Services;
using Entities;
using WebApiShop.Controllers;

namespace TestWebApiShop
{
    public class PasswordControllerTests
    {
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly PasswordController _controller;

        public PasswordControllerTests()
        {
            _mockPasswordService = new Mock<IPasswordService>();
            _controller = new PasswordController(_mockPasswordService.Object);
        }

        #region Happy Path Tests

        [Fact]
        public void Post_WithStrongPassword_ReturnsPasswordObject()
        {
            // Arrange
            var password = "S0m3_Strong@Pass123!";
            var passwordResult = new Password { Name = password, Level = 4 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(4);
            result.Name.Should().Be(password);
        }

        [Fact]
        public void Post_WithMediumPassword_ReturnsPasswordObjectWithMediumLevel()
        {
            // Arrange
            var password = "MyPassword123";
            var passwordResult = new Password { Name = password, Level = 2 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().BeInRange(1, 3);
            result.Name.Should().Be(password);
        }

        #endregion

        #region Unhappy Path Tests

        [Fact]
        public void Post_WithWeakPassword_ReturnsPasswordObjectWithLowLevel()
        {
            // Arrange
            var password = "12345";
            var passwordResult = new Password { Name = password, Level = 0 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(0);
        }

        [Fact]
        public void Post_WithEmptyPassword_ReturnsPasswordObjectWithZeroLevel()
        {
            // Arrange
            var password = "";
            var passwordResult = new Password { Name = password, Level = 0 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(0);
            result.Name.Should().Be("");
        }

        [Fact]
        public void Post_WithBoundaryPassword_ReturnsPasswordObject()
        {
            // Arrange
            var password = "aBc1!@#";
            var passwordResult = new Password { Name = password, Level = 2 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(password);
        }

        [Fact]
        public void Post_WithVeryLongPassword_ReturnsPasswordObject()
        {
            // Arrange
            var password = new string('A', 500) + "1!@";
            var passwordResult = new Password { Name = password, Level = 3 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            var result = _controller.Post(password);

            // Assert
            result.Should().NotBeNull();
            result.Name.Length.Should().BeGreaterThan(100);
        }

        #endregion

        #region Service Call Verification

        [Fact]
        public void Post_CallsPasswordServiceExactlyOnce()
        {
            // Arrange
            var password = "TestPass123!";
            var passwordResult = new Password { Name = password, Level = 3 };
            _mockPasswordService.Setup(s => s.PasswordHardness(password))
                .Returns(passwordResult);

            // Act
            _controller.Post(password);

            // Assert
            _mockPasswordService.Verify(s => s.PasswordHardness(password), Times.Once);
        }

        [Fact]
        public void Post_ServiceCalledWithCorrectPassword()
        {
            // Arrange
            var password = "CorrectPass123!";
            var passwordResult = new Password { Name = password, Level = 3 };
            _mockPasswordService.Setup(s => s.PasswordHardness(It.IsAny<string>()))
                .Returns(passwordResult);

            // Act
            _controller.Post(password);

            // Assert
            _mockPasswordService.Verify(s => s.PasswordHardness(password), Times.Once);
            _mockPasswordService.VerifyNoOtherCalls();
        }

        #endregion
    }
}
