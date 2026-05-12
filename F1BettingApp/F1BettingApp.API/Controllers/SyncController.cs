using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Controller for administering synchronization operations.
    /// </summary>
    [ApiController]
    [Route("admin")]
    [Authorize(Roles = "Admin")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(ISyncService syncService, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Triggers a full synchronization of OpenF1 data for the current season.
        /// </summary>
        [HttpPost("sync")]
        public async Task<ActionResult<SyncResultDto>> TriggerSync()
        {
            _logger.LogInformation("Admin sync triggered.");

            var currentSeason = DateTime.UtcNow.Year;
            var result = await _syncService.SyncAllAsync(currentSeason);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogError("Full sync failed: {ErrorMessage}", result.ErrorMessage);
                return StatusCode(500, result);
            }
        }
    }
}