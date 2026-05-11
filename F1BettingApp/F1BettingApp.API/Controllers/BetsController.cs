using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;
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

        /// <summary>
        /// Constructor for BetsController - simplified to avoid NullReferenceException
        /// </summary>
        public BetsController(
            IBettingService bettingService,
            ILogger<BetsController> logger)
        {
            _bettingService = bettingService;
            _logger = logger;
        }

        /// <summary>
        /// Helper property to safely extract userId from authenticated token during request execution
        /// </summary>
        private int AuthenticatedUserId
        {
            get
            {
                var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimValue))
                {
                    _logger.LogError("User claim not found in token");
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                if (!int.TryParse(claimValue, out var id))
                {
                    _logger.LogWarning("Invalid user identifier in token: {ClaimValue}", claimValue);
                    throw new UnauthorizedAccessException("Invalid user identifier in authentication token");
                }
                return id;
            }
        }

        /// <summary>
        /// Place a new bet
        /// </summary>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetDto dto)
        {
            _logger.LogInformation("Attempting to place bet. RaceId: {RaceId}, DriverId: {DriverId}, Amount: {Amount}",
                dto.RaceId, dto.DriverId, dto.Amount);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Use the helper property instead of constructor field
                var result = await _bettingService.PlaceBetAsync(AuthenticatedUserId, dto);

                _logger.LogInformation("Bet placed successfully");

                return Ok(new { message = "Bet placed successfully", userId = AuthenticatedUserId });
            }
            catch (UserNotFoundException)
            {
                return Unauthorized();
            }
            catch (InsufficientFundsException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (RaceNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing bet");
                return StatusCode(500, new { error = "An error occurred while placing the bet" });
            }
        }

        [HttpPost("place")]
        public Task<IActionResult> PlaceBetSpecAlias([FromBody] PlaceBetDto dto)
        {
            return PlaceBet(dto);
        }

        /// <summary>
        /// Get all bets for the current user
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBets()
        {
            _logger.LogInformation("Fetching bets for current user");

            try
            {
                var bets = await _bettingService.GetUserBetsAsync(AuthenticatedUserId);
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBetById([FromRoute] int id)
        {
            _logger.LogInformation("Fetching bet with ID: {BetId}", id);

            try
            {
                var bet = await _bettingService.GetBetByIdAsync(id, AuthenticatedUserId);

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
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBet([FromRoute] int id)
        {
            _logger.LogInformation("Attempting to cancel bet with ID: {BetId}", id);

            try
            {
                await _bettingService.CancelBetAsync(id, AuthenticatedUserId);
                return Ok(new { message = "Bet cancelled successfully", betId = id });
            }
            catch (BetNotFoundException)
            {
                return NotFound();
            }
            catch (RaceAlreadyStartedException)
            {
                return UnprocessableEntity(new { error = "Cannot cancel bet after race has started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling bet");
                return StatusCode(500, new { error = "An error occurred while cancelling the bet" });
            }
        }

        [HttpPost("{id}/cancel")]
        public Task<IActionResult> CancelBetSpecAlias([FromRoute] int id)
        {
            return CancelBet(id);
        }
    }
}