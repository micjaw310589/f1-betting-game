using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Text.Json;
using F1BettingApp.Application.Exceptions;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Controller for betting operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BetsController : ControllerBase
    {
        private readonly IBettingService _bettingService;
        private readonly ILogger<BetsController> _logger;
        private int userId;

        /// <summary>
        /// Constructor for BetsController
        /// </summary>
        public BetsController(
            IBettingService bettingService,
            ILogger<BetsController> logger)
        {
            _bettingService = bettingService;
            _logger = logger;

            // Extract userId from authenticated token
            var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimValue))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            if (!int.TryParse(claimValue, out userId))
            {
                _logger.LogWarning("Invalid user identifier in token: {ClaimValue}", claimValue);
                throw new UnauthorizedAccessException("Invalid user identifier in authentication token");
            }
        }

        /// <summary>
        /// Place a new bet
        /// </summary>
        /// <remarks>
        /// <example>
        /// POST /api/bets
        /// {
        ///   "raceId": 1,
        ///   "driverId": 5,
        ///   "amount": 100,
        ///   "betType": "RaceWinner"
        /// }
        /// </example>
        /// </remarks>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetDto dto)
        {
            _logger.LogInformation("Attempting to place bet. RaceId: {RaceId}, DriverId: {DriverId}, Amount: {Amount}",
                dto.RaceId, dto.DriverId, dto.Amount);

            try
            {
                // Validate DTO
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Call service to place the bet using the authenticated user ID
                var result = await _bettingService.PlaceBetAsync(userId, dto);

                _logger.LogInformation("Bet placed successfully");

                return Ok(new { message = "Bet placed successfully", userId });
            }
            catch (UserNotFoundException)
            {
                _logger.LogWarning("User not found for bet placement");
                return Unauthorized();
            }
            catch (InsufficientFundsException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (RaceNotFoundException)
            {
                _logger.LogWarning("Race not found for bet placement");
                return NotFound();
            }
            catch (RaceNotUpcomingException)
            {
                _logger.LogWarning("Race is not upcoming");
                return BadRequest(new { error = "Race is not scheduled" });
            }
            catch (DriverNotFoundException)
            {
                _logger.LogWarning("Driver not found for bet placement");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing bet");
                return StatusCode(500, new { error = "An error occurred while placing the bet" });
            }
        }

        /// <summary>
        /// Spec-aligned alias for placing a bet.
        /// </summary>
        [HttpPost("place")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> PlaceBetSpecAlias([FromBody] PlaceBetDto dto)
        {
            // Reuse the existing implementation to avoid divergence.
            return PlaceBet(dto);
        }

        /// <summary>
        /// Get all bets for the current user
        /// </summary>
        /// <remarks>
        /// <example>
        /// GET /api/bets
        /// </example>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBets()
        {
            _logger.LogInformation("Fetching bets for current user");

            try
            {


                var bets = await _bettingService.GetUserBetsAsync(userId);
                return Ok(bets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bets");
                return StatusCode(500, new { error = "An error occurred while fetching bets" });
            }
        }

        /// <summary>
        /// Get a specific bet by ID
        /// </summary>
        /// <remarks>
        /// <example>
        /// GET /api/bets/123
        /// </example>
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBetById([FromRoute] int id)
        {
            _logger.LogInformation("Fetching bet with ID: {BetId}", id);

            try
            {


                var bet = await _bettingService.GetBetByIdAsync(id, userId);

                if (bet == null)
                {
                    return NotFound();
                }

                return Ok(bet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bet by ID");
                return StatusCode(500, new { error = "An error occurred while fetching the bet" });
            }
        }

        /// <summary>
        /// Cancel a bet
        /// </summary>
        /// <remarks>
        /// <example>
        /// DELETE /api/bets/123
        /// </example>
        /// </remarks>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelBet([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to cancel bet with ID: {BetId}", id);

            try
            {
                await _bettingService.CancelBetAsync(id, userId);

                _logger.LogInformation("Bet cancelled successfully. BetId: {BetId}, UserId: {UserId}", id, userId);

                return Ok(new { message = "Bet cancelled successfully", betId = id });
            }
            catch (BetNotFoundException)
            {
                _logger.LogWarning("Bet not found for cancellation");
                return NotFound();
            }
            catch (RaceAlreadyStartedException)
            {
                _logger.LogWarning("Cannot cancel bet after race started");
                return UnprocessableEntity(new { error = "Cannot cancel bet after race has started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling bet");
                return StatusCode(500, new { error = "An error occurred while cancelling the bet" });
            }
        }

        /// <summary>
        /// Spec-aligned alias for cancelling a bet.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> CancelBetSpecAlias([FromRoute] int id)
        {
            // Reuse the existing implementation to avoid divergence.
            return CancelBet(id);
        }
    }
}
