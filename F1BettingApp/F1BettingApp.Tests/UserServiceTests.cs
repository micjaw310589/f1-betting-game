using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;

namespace F1BettingApp.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserDto_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com", Points = 100 };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterUserAsync_AddsUser()
        {
            // Arrange
            var users = new List<User>();
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => users.Add(u));

            // Act
            await _userService.RegisterUserAsync("newuser", "new@example.com", "password");

            // Assert
            Assert.Single(users);
            Assert.Equal("newuser", users[0].Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ReturnsUserDto_WhenUserExists()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "testuser", Email = "test@example.com", Points = 100 }
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _userService.GetUserByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsTrue_WhenCredentialsAreValid()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "testuser", PasswordHash = "password", Points = 100 }
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _userService.ValidateUserAsync("testuser", "password");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateUserAsync_ReturnsFalse_WhenCredentialsAreInvalid()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "testuser", PasswordHash = "password", Points = 100 }
            };
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _userService.ValidateUserAsync("testuser", "wrongpassword");

            // Assert
            Assert.False(result);
        }
    }
}