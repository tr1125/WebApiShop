using Xunit;
using FluentAssertions;
using Services;
using Entities;

namespace TestWebApiShop
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service = new PasswordService();

        #region Happy Path Tests

        [Fact]
        public void PasswordHardness_StrongPassword_ReturnsHighScore()
        {
            // Arrange
            var password = "S0m3_Very$trongP@ssw0rd!";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().BeGreaterThanOrEqualTo(3);
            result.Name.Should().Be(password);
        }

        [Fact]
        public void PasswordHardness_MediumPassword_ReturnsMediumScore()
        {
            // Arrange
            var password = "MyPassword123";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(password);
            result.Level.Should().BeGreaterThanOrEqualTo(1);
        }

        #endregion

        #region Unhappy Path Tests

        [Fact]
        public void PasswordHardness_WeakPassword_ReturnsLowScore()
        {
            // Arrange
            var password = "12345";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().BeInRange(0, 1);
            result.Name.Should().Be(password);
        }

        [Fact]
        public void PasswordHardness_VeryWeakPassword_ReturnsZeroScore()
        {
            // Arrange
            var password = "abc";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(0);
            result.Name.Should().Be(password);
        }

        [Fact]
        public void PasswordHardness_EmptyPassword_ReturnsLowScore()
        {
            // Arrange
            var password = "";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(0);
            result.Name.Should().Be(password);
        }

        [Fact]
        public void PasswordHardness_SingleCharacter_ReturnsLowScore()
        {
            // Arrange
            var password = "A";

            // Act
            var result = _service.PasswordHardness(password);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(0);
        }

        #endregion
    }
}
