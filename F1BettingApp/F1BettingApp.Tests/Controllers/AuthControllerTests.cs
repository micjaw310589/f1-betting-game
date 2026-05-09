using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup JWT configuration
            var jwtSettings = new Mock<IConfigurationSection>();
            jwtSettings.SetupGet(x => x["SecretKey"]).Returns("test-secret-key-with-at-least-32-characters");
            jwtSettings.SetupGet(x => x["Issuer"]).Returns("TestIssuer");
            jwtSettings.SetupGet(x => x["Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettings.Object);

            _controller = new AuthController(_mockUserService.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var expectedResponse = new RegisterResponseDto
            {
                IsSuccess = true,
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" }
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(new AuthResponseDto
                {
                    IsSuccess = true,
                    User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" }
                });

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<RegisterResponseDto>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.User);
            Assert.Equal("testuser", response.User.Username);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "duplicate@example.com",
                Password = "Password123!"
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var response = Assert.IsType<RegisterResponseDto>(conflictResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Contains("already exists", response.ErrorMessage);
        }

        [Fact]
        public async Task Register_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "", // Invalid
                Email = "invalid-email",
                Password = "short"
            };

            _controller.ModelState.AddModelError("Username", "Username is required");
            _controller.ModelState.AddModelError("Email", "Invalid email format");
            _controller.ModelState.AddModelError("Password", "Password must be between 8 and 100 characters");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<RegisterResponseDto>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.NotNull(response.ErrorMessage);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsTokens()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UsernameOrEmail = "test@example.com",
                Password = "Password123!"
            };

            var expectedResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" }
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.AccessToken);
            Assert.NotNull(response.RefreshToken);
            Assert.Equal("testuser", response.User.Username);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                UsernameOrEmail = "nonexistent@example.com",
                Password = "wrongpassword"
            };

            var failedResponse = new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid credentials"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid credentials", response.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                Token = "valid-access-token",
                RefreshToken = "valid-refresh-token"
            };

            var expectedResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            _mockUserService.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.NotNull(response.AccessToken);
            Assert.NotNull(response.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto
            {
                Token = "invalid-token",
                RefreshToken = "invalid-refresh-token"
            };

            var failedResponse = new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid or expired refresh token"
            };

            _mockUserService.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid or expired refresh token", response.ErrorMessage);
        }
    }
}