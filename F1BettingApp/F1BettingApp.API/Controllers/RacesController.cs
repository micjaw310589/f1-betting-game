using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Controller for race-related API endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _raceService;
        private readonly ILogger<RacesController> _logger;
        private readonly IOptions<RaceCacheOptions> _cacheOptions;

        /// <summary>
        /// Cache options for race data
        /// </summary>
        public class RaceCacheOptions
        {
            public int DefaultExpirationMinutes { get; set; } = 30;
            public int UpcomingRacesExpirationMinutes { get; set; } = 60;
            public int RaceDetailsExpirationMinutes { get; set; } = 15;
            public int ResultsExpirationMinutes { get; set; } = 30;
        }

        private readonly IRepository<Driver> _driverRepository;

        public RacesController(
            IRaceService raceService,
            ILogger<RacesController> logger,
            IOptions<RaceCacheOptions> cacheOptions,
            IRepository<Driver> driverRepository)
        {
            _raceService = raceService;
            _logger = logger;
            _cacheOptions = cacheOptions;
            _driverRepository = driverRepository;
        }

        /// <summary>
        /// Get all races with pagination support
        /// </summary>
        /// <param name="page">Page number (1-indexed)</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="status">Filter by race status</param>
        /// <param name="season">Filter by season year</param>
        /// <param name="country">Filter by country</param>
        /// <returns>Paginated list of races</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<RaceSummaryDto>>> GetRaces(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string status = null,
            [FromQuery] int? season = null,
            [FromQuery] string country = null)
        {
            _logger.LogInformation("Getting races with parameters: Page={Page}, PageSize={PageSize}, Status={Status}, Season={Season}, Country={Country}",
                page, pageSize, status, season, country);

            try
            {
                var races = await _raceService.GetAllRacesAsync();

                // Apply filters
                var filteredRaces = ApplyFilters(races, status, season, country);

                // Calculate pagination
                var totalItems = filteredRaces.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var startIndex = (page - 1) * pageSize;
                var pagedRaces = filteredRaces.Skip(startIndex).Take(pageSize);

                var result = new PagedResult<RaceSummaryDto>
                {
                    Items = pagedRaces.Select(r => new RaceSummaryDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Circuit = r.Circuit,
                        Country = r.Country,
                        RaceDate = r.RaceDate,
                        Status = r.Status,
                        Season = r.Season,
                        Flag = r.Flag
                    }),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                _logger.LogInformation("Races retrieved: Total={Total}, Page={Page}, PageSize={PageSize}",
                    totalItems, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving races");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving races",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get upcoming races only
        /// </summary>
        /// <param name="season">Filter by season year</param>
        /// <returns>List of upcoming races</returns>
        [HttpGet("upcoming")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RaceSummaryDto>>> GetUpcomingRaces(
            [FromQuery] int? season = null)
        {
            _logger.LogInformation("Getting upcoming races");

            try
            {
                var upcomingRaces = await _raceService.GetUpcomingRacesAsync();

                var races = upcomingRaces.Select(r => new RaceSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Circuit = r.Circuit,
                    Country = r.Country,
                    RaceDate = r.RaceDate,
                    Status = r.Status,
                    Season = r.Season,
                    Flag = r.Flag
                });

                _logger.LogInformation("Upcoming races retrieved: Count={Count}", races.Count());

                return Ok(races);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming races");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving upcoming races",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get race details by ID
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        /// <returns>Detailed race information</returns>
        [HttpGet("{raceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RaceDetailDto>> GetRaceById(
            [FromRoute] int raceId)
        {
            _logger.LogInformation("Getting race by ID: {RaceId}", raceId);

            try
            {
                var race = await _raceService.GetRaceByIdAsync(raceId);

                if (race == null)
                {
                    _logger.LogWarning("Race not found: {RaceId}", raceId);
                    return NotFound(new ErrorResponse
                    {
                        Error = "RACE_NOT_FOUND",
                        Message = $"Race with ID {raceId} not found"
                    });
                }

                var raceDetail = new RaceDetailDto
                {
                    Id = race.Id,
                    Name = race.Name,
                    Circuit = race.Circuit,
                    Country = race.Country,
                    RaceDate = race.RaceDate,
                    Status = race.Status,
                    Season = race.Season
                };

                _logger.LogInformation("Race retrieved: {RaceId}, {RaceName}", raceId, race.Name);

                return Ok(raceDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving race by ID: {RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving race details",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get race results by race ID
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        /// <returns>Race results</returns>
        [HttpGet("{raceId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RaceResultDto>> GetRaceResults(
            [FromRoute] int raceId)
        {
            _logger.LogInformation("Getting race results for race: {RaceId}", raceId);

            try
            {
                var race = await _raceService.GetRaceByIdAsync(raceId);

                // For now, we'll return a default result structure
                // In a real implementation, this would fetch actual race results
                var result = new RaceResultDto
                {
                    RaceId = raceId,
                    RaceName = race?.Name ?? "Race Results",
                    Circuit = race?.Circuit ?? "Circuit",
                    Country = race?.Country ?? "Country",
                    RaceDate = race?.RaceDate ?? DateTime.UtcNow,
                    WinnerDriverId = 0,
                    WinnerDriverName = "TBD",
                    WinnerTeamId = 0,
                    WinnerTeamName = "TBD",
                    WinningMargin = 0,
                    FastestLapDriverId = 0,
                    FastestLapDriverName = "TBD",
                    PolePositionDriverId = 0,
                    PolePositionDriverName = "TBD",
                    SafetyCar = 0,
                    VirtualSafetyCar = 0,
                    RedFlag = 0,
                    YellowFlag = 0,
                    BlackFlag = 0,
                    BlueFlag = 0,
                    BlackAndWhiteFlag = 0,
                    ChequeredFlag = 0,
                    RaceDistance = 0,
                    RaceDistanceUnit = 0,
                    Laps = 0,
                    LapsCompleted = 0,
                    LapsToFinish = 0,
                    RaceControlMessage = 0,
                    RaceControlMessageText = "",
                    TimeAttack = "",
                    TimeAttackResult = "",
                    TimeAttackComment = "",
                    TimeAttackStatus = "",
                    TimeAttackLaps = ""
                };

                _logger.LogInformation("Race results retrieved for: {RaceId}", raceId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving race results: {RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving race results",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Search races by name or circuit
        /// </summary>
        /// <param name="query">Search query</param>
        /// <returns>Matching races</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RaceSummaryDto>>> SearchRaces(
            [FromQuery] string query)
        {
            _logger.LogInformation("Searching races with query: {Query}", query);

            try
            {
                var races = await _raceService.GetAllRacesAsync();
                var filtered = races
                    .Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                              r.Circuit.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(10);

                var results = filtered.Select(r => new RaceSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Circuit = r.Circuit,
                    Country = r.Country,
                    RaceDate = r.RaceDate,
                    Status = r.Status,
                    Season = r.Season,
                    Flag = r.Flag
                });

                _logger.LogInformation("Search results: Query={Query}, Count={Count}", query, results.Count());

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching races");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while searching races",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get races by season
        /// </summary>
        /// <param name="season">Season year</param>
        /// <returns>Races for the specified season</returns>
        [HttpGet("season/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RaceSummaryDto>>> GetRacesBySeason(
            [FromRoute] int season)
        {
            _logger.LogInformation("Getting races by season: {Season}", season);

            try
            {
                var races = await _raceService.GetAllRacesAsync();
                var filtered = races.Where(r => r.Season == season);

                var results = filtered.Select(r => new RaceSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Circuit = r.Circuit,
                    Country = r.Country,
                    RaceDate = r.RaceDate,
                    Status = r.Status,
                    Season = r.Season,
                    Flag = r.Flag
                });

                _logger.LogInformation("Season races retrieved: Season={Season}, Count={Count}",
                    season, results.Count());

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting races by season: {Season}", season);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving races by season",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Synchronize race data from OpenF1 API
        /// </summary>
        /// <returns>Sync status</returns>
        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SyncRaceData()
        {
            _logger.LogInformation("Syncing race data from OpenF1 API");

            try
            {
                await _raceService.SyncRaceDataFromOpenF1Async();

                _logger.LogInformation("Race data synchronization completed");

                return Ok(new { message = "Race data synchronized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during race data synchronization");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_SYNC_FAILED",
                        Message = "Failed to synchronize race data from OpenF1 API",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get upcoming races with calculated odds
        /// </summary>
        /// <returns>Upcoming races with odds</returns>
        [HttpGet("upcoming/odds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RaceDto>>> GetUpcomingRacesWithOdds()
        {
            _logger.LogInformation("Getting upcoming races with odds");

            try
            {
                var races = await _raceService.GetUpcomingRacesWithOddsAsync();

                _logger.LogInformation("Upcoming races with odds retrieved: Count={Count}",
                    races.Count());

                return Ok(races);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming races with odds");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving upcoming races with odds",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Update race status (admin functionality - out of scope for production)
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        /// <param name="status">New status</param>
        /// <returns>Updated race status</returns>
        [HttpPut("{raceId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateRaceStatus(
            [FromRoute] int raceId,
            [FromBody] RaceStatus status)
        {
            _logger.LogInformation("Updating race status: RaceId={RaceId}, Status={Status}",
                raceId, status);

            try
            {
                await _raceService.UpdateRaceStatusAsync(raceId, status);

                _logger.LogInformation("Race status updated successfully: RaceId={RaceId}", raceId);

                return Ok(new { message = "Race status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating race status: RaceId={RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_UPDATE_FAILED",
                        Message = "Failed to update race status",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get all drivers (for admin override dropdowns)
        /// </summary>
        /// <returns>List of all drivers</returns>
        [HttpGet("drivers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DriverDto>>> GetAllDrivers()
        {
            _logger.LogInformation("Getting all drivers");

            try
            {
                var drivers = await _driverRepository.GetAllAsync();
                var driverList = drivers
                    .Select(d => new DriverDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Abbreviation = string.Empty,
                        TeamId = d.TeamId,
                        TeamName = d.Team != null ? d.Team.Name : "TBD"
                    })
                    .OrderBy(d => d.Id)
                    .ToList();

                _logger.LogInformation("Drivers retrieved: Count={Count}", driverList.Count);
                return Ok(driverList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "DRIVER_DATA_ERROR",
                        Message = "An error occurred while retrieving drivers",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Helper method to apply filters to races
        /// </summary>
        private static IEnumerable<RaceDto> ApplyFilters(
            IEnumerable<RaceDto> races,
            string status,
            int? season,
            string country)
        {
            var filtered = races;

            if (!string.IsNullOrEmpty(status))
            {
                filtered = filtered.Where(r => r.Status.ToString() == status);
            }

            if (season.HasValue)
            {
                filtered = filtered.Where(r => r.Season == season);
            }

            if (!string.IsNullOrEmpty(country))
            {
                filtered = filtered.Where(r => r.Country == country);
            }

            return filtered;
        }

        // W RacesController.cs
[HttpGet("{raceId}/drivers-with-odds")]
public async Task<ActionResult<IEnumerable<DriverWithOddsDto>>> GetDriversWithOdds(int raceId)
{
    _logger.LogInformation("Pobieranie kierowców z kursami dla wyścigu: {RaceId}", raceId);
    var results = await _raceService.GetDriversWithOddsForRaceAsync(raceId);
    return Ok(results);
}
    }

    
}

