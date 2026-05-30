using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Enums;
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
        /// Gets bet history for the currently authenticated user (Task-05 route).
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
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving bet history");
            }
        }

        // --- Enhanced Statistics Endpoints ---
        /// <summary>
        /// Gets enhanced statistics for the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns the current user's enhanced statistics.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("me/stats/enhanced")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EnhancedUserStatisticsDto>> GetCurrentUserEnhancedStatistics()
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

                var statistics = await _userService.GetEnhancedUserStatisticsAsync(userIdInt);
                return Ok(statistics);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving user statistics");
            }
        }

        /// <summary>
        /// Gets bet history for a specific user with filtering and pagination.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="limit">Number of items to return (default: 50)</param>
        /// <param name="offset">Offset for pagination (default: 0)</param>
        /// <param name="status">Optional filter by bet status</param>
        /// <param name="driverId">Optional filter by driver ID</param>
        /// <response code="200">Returns the user's bet history.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="403">Returns forbidden if user is not an admin and tries to access other users.</response>
        [HttpGet("{userId}/bets/history")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<BetHistoryDto>>> GetUserBetHistory(
            int userId,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            [FromQuery] BetStatus? status = null,
            [FromQuery] int? driverId = null)
        {
            // Check if user is admin or trying to access their own data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (!int.TryParse(currentUserId, out var currentUserIdInt))
            {
                return BadRequest("Invalid user identifier");
            }

            // Only admins can access other users' data
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && userId != currentUserIdInt)
            {
                return Forbid();
            }

            try
            {
                var history = await _userService.GetBetHistoryAsync(userId, limit, offset, status, driverId);
                return Ok(history);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving bet history");
            }
        }

        /// <summary>
        /// Gets comprehensive bet analysis for the currently authenticated user.
        /// </summary>
        /// <response code="200">Returns the current user's bet analysis.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        [HttpGet("me/bets/analysis")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserBetAnalysisDto>> GetCurrentUserBetAnalysis()
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

                var analysis = await _userService.GetUserBetAnalysisAsync(userIdInt);
                return Ok(analysis);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving bet analysis");
            }
        }

        /// <summary>
        /// Gets user statistics for a specific time range.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <response code="200">Returns the user's statistics for the time range.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="403">Returns forbidden if user is not an admin and tries to access other users.</response>
        [HttpGet("{userId}/stats/range")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EnhancedUserStatisticsDto>> GetUserStatisticsByRange(
            int userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            // Check if user is admin or trying to access their own data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (!int.TryParse(currentUserId, out var currentUserIdInt))
            {
                return BadRequest("Invalid user identifier");
            }

            // Only admins can access other users' data
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && userId != currentUserIdInt)
            {
                return Forbid();
            }

            try
            {
                var stats = await _userService.GetUserStatisticsByTimeRangeAsync(userId, startDate, endDate);
                return Ok(stats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving statistics");
            }
        }

        /// <summary>
        /// Checks if an exception is related to authorization.
        /// </summary>
        private bool IsAuthorizationException(Exception ex) => 
            ex is UnauthorizedAccessException || 
            ex.Message.Contains("unauthorized") ||
            ex.Message.Contains("forbidden");

        // ========================================
        // Admin Endpoints
        // ========================================

        /// <summary>
        /// Lists all users with optional filtering and pagination (admin only).
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
        /// <param name="filterIsActive">Optional filter by active status.</param>
        /// <param name="searchTerm">Optional search by username or email.</param>
        /// <response code="200">Returns paginated list of users.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="403">Returns forbidden if user is not an admin.</response>
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<AdminUserDto>>> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? filterIsActive = null,
            [FromQuery] string? searchTerm = null)
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest("Page number must be at least 1");
            }
            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            try
            {
                var result = await _userService.GetAllUsersAsync(page, pageSize, filterIsActive, searchTerm);
                return Ok(result);
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while retrieving users");
            }
        }

        /// <summary>
        /// Adjusts a user's point balance (admin only).
        /// </summary>
        /// <param name="userId">The ID of the user to adjust.</param>
        /// <param name="dto">The adjustment details including delta and reason.</param>
        /// <response code="200">Returns the adjustment result.</response>
        /// <response code="400">Returns bad request if validation fails or balance would go negative.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="403">Returns forbidden if user is not an admin.</response>
        /// <response code="404">Returns not found if user does not exist.</response>
        [HttpPatch("admin/users/{userId}/points")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdjustPointsResultDto>> AdjustUserPoints(
            int userId,
            [FromBody] AdjustUserPointsDto dto)
        {
            // Validate the adjustment amount
            if (dto.Points == 0)
            {
                return BadRequest("Points adjustment must be non-zero.");
            }

            // Get admin user ID from claims
            var adminUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminUserIdClaim) || !int.TryParse(adminUserIdClaim, out var adminUserId))
            {
                return Unauthorized("Unable to identify admin user.");
            }

            try
            {
                var result = await _userService.AdjustUserPointsAsync(userId, dto.Points, dto.Reason, adminUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while adjusting points");
            }
        }

        /// <summary>
        /// Changes a user's account status (suspend/reactivate) (admin only).
        /// </summary>
        /// <param name="userId">The ID of the user to modify.</param>
        /// <param name="dto">The status change details.</param>
        /// <response code="200">Returns the updated user.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="403">Returns forbidden if user is not an admin.</response>
        /// <response code="404">Returns not found if user does not exist.</response>
        [HttpPatch("admin/users/{userId}/status")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdminUserDto>> ChangeUserStatus(
            int userId,
            [FromBody] ChangeUserStatusDto dto)
        {
            // Get admin user ID from claims
            var adminUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminUserIdClaim) || !int.TryParse(adminUserIdClaim, out var adminUserId))
            {
                return Unauthorized("Unable to identify admin user.");
            }

            try
            {
                var result = await _userService.ChangeUserStatusAsync(userId, dto.IsActive, dto.Reason, adminUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                return StatusCode(500, "An internal error occurred while changing user status");
            }
        }
    }
}
