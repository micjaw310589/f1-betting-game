using F1BettingApp.Domain.Entities;
using Xunit;
using System;

namespace F1BettingApp.Tests
{
    public class UserTests
    {
        [Fact]
        public void Constructor_ValidInputs_CreatesUserObject()
        {
            // Act
            var user = new User("TestUser", "test@example.com", "passwordhash");

            // Assert
            Assert.Equal("TestUser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("passwordhash", user.PasswordHash);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_InvalidUsername_ThrowsArgumentException(string invalidUsername)
        {
            // Act & Assert
            var ex = Record.Exception(() => new User(invalidUsername, "test@example.com", "passwordhash"));
            Assert.IsType<ArgumentException>(ex);
        }

        [Theory]
        [InlineData("invalidemail")]
        [InlineData("@")]
        [InlineData("test.com")]
        public void Constructor_InvalidEmail_ThrowsArgumentException(string invalidEmail)
        {
            // Act & Assert
            var ex = Record.Exception(() => new User("user", invalidEmail, "passwordhash"));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void Constructor_NullPasswordHash_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Record.Exception(() => new User("user", "test@example.com", null));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void AddPoints_ValidPoints_IncrementsPoints()
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");
            user.Points = 500;

            // Act
            user.AddPoints(150);

            // Assert
            Assert.Equal(650, user.Points);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void AddPoints_InvalidPoints_ThrowsArgumentException(int invalidPoints)
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");

            // Act & Assert
            var ex = Record.Exception(() => user.AddPoints(invalidPoints));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void DeductPoints_SufficientBalance_DecrementsPoints()
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");
            user.Points = 1000;

            // Act
            user.DeductPoints(300);

            // Assert
            Assert.Equal(700, user.Points);
        }

        [Fact]
        public void DeductPoints_InsufficientBalance_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");
            user.Points = 100;

            // Act & Assert
            var ex = Record.Exception(() => user.DeductPoints(101));
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void HasSufficientBalance_TrueCase()
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");
            user.Points = 500;

            // Assert
            Assert.True(user.HasSufficientBalance(499));
        }

        [Fact]
        public void HasSufficientBalance_FalseCase()
        {
            // Arrange
            var user = new User("user", "test@example.com", "passwordhash");
            user.Points = 100;

            // Assert
            Assert.False(user.HasSufficientBalance(101));
        }
    }
}
