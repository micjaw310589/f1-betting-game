using Xunit;
using Moq;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;
using F1BettingApp.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace F1BettingApp.Tests
{
    // This test class uses Unit Testing principles by mocking dependencies (like IUserService)
    // to isolate the controller's logic layer interactions, making it less dependent on full API integration setup.
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
            _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns("SuperTajnyKluczF1BettingApp2026!WymagaMinimum32Znakow");
            _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("F1BettingApp");
            _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("F1BettingAppUsers");

            _controller = new AuthController(_mockUserService.Object, _mockConfiguration.Object);
        }

        // --- Test User Registration Logic Path ---
        [Fact]
        public async Task Register_WithValidData_ShouldCallServiceAndReturnToken()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "testuser", Email = "test@example.com", Password = "StrongPassword123" };
            var expectedAuthResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "mock_jwt_token_success",
                RefreshToken = "mock_refresh_token",
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com", Points = 0 }
            };

            // Configure mock service: Simulate successful registration logic flow
            _mockUserService.Setup(s => s.RegisterUserAsync(registerDto))
                            .ReturnsAsync(expectedAuthResponse);

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
        public async Task Register_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidDto = new RegisterDto { Username = "testuser", Email = "invalid-email", Password = "StrongPassword123" };

            // Add model state error
            _controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

            // Act
            var result = await _controller.Register(invalidDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<RegisterResponseDto>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Contains("The Email field is not a valid e-mail address.", response.ErrorMessage);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "testuser", Email = "duplicate@example.com", Password = "StrongPassword123" };

            // Configure mock service to throw exception for duplicate email
            _mockUserService.Setup(s => s.RegisterUserAsync(registerDto))
                            .ThrowsAsync(new System.InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var response = Assert.IsType<RegisterResponseDto>(conflictResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Contains("Email already exists", response.ErrorMessage);
        }

        // --- Test User Login Logic Path ---
        [Fact]
        public async Task Login_WithCorrectCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "testuser", Password = "StrongPassword123" };
            var expectedAuthResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "mock_jwt_token_success",
                RefreshToken = "mock_refresh_token",
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com", Points = 1000 }
            };

            // Setup successful authentication call
            _mockUserService.Setup(s => s.AuthenticateUserAsync(loginDto))
                            .ReturnsAsync(expectedAuthResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal("mock_jwt_token_success", response.AccessToken);
            Assert.Equal("mock_refresh_token", response.RefreshToken);
        }

        [Fact]
        public async Task Login_WithIncorrectCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "wronguser", Password = "WrongPassword" };
            var failedAuthResponse = new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid credentials"
            };

            // Setup failed authentication call
            _mockUserService.Setup(s => s.AuthenticateUserAsync(loginDto))
                            .ReturnsAsync(failedAuthResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid credentials", response.ErrorMessage);
        }

        [Fact]
        public async Task Login_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "", Password = "" };

            // Add model state error
            _controller.ModelState.AddModelError("UsernameOrEmail", "The UsernameOrEmail field is required.");
            _controller.ModelState.AddModelError("Password", "The Password field is required.");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid request data", response.ErrorMessage);
        }

        // --- Test Token Refresh Logic Path ---
        [Fact]
        public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewAccessToken()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "valid-refresh-token", Token = "expired-access-token" };
            var expectedAuthResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                User = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com", Points = 1000 }
            };

            // Setup successful token refresh call
            _mockUserService.Setup(s => s.RefreshTokenAsync(refreshTokenDto))
                            .ReturnsAsync(expectedAuthResponse);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(response.IsSuccess);
            Assert.Equal("new_access_token", response.AccessToken);
            Assert.Equal("new_refresh_token", response.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "invalid-token", Token = "expired-access-token" };
            var failedAuthResponse = new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Invalid or expired refresh token"
            };

            // Setup failed token refresh call
            _mockUserService.Setup(s => s.RefreshTokenAsync(refreshTokenDto))
                            .ReturnsAsync(failedAuthResponse);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid or expired refresh token", response.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { RefreshToken = "", Token = "" };

            // Add model state error
            _controller.ModelState.AddModelError("RefreshToken", "The RefreshToken field is required.");

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<AuthResponseDto>(badRequestResult.Value);
            Assert.False(response.IsSuccess);
            Assert.Equal("Invalid request data", response.ErrorMessage);
        }
    }
}