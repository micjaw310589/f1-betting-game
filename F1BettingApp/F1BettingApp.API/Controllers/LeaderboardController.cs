using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Controller for leaderboard and ranking operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        /// <summary>
        /// Initializes a new instance of the LeaderboardController.
        /// </summary>
        /// <param name="leaderboardService">The leaderboard service for business logic operations.</param>
        /// <param name="logger">The logger for logging controller operations.</param>
        public LeaderboardController(
            ILeaderboardService leaderboardService,
            ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the global leaderboard with top players.
        /// </summary>
        /// <response code="200">Returns the global leaderboard.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="500">Returns internal server error on failure.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetGlobalLeaderboard(
            [FromQuery] int limit = 50)
        {
            _logger.LogInformation("Fetching global leaderboard. Limit: {Limit}", limit);

            if (limit < 1 || limit > 1000)
            {
                return BadRequest(new { error = "Limit must be between 1 and 1000" });
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var leaderboard = await _leaderboardService.GetGlobalLeaderboardAsync(limit);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching global leaderboard");
                return StatusCode(500, new { error = "An error occurred while fetching the leaderboard" });
            }
        }

        /// <summary>
        /// Gets the top players by count.
        /// </summary>
        /// <param name="count">The number of top players to retrieve.</param>
        /// <response code="200">Returns the top players.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="500">Returns internal server error on failure.</response>
        [HttpGet("top/{count}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetTopPlayers(
            [FromRoute] int count)
        {
            _logger.LogInformation("Fetching top players. Count: {Count}", count);

            if (count < 1 || count > 100)
            {
                return BadRequest(new { error = "Count must be between 1 and 100" });
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var topPlayers = await _leaderboardService.GetTopPlayersAsync(count);
                return Ok(topPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top players");
                return StatusCode(500, new { error = "An error occurred while fetching top players" });
            }
        }

        /// <summary>
        /// Gets the current user's ranking information.
        /// </summary>
        /// <response code="200">Returns the current user's ranking.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="500">Returns internal server error on failure.</response>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserRankingDto>> GetCurrentUserRanking()
        {
            _logger.LogInformation("Fetching current user ranking");

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

                var ranking = await _leaderboardService.GetUserRankingAsync(userIdInt);
                return Ok(ranking);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "User not found" });
            }
            catch (Exception ex) when (!IsAuthorizationException(ex))
            {
                _logger.LogError(ex, "Error fetching user ranking");
                return StatusCode(500, new { error = "An error occurred while fetching user ranking" });
            }
        }

        /// <summary>
        /// Gets historical leaderboard data.
        /// </summary>
        /// <param name="season">The season for which to retrieve historical data.</param>
        /// <response code="200">Returns the historical leaderboard data.</response>
        /// <response code="401">Returns unauthorized if not authenticated.</response>
        /// <response code="500">Returns internal server error on failure.</response>
        [HttpGet("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<HistoricalLeaderboardDto>>> GetHistoricalLeaderboard(
            [FromQuery] string? season = null)
        {
            _logger.LogInformation("Fetching historical leaderboard data. Season: {Season}", season);

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!int.TryParse(userId, out var userIdInt))
                {
                    return BadRequest("Invalid user identifier");
                }

                var historicalData = await _leaderboardService.GetHistoricalLeaderboardAsync(season);
                return Ok(historicalData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical leaderboard data");
                return StatusCode(500, new { error = "An error occurred while fetching historical data" });
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