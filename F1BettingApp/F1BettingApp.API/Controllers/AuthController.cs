using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Authentication Controller for handling user authentication and token operations
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IDailyLoginService _dailyLoginService;
        private readonly IQuestService _questService;

        public AuthController(IUserService userService, IConfiguration configuration, IDailyLoginService dailyLoginService, IQuestService questService)
        {
            _userService = userService;
            _configuration = configuration;
            _dailyLoginService = dailyLoginService;
            _questService = questService;
            var jwtSettings = configuration.GetSection("JwtSettings");
            _secretKey = jwtSettings["SecretKey"] ?? "fallback-secret-key";
            _issuer = jwtSettings["Issuer"] ?? "F1BettingApp";
            _audience = jwtSettings["Audience"] ?? "F1BettingApp";
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="dto">Registration data</param>
        /// <returns>Registration response with user information</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterDto dto)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new RegisterResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = string.Join("; ", errors)
                });
            }

            try
            {
                // Register the user
                var userDto = await _userService.RegisterUserAsync(dto);

                if (userDto == null)
                {
                    return BadRequest(new RegisterResponseDto
                    {
                        IsSuccess = false,
                        ErrorMessage = "Registration failed"
                    });
                }

                return Ok(new RegisterResponseDto
                {
                    IsSuccess = true,
                    User = userDto.User
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new RegisterResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new RegisterResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Registration failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Authenticate user and return access token
        /// </summary>
        /// <param name="dto">Login data</param>
        /// <returns>Authentication response with tokens</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid request data"
                });
            }

            try
            {
                // Authenticate the user
                var authResponse = await _userService.AuthenticateUserAsync(dto);

                if (authResponse == null || !authResponse.IsSuccess)
                {
                    return Unauthorized(authResponse);
                }

                // Process daily login streak (awards points, updates streak)
                try
                {
                    var userId = authResponse.User?.Id;
                    if (userId.HasValue)
                    {
                        await _dailyLoginService.ProcessDailyLoginAsync(userId.Value);

                        // Update quest progress for login-related quests
                        try
                        {
                            await _questService.UpdateQuestProgressAsync(userId.Value, "first_login", 1);
                            await _questService.UpdateQuestProgressAsync(userId.Value, "login_streak_weekly", 1);
                            // streak_master: awards one-time when streak hits 7
                            var streakInfo = await _dailyLoginService.GetStreakInfoAsync(userId.Value);
                            if (streakInfo != null && streakInfo.CurrentStreak >= 7)
                            {
                                await _questService.UpdateQuestProgressAsync(userId.Value, "streak_master", 1);
                            }
                        }
                        catch
                        {
                            // Quest progress updates should not block login
                        }
                    }
                }
                catch
                {
                    // Log error but don't fail authentication if streak processing fails
                    // This ensures login always succeeds even if streak tracking has issues
                }

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="dto">Refresh token data</param>
        /// <returns>New tokens</returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid request data"
                });
            }

            try
            {
                // Refresh the token
                var authResponse = await _userService.RefreshTokenAsync(dto);

                if (authResponse == null || !authResponse.IsSuccess)
                {
                    return Unauthorized(authResponse);
                }

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Token refresh failed: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Logout user by invalidating refresh token
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            try
            {
                await _userService.RefreshTokenAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Get current user info from JWT token
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
                return Unauthorized("Missing authorization header");

            try
            {
                var validatedPrincipal = _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                if (validatedToken == null)
                    return Unauthorized("Invalid token");

                var userIdClaim = validatedPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Invalid token");

                var userId = int.Parse(userIdClaim.Value);
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                    return Unauthorized();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Validate refresh token without returning new tokens
        /// </summary>
        [HttpPost("validate-refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> ValidateRefreshToken([FromBody] RefreshTokenDto dto)
        {
            try
            {
                await _userService.RefreshTokenAsync(dto);
                return Ok(true);
            }
            catch (Exception)
            {
                return Unauthorized();
            }
        }
    }
}