using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using F1BettingApp.Application.Exceptions;
namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Controller for user profile management operations.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the UsersController.
        /// </summary>
        /// <param name="userService">The user service for business logic operations.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns the current user's profile.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
        {
            // Zmień sposób wyciągania ID na bardziej odporny:
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
               ?? User.FindFirst("sub"); // Dodaj to sprawdzenie alternatywnego claimu
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Parse userId to int for service call
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var userProfile = await _userService.GetUserProfileAsync(userIdInt);
                return Ok(userProfile);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                // Log the exception for debugging purposes
                return StatusCode(500, "An internal error occurred while retrieving user profile");
            }
        }

        /// <summary>
        /// Updates the profile of the currently authenticated user.
        /// </summary>
        /// <param name="dto">The updated profile data.</param>
        /// <response code="200">Returns the updated user's profile.</response>
        /// <response code="400">Returns bad request if validation fails.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpPut("me")]
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> UpdateCurrentUserProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Parse userId to int for service call
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var updatedProfile = await _userService.UpdateUserProfileAsync(userIdInt, dto);
                return Ok(updatedProfile);
            }
            catch (ValidationException ex)
            {
                // Return validation errors with appropriate HTTP status
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                // Log the exception for debugging purposes
                return StatusCode(500, "An internal error occurred while updating user profile");
            }
        }

        /// <summary>
        /// Gets statistics for the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns the current user's statistics.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("me/statistics")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserStatisticsDto>> GetCurrentUserStatistics()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Parse userId to int for service call
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var statistics = await _userService.GetUserStatisticsAsync(userIdInt);
                return Ok(statistics);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                // Log the exception for debugging purposes
                return StatusCode(500, "An internal error occurred while retrieving user statistics");
            }
        }

        /// <summary>
        /// Gets bet history for the currently authenticated user.
        /// </summary>
        /// <param name="page">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 20, max: 100).</param>
        /// <response code="200">Returns the current user's bet history.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("me/history")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<BetHistoryDto>>> GetCurrentUserBetHistory(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate pagination parameters
            if (page < 1 || page > int.MaxValue / pageSize)
            {
                return BadRequest("Invalid page number");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            try
            {
                // Parse userId to int for service call
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var history = await _userService.GetUserBetHistoryAsync(userIdInt, page, pageSize);
                return Ok(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                // Log the exception for debugging purposes
                return StatusCode(500, "An internal error occurred while retrieving bet history");
            }
        }

        /// <summary>
        /// Gets the profile of the currently authenticated user (Task-05 route).
        /// </summary>
        /// <response code="200">Returns the current user's profile.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfileForTask05()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var userProfile = await _userService.GetUserProfileAsync(userIdInt);
                return Ok(userProfile);
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving user profile");
            }
        }

        /// <summary>
        /// Gets paginated bet history for the currently authenticated user (Task-05 route).
        /// </summary>
        /// <param name="page">The page number for pagination (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 20, max: 100).</param>
        /// <response code="200">Returns the current user's bet history.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("bets")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BetHistoryResponseDto>> GetCurrentUserBetHistoryForTask05(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (page < 1 || page > int.MaxValue / pageSize)
            {
                return BadRequest("Invalid page number");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            try
            {
                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                // Per Task-05: use BettingService.GetUserBetHistoryAsync
                var history = await _userService.GetUserBetHistoryAsync(userIdInt, page, pageSize);
                return Ok(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && !IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving bet history");
            }
        }

        /// <summary>
        /// Checks if an exception is related to authorization.
        /// </summary>
        private bool IsAuthorizationException(Exception ex) => 
            ex is UnauthorizedAccessException || 
            ex.Message.Contains("unauthorized") ||
            ex.Message.Contains("forbidden");
    }
}
