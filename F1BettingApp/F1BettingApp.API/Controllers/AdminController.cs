using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Admin controller for system management operations.
    /// Requires admin role authorization.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IRaceService _raceService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IRaceService raceService,
            ILogger<AdminController> logger)
        {
            _raceService = raceService;
            _logger = logger;
        }

        /// <summary>
        /// Manually triggers OpenF1 data synchronization.
        /// Skips races that have been manually overridden by an admin.
        /// </summary>
        /// <returns>Sync result with details of processed races</returns>
        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SyncResultDto>> TriggerSync()
        {
            _logger.LogInformation("Admin manually triggering race data sync");

            try
            {
                var result = await _raceService.SyncRaceDataFromOpenF1Async();

                _logger.LogInformation(
                    "Admin sync completed: Success={Success}, Created={Created}, Updated={Updated}, Processed={Processed}",
                    result.Success, result.RacesCreated, result.RacesUpdated, result.RacesProcessed);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin sync failed: entity not found");
                return NotFound(new ErrorResponse
                {
                    Error = "SYNC_ENTITY_NOT_FOUND",
                    Message = "An entity required for sync was not found",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Admin sync failed: invalid operation");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "SYNC_FAILED",
                    Message = "Failed to synchronize race data from OpenF1 API",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin sync failed with unexpected error");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "SYNC_ERROR",
                    Message = "An unexpected error occurred during synchronization",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the current results for a race (admin view).
        /// </summary>
        /// <param name="raceId">The ID of the race</param>
        /// <returns>Race result DTO with driver details</returns>
        [HttpGet("races/{raceId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RaceResultDto>> GetRaceResults(int raceId)
        {
            _logger.LogInformation("Admin fetching race results: RaceId={RaceId}", raceId);

            try
            {
                var result = await _raceService.GetRaceResultDtoAsync(raceId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin race results not found: RaceId={RaceId}", raceId);
                return NotFound(new ErrorResponse
                {
                    Error = "RACE_NOT_FOUND",
                    Message = $"Race with ID {raceId} not found",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin race results: RaceId={RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "RACE_DATA_ERROR",
                    Message = "An error occurred while retrieving race results",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Overrides race results manually (admin only).
        /// Sets IsManuallyOverridden flag to prevent future auto-sync from reverting.
        /// </summary>
        /// <param name="raceId">The ID of the race to override</param>
        /// <param name="dto">The override data with finishing positions</param>
        /// <returns>Confirmation of the override</returns>
        [HttpPut("races/{raceId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> OverrideRaceResults(
            [FromRoute] int raceId,
            [FromBody] OverrideRaceResultDto dto)
        {
            _logger.LogInformation(
                "Admin overriding race results: RaceId={RaceId}, Positions={PositionCount}",
                raceId, dto.Positions?.Count ?? 0);

            try
            {
                if (dto.Positions == null || !dto.Positions.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Error = "INVALID_INPUT",
                        Message = "At least one position entry is required.",
                        Details = "The positions list cannot be empty."
                    });
                }

                await _raceService.OverrideRaceResultAsync(raceId, dto);

                _logger.LogInformation(
                    "Race results overridden successfully: RaceId={RaceId}", raceId);

                return Ok(new
                {
                    message = "Race results overridden successfully",
                    raceId = raceId,
                    positionsCount = dto.Positions.Count,
                    isManuallyOverridden = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin race result override failed: Race not found, RaceId={RaceId}", raceId);
                return NotFound(new ErrorResponse
                {
                    Error = "RACE_NOT_FOUND",
                    Message = $"Race with ID {raceId} not found",
                    Details = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Admin race result override failed: Invalid input, RaceId={RaceId}", raceId);
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_INPUT",
                    Message = "Invalid position data provided",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Admin race result override failed: Invalid operation, RaceId={RaceId}", raceId);
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_OPERATION",
                    Message = ex.Message,
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overriding race results: RaceId={RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "OVERRIDE_FAILED",
                    Message = "Failed to override race results",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets all races with their override status (admin view).
        /// </summary>
        /// <returns>List of races with override status</returns>
        [HttpGet("races")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<RaceDto>>> GetAdminRaces()
        {
            _logger.LogInformation("Admin fetching all races");

            try
            {
                var races = await _raceService.GetAllRacesAsync();
                return Ok(races);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching admin races");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "RACE_DATA_ERROR",
                    Message = "An error occurred while retrieving races",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates race metadata (name, date, status, circuit, country) - admin only.
        /// Sets IsManuallyOverridden to prevent future auto-sync from reverting.
        /// </summary>
        /// <param name="raceId">The ID of the race to update.</param>
        /// <param name="dto">The metadata to update.</param>
        /// <returns>Confirmation of the update</returns>
        [HttpPut("races/{raceId}/metadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateRaceMetadata(
            [FromRoute] int raceId,
            [FromBody] UpdateRaceMetadataDto dto)
        {
            _logger.LogInformation(
                "Admin updating race metadata: RaceId={RaceId}, Name={Name}, Status={Status}",
                raceId, dto.Name, dto.Status);

            try
            {
                await _raceService.UpdateRaceMetadataAsync(raceId, dto);

                _logger.LogInformation(
                    "Race metadata updated successfully: RaceId={RaceId}", raceId);

                return Ok(new
                {
                    message = "Race metadata updated successfully",
                    raceId = raceId,
                    isManuallyOverridden = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin race metadata update failed: Race not found, RaceId={RaceId}", raceId);
                return NotFound(new ErrorResponse
                {
                    Error = "RACE_NOT_FOUND",
                    Message = $"Race with ID {raceId} not found",
                    Details = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Admin race metadata update failed: Invalid input, RaceId={RaceId}", raceId);
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_INPUT",
                    Message = "Invalid metadata data provided",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating race metadata: RaceId={RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "METADATA_UPDATE_FAILED",
                    Message = "Failed to update race metadata",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates a new race (admin only).
        /// </summary>
        /// <param name="dto">The race creation data.</param>
        /// <returns>The created race DTO.</returns>
        [HttpPost("races")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RaceDto>> CreateRace([FromBody] CreateRaceDto dto)
        {
            _logger.LogInformation("Admin creating new race: {Name}", dto.Name);

            try
            {
                var race = await _raceService.CreateRaceAsync(dto);

                _logger.LogInformation("Race created successfully: RaceId={RaceId}, Name={Name}", race.Id, race.Name);

                return CreatedAtAction(nameof(GetAdminRaces), new { }, race);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Admin race creation failed: Invalid input");
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_INPUT",
                    Message = ex.Message,
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating race");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "RACE_CREATION_FAILED",
                    Message = "Failed to create race",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a race (admin only). Only allowed if the race has no bets.
        /// </summary>
        /// <param name="raceId">The ID of the race to delete.</param>
        [HttpDelete("races/{raceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteRace(int raceId)
        {
            _logger.LogInformation("Admin deleting race: RaceId={RaceId}", raceId);

            try
            {
                await _raceService.DeleteRaceAsync(raceId);

                _logger.LogInformation("Race deleted successfully: RaceId={RaceId}", raceId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Admin race deletion failed: Race not found, RaceId={RaceId}", raceId);
                return NotFound(new ErrorResponse
                {
                    Error = "RACE_NOT_FOUND",
                    Message = $"Race with ID {raceId} not found",
                    Details = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Admin race deletion failed: Has bets, RaceId={RaceId}", raceId);
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_OPERATION",
                    Message = ex.Message,
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting race: RaceId={RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "RACE_DELETION_FAILED",
                    Message = "Failed to delete race",
                    Details = ex.Message
                });
            }
        }
    }
}
