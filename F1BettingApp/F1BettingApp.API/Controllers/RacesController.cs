using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Enums;
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
        private readonly IQuestService _questService;

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

        public RacesController(
            IRaceService raceService,
            ILogger<RacesController> logger,
            IOptions<RaceCacheOptions> cacheOptions,
            IQuestService questService)
        {
            _raceService = raceService;
            _logger = logger;
            _cacheOptions = cacheOptions;
            _questService = questService;
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

        // --- TUTAJ JEST NAPRAWA LOGIKI STATUSÓW ---
        IEnumerable<RaceDto> filteredRaces;

        if (!string.IsNullOrEmpty(status) && status.Equals("Finished", StringComparison.OrdinalIgnoreCase))
        {
            // Jeśli użytkownik chce "Finished" (Past Races), to szukamy najpierw ignorując filtr statusu w ApplyFilters, 
            // a potem ręcznie filtrujemy na oba przeszłe statusy: Finished oraz ResultsProcessed
            var racesWithoutStatusFilter = ApplyFilters(races, null, season, country);
            filteredRaces = racesWithoutStatusFilter.Where(r => 
                r.Status.ToString().Equals("Finished", StringComparison.OrdinalIgnoreCase) || 
                r.Status.ToString().Equals("ResultsProcessed", StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // Dla wszystkich innych statusów (w tym InProgress dla Live oraz Scheduled dla Upcoming) 
            // działamy standardowo, tak jak było wcześniej
            filteredRaces = ApplyFilters(races, status, season, country);
        }
        // ------------------------------------------

        // Calculate pagination (reszta kodu bez zmian)
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

                // Update quest progress for race page visit (engagement quests)
                try
                {
                    // Get authenticated user ID if available
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                    {
                        // Track race page views for engagement quests
                        await _questService.UpdateQuestProgressByCategoryEventAsync(userId, "Engagement", "RaceViewed", 1, raceId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    // Quest progress updates should not block race page access
                    _logger.LogDebug(ex, "Failed to update quest progress for race page visit, raceId={RaceId}", raceId);
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
                var result = await _raceService.GetRaceResultDtoAsync(raceId);

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

        /// <summary>
        /// Get all available drivers
        /// </summary>
        /// <returns>List of all drivers</returns>
        [HttpGet("drivers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DriverDto>>> GetDrivers()
        {
            _logger.LogInformation("Getting all drivers");

            try
            {
                var drivers = await _raceService.GetAllDriversAsync();
                _logger.LogInformation("Drivers retrieved: Count={Count}", drivers.Count());
                return Ok(drivers);
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

        [HttpGet("{raceId}/drivers-with-odds")]
        public async Task<ActionResult<IEnumerable<DriverWithOddsDto>>> GetDriversWithOdds(int raceId)
        {
            _logger.LogInformation("Pobieranie kierowców z kursami dla wyścigu: {RaceId}", raceId);
            var results = await _raceService.GetDriversWithOddsForRaceAsync(raceId);
            return Ok(results);
        }


        /// <summary>
        /// Get the driver championship standings for the current season.
        /// </summary>
        [HttpGet("championship/current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DriverChampionshipDto>>> GetCurrentChampionship()
        {
            int currentSeason = DateTime.UtcNow.Year;
            _logger.LogInformation("Retrieving driver championship standings for the current season: {Season}", currentSeason);

            try
            {
                var championship = await _raceService.GetDriverChampionshipStandingsAsync(currentSeason);
                return Ok(championship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving championship standings for season {Season}", currentSeason);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "CHAMPIONSHIP_DATA_ERROR",
                        Message = "An error occurred while retrieving the current championship standings",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get detailed race history and standings for a specific driver in the current season.
        /// </summary>
        [HttpGet("championship/driver/{driverId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DriverChampionshipDto>> GetDriverChampionship(int driverId)
        {
            int currentSeason = DateTime.UtcNow.Year;
            _logger.LogInformation("Retrieving championship history for driver ID: {DriverId} in season {Season}", driverId, currentSeason);

            try
            {
                var details = await _raceService.GetDriverChampionshipDetailsAsync(driverId, currentSeason);
                if (details == null)
                {
                    return NotFound(new ErrorResponse 
                    { 
                        Error = "DRIVER_CHAMPIONSHIP_NOT_FOUND", 
                        Message = $"Championship data for driver ID {driverId} was not found for the {currentSeason} season." 
                    });
                }
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving championship details for driver ID: {DriverId}", driverId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "CHAMPIONSHIP_DRIVER_ERROR",
                        Message = "An error occurred while retrieving the driver's championship history",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get the driver championship standings for a specific historical season.
        /// </summary>
        [HttpGet("championship/season/{season}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DriverChampionshipDto>>> GetSeasonChampionship(int season)
        {
            _logger.LogInformation("Retrieving driver championship standings for season: {Season}", season);

            try
            {
                var championship = await _raceService.GetDriverChampionshipStandingsAsync(season);
                return Ok(championship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving championship standings for season {Season}", season);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "CHAMPIONSHIP_SEASON_ERROR",
                        Message = $"An error occurred while retrieving the championship standings for the {season} season.",
                        Details = ex.Message
                    });
            }
        }
        

        /// <summary>
        /// Store race results automatically for a finished race (current season only).
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        /// <param name="dto">The race results data with positions and optional fastest lap.</param>
        /// <returns>Success message</returns>
        [HttpPost("{raceId}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> StoreRaceResults(int raceId, [FromBody] StoreRaceResultsDto dto)
        {
            _logger.LogInformation("Storing race results for race: {RaceId}", raceId);

            try
            {
                await _raceService.StoreRaceResultAsync(raceId, dto.Positions, dto.FastestLapDriverId);

                _logger.LogInformation("Race results stored successfully for: {RaceId}", raceId);

                return Ok(new { message = "Race results stored successfully" });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Race not found while storing results: {RaceId}", raceId);
                return NotFound(new ErrorResponse
                {
                    Error = "RACE_NOT_FOUND",
                    Message = $"Race with ID {raceId} not found"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid arguments while storing race results: {RaceId}", raceId);
                return BadRequest(new ErrorResponse
                {
                    Error = "INVALID_INPUT",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing race results: {RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while storing race results",
                        Details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Get stored race results from the RaceResult entity (current season only).
        /// Returns null-compatible result if no stored results exist.
        /// </summary>
        /// <param name="raceId">Race identifier</param>
        /// <returns>Stored race result DTO or null</returns>
        [HttpGet("{raceId}/stored-results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RaceResultDto?>> GetStoredRaceResults(int raceId)
        {
            _logger.LogInformation("Getting stored race results for race: {RaceId}", raceId);

            try
            {
                var result = await _raceService.GetStoredRaceResultAsync(raceId);

                if (result == null)
                {
                    _logger.LogInformation("No stored race results found for: {RaceId}", raceId);
                    return Ok((object)null);
                }

                _logger.LogInformation("Stored race results retrieved for: {RaceId}", raceId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stored race results: {RaceId}", raceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse
                    {
                        Error = "RACE_DATA_ERROR",
                        Message = "An error occurred while retrieving stored race results",
                        Details = ex.Message
                    });
            }
        }
    }
}
