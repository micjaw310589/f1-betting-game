//cd F1BettingApp/F1BettingApp.Tests
//dotnet test --filter "F1BettingApp.Tests.AuthControllerTests"
using F1BettingApp.API.Controllers;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace F1BettingApp.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly IConfiguration _configuration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockUserService = new Mock<IUserService>();

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"JwtSettings:SecretKey", "test-secret-key-with-at-least-32-characters"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"}
            });

            _configuration = configurationBuilder.Build();
            _controller = new AuthController(_mockUserService.Object, _configuration);
        }

        [Fact]
        public async Task Register_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!"
            };

            var userDto = new UserDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com"
            };

            // Zmieniono na RegisterResponseDto, bo tego oczekuje kontroler przy rejestracji
            var authResponse = new AuthResponseDto
            {
                IsSuccess = true,
                User = userDto
            };

            _mockUserService.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<RegisterResponseDto>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
            Assert.NotNull(returnValue.User);
        }

        [Fact]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "", Email = "invalid", Password = "123" };
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<RegisterResponseDto>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "user", Email = "dup@ex.com", Password = "Password123!" };
            _mockUserService.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterDto>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var returnValue = Assert.IsType<RegisterResponseDto>(conflictResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task Register_RegistrationFails_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "user", Email = "test@ex.com", Password = "Password123!" };
            _mockUserService.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<RegisterResponseDto>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "test@ex.com", Password = "Password123!" };
            var authResponse = new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = "access",
                RefreshToken = "refresh",
                User = new UserDto { Id = 1, Username = "user" }
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "wrong", Password = "wrong" };
            var authResponse = new AuthResponseDto { IsSuccess = false, ErrorMessage = "Invalid credentials" };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task Login_SuspendedAccount_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { UsernameOrEmail = "test@ex.com", Password = "Password123!" };
            var authResponse = new AuthResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "Account suspended"
            };

            _mockUserService.Setup(x => x.AuthenticateUserAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Account suspended", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto();
            _controller.ModelState.AddModelError("Error", "Required");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(badRequestResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task RefreshToken_ValidTokens_ReturnsOkResult()
        {
            // Arrange
            var dto = new RefreshTokenDto { Token = "old", RefreshToken = "valid" };
            var response = new AuthResponseDto { IsSuccess = true, AccessToken = "new-access", RefreshToken = "new-refresh" };
            _mockUserService.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>())).ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.True(returnValue.IsSuccess);
        }

        [Fact]
        public async Task RefreshToken_SuspendedAccount_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new RefreshTokenDto { Token = "old", RefreshToken = "valid" };
            var response = new AuthResponseDto { IsSuccess = false, ErrorMessage = "Account suspended" };
            _mockUserService.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>())).ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(returnValue.IsSuccess);
            Assert.Equal("Account suspended", returnValue.ErrorMessage);
        }

        [Fact]
        public async Task RefreshToken_InvalidTokens_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new RefreshTokenDto { Token = "bad", RefreshToken = "bad" };
            var response = new AuthResponseDto { IsSuccess = false, ErrorMessage = "Invalid refresh token" };
            _mockUserService.Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>())).ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(unauthorizedResult.Value);
            Assert.False(returnValue.IsSuccess);
        }

        [Fact]
        public async Task GetCurrentUser_ValidToken_ReturnsUser()
        {
            // Arrange
            var userDto = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" };
            _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(userDto);

            var key = Encoding.ASCII.GetBytes("test-secret-key-with-at-least-32-characters");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            _controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };
            _controller.Request.Headers["Authorization"] = $"Bearer {tokenString}";

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }
    }
}