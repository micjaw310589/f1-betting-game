# OpenF1 API Synchronization - Implementation Task

## Overview
Enhance the OpenF1 API integration with robust synchronization capabilities, improved data models, background workers, and conflict resolution mechanisms.

## Requirements
- Robust synchronization of race data from OpenF1
- Handle API failures and rate limits gracefully
- Ensure data consistency between OpenF1 and local database
- Support historical data synchronization
- Provide monitoring and alerting for synchronization issues

## Database Changes

### Extend OpenF1 Data Models
```csharp
// Enhanced OpenF1Race model
public class OpenF1Race
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Circuit { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Season { get; set; }
    public string CircuitId { get; set; } = string.Empty;
    public DateTime? QualifyingDate { get; set; }
    public DateTime? SprintDate { get; set; }
    public string CircuitLayoutUrl { get; set; } = string.Empty;
    public string CircuitImageUrl { get; set; } = string.Empty;
    public int? Laps { get; set; }
    public string CircuitLength { get; set; } = string.Empty;
    public DateTime LastSynced { get; set; }
}

public class OpenF1RaceResult
{
    public int RaceId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Points { get; set; }
    public TimeSpan? FastestLapTime { get; set; }
    public int LapsCompleted { get; set; }
    public string Status { get; set; } = string.Empty; // "Finished", "DNF", "DSQ", etc.
    public TimeSpan? TimeBehindLeader { get; set; }
    public int StartingPosition { get; set; }
    public int PositionsGained { get; set; }
    public string FastestLapSpeed { get; set; } = string.Empty;
}

public class OpenF1DriverStanding
{
    public int Position { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int Poles { get; set; }
    public int FastestLaps { get; set; }
}

public class OpenF1ConstructorStanding
{
    public int Position { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Wins { get; set; }
}

public class OpenF1Session
{
    public string Id { get; set; } = string.Empty;
    public string RaceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // "FP1", "FP2", "FP3", "Qualifying", "Sprint", "Race"
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
}
```

### Add Synchronization Tracking Tables
```csharp
public class OpenF1SyncLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } // "Race", "Result", "Standing", "Session"
    public string EntityId { get; set; }
    public DateTime SyncDate { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public string SourceData { get; set; } // JSON snapshot of raw data
    public string ProcessedData { get; set; } // JSON snapshot of processed data
    public int RetryCount { get; set; }
}

public class RaceSyncStatus
{
    public int RaceId { get; set; }
    public Race Race { get; set; }
    public DateTime? ResultsSyncedAt { get; set; }
    public DateTime? StandingsSyncedAt { get; set; }
    public DateTime? SessionsSyncedAt { get; set; }
    public bool HasDiscrepancies { get; set; }
    public string DiscrepancyNotes { get; set; }
    public DateTime LastSyncAttempt { get; set; }
    public int SyncAttempts { get; set; }
}

public class DriverMapping
{
    public int Id { get; set; }
    public string OpenF1DriverId { get; set; }
    public int? LocalDriverId { get; set; }
    public string OpenF1DriverName { get; set; }
    public string LocalDriverName { get; set; }
    public bool IsManualMapping { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TeamMapping
{
    public int Id { get; set; }
    public string OpenF1TeamName { get; set; }
    public int? LocalTeamId { get; set; }
    public string LocalTeamName { get; set; }
    public bool IsManualMapping { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## Service Layer

### Extend IOpenF1ApiClient
```csharp
public interface IOpenF1ApiClient
{
    // Existing methods
    Task<IEnumerable<OpenF1Race>> GetRacesAsync();
    Task<OpenF1Race?> GetRaceByIdAsync(string raceId);
    Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId);

    // New methods
    Task<IEnumerable<OpenF1RaceResult>> GetRaceResultsAsync(string raceId);
    Task<IEnumerable<OpenF1DriverStanding>> GetDriverStandingsAsync(int season);
    Task<IEnumerable<OpenF1ConstructorStanding>> GetConstructorStandingsAsync(int season);
    Task<OpenF1RaceDetails> GetRaceDetailsAsync(string raceId);
    Task<IEnumerable<OpenF1Session>> GetRaceSessionsAsync(string raceId);
    Task SyncHistoricalDataAsync(int season);
    Task<OpenF1ApiStatus> GetApiStatusAsync();
}
```

### Create IOpenF1SynchronizationService
```csharp
public interface IOpenF1SynchronizationService
{
    Task<SyncResultDto> SyncUpcomingRacesAsync();
    Task<SyncResultDto> SyncRaceResultsAsync(string raceId);
    Task<SyncResultDto> SyncDriverStandingsAsync(int season);
    Task<SyncResultDto> SyncConstructorStandingsAsync(int season);
    Task<SyncResultDto> SyncHistoricalSeasonAsync(int season);
    Task<SyncResultDto> SyncAllMissingDataAsync();
    Task<SyncStatusDto> GetSyncStatusAsync();
    Task<SyncResultDto> SyncRaceSessionsAsync(string raceId);
    Task<SyncResultDto> ResolveDataDiscrepanciesAsync(int raceId);
    Task<SyncResultDto> ManualOverrideRaceResultsAsync(int raceId, ManualOverrideDto overrideData);
    Task<IEnumerable<SyncLogDto>> GetSyncLogsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
```

### Implement OpenF1SynchronizationService
```csharp
public class OpenF1SynchronizationService : IOpenF1SynchronizationService
{
    private readonly IOpenF1ApiClient _apiClient;
    private readonly AppDbContext _context;
    private readonly ILogger<OpenF1SynchronizationService> _logger;
    private readonly IRaceService _raceService;
    private readonly INotificationService _notificationService;

    public OpenF1SynchronizationService(
        IOpenF1ApiClient apiClient,
        AppDbContext context,
        ILogger<OpenF1SynchronizationService> logger,
        IRaceService raceService,
        INotificationService notificationService)
    {
        _apiClient = apiClient;
        _context = context;
        _logger = logger;
        _raceService = raceService;
        _notificationService = notificationService;
    }

    public async Task<SyncResultDto> SyncUpcomingRacesAsync()
    {
        var result = new SyncResultDto
        {
            Operation = "SyncUpcomingRaces",
            StartTime = DateTime.UtcNow,
            ItemsProcessed = 0,
            SuccessCount = 0,
            FailureCount = 0
        };

        try
        {
            // Get races from OpenF1
            var openF1Races = await _apiClient.GetRacesAsync();
            var upcomingRaces = openF1Races.Where(r => r.Date >= DateTime.UtcNow.AddDays(-1));

            foreach (var openF1Race in upcomingRaces)
            {
                try
                {
                    var syncResult = await SyncSingleRaceAsync(openF1Race);
                    result.ItemsProcessed++;
                    if (syncResult.Success) result.SuccessCount++;
                    else result.FailureCount++;
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    _logger.LogError(ex, "Error syncing race {RaceId}", openF1Race.Id);
                    await LogSyncError("Race", openF1Race.Id, ex);
                }
            }

            result.EndTime = DateTime.UtcNow;
            result.Status = "Completed";
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.UtcNow;
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error in SyncUpcomingRaces");
        }

        return result;
    }

    private async Task<SyncResultDto> SyncSingleRaceAsync(OpenF1Race openF1Race)
    {
        var result = new SyncResultDto
        {
            Operation = "SyncSingleRace",
            EntityId = openF1Race.Id,
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Check if race exists
            var existingRace = await _context.Races
                .FirstOrDefaultAsync(r => r.OpenF1RaceId == openF1Race.Id);

            if (existingRace == null)
            {
                // Create new race
                existingRace = new Race(
                    openF1Race.Name,
                    openF1Race.Date,
                    openF1Race.Circuit,
                    openF1Race.Country,
                    openF1Race.Id,
                    openF1Race.Season
                );

                _context.Races.Add(existingRace);
                result.ItemsProcessed++;
                result.SuccessCount++;
                result.Status = "Created";
            }
            else
            {
                // Update existing race if needed
                var needsUpdate = CheckRaceNeedsUpdate(existingRace, openF1Race);
                if (needsUpdate)
                {
                    UpdateRaceFromOpenF1(existingRace, openF1Race);
                    result.ItemsProcessed++;
                    result.SuccessCount++;
                    result.Status = "Updated";
                }
                else
                {
                    result.Status = "NoChanges";
                }
            }

            await _context.SaveChangesAsync();
            result.EndTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.UtcNow;
            result.Status = "Failed";
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error syncing race {RaceId}", openF1Race.Id);
        }

        return result;
    }
}
```

## Background Workers

### Create OpenF1SynchronizationJob
```csharp
public class OpenF1SynchronizationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OpenF1SynchronizationJob> _logger;
    private readonly IConfiguration _configuration;

    // Configurable intervals
    private TimeSpan _fullSyncInterval = TimeSpan.FromHours(6);
    private TimeSpan _raceCheckInterval = TimeSpan.FromMinutes(30);
    private TimeSpan _recentRacesInterval = TimeSpan.FromMinutes(15);

    public OpenF1SynchronizationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<OpenF1SynchronizationJob> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;

        // Load intervals from configuration
        ConfigureIntervals();
    }

    private void ConfigureIntervals()
    {
        _fullSyncInterval = TimeSpan.FromHours(
            _configuration.GetValue<int>("OpenF1Sync:FullSyncIntervalHours", 6));

        _raceCheckInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<int>("OpenF1Sync:RaceCheckIntervalMinutes", 30));

        _recentRacesInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<int>("OpenF1Sync:RecentRacesIntervalMinutes", 15));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OpenF1SynchronizationJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IOpenF1SynchronizationService>();
                var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();

                // 1. Check API status
                var apiStatus = await CheckApiStatusAsync(scope);
                if (!apiStatus.IsAvailable)
                {
                    await Task.Delay(_raceCheckInterval, stoppingToken);
                    continue;
                }

                // 2. Sync upcoming races (frequent)
                _logger.LogInformation("Starting upcoming races sync...");
                var upcomingResult = await syncService.SyncUpcomingRacesAsync();
                LogSyncResult(upcomingResult);

                // 3. Check for recently finished races that need results
                _logger.LogInformation("Checking for races needing result sync...");
                await CheckAndSyncRecentRaceResultsAsync(scope, stoppingToken);

                // 4. Periodically sync driver standings
                if (ShouldSyncStandings())
                {
                    _logger.LogInformation("Starting driver standings sync...");
                    var currentSeason = DateTime.UtcNow.Year;
                    var standingsResult = await syncService.SyncDriverStandingsAsync(currentSeason);
                    LogSyncResult(standingsResult);
                }

                // 5. Periodically do full sync
                if (ShouldDoFullSync())
                {
                    _logger.LogInformation("Starting full synchronization...");
                    await PerformFullSyncAsync(scope, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OpenF1SynchronizationJob");
            }

            // Wait for next cycle
            await Task.Delay(_raceCheckInterval, stoppingToken);
        }

        _logger.LogInformation("OpenF1SynchronizationJob stopped");
    }

    private async Task CheckAndSyncRecentRaceResultsAsync(IServiceScope scope, CancellationToken stoppingToken)
    {
        var syncService = scope.ServiceProvider.GetRequiredService<IOpenF1SynchronizationService>();
        var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();

        // Find races that are finished but don't have results synced
        var racesNeedingResults = await _context.Races
            .Where(r => r.Status == RaceStatus.Finished)
            .Where(r => !_context.RaceSyncStatuses.Any(s => s.RaceId == r.Id && s.ResultsSyncedAt != null))
            .OrderByDescending(r => r.Date)
            .Take(5) // Limit to most recent 5
            .ToListAsync(stoppingToken);

        foreach (var race in racesNeedingResults)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                _logger.LogInformation("Syncing results for race {RaceId} - {RaceName}", race.Id, race.Name);
                var result = await syncService.SyncRaceResultsAsync(race.OpenF1RaceId);
                LogSyncResult(result);

                if (result.Success)
                {
                    // Update race status to trigger bet processing
                    await raceService.UpdateRaceStatusAsync(race.Id, RaceStatus.ResultsProcessed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing results for race {RaceId}", race.Id);
            }
        }
    }
}
```

### Enhance Existing RaceStatusMonitorJob
```csharp
// Add to RaceStatusMonitorJob constructor
public RaceStatusMonitorJob(
    ILogger<RaceStatusMonitorJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    IOpenF1SynchronizationService synchronizationService)
{
    // ... existing code ...
    _synchronizationService = synchronizationService;
}

// Modify ProcessFinishedRacesAsync to use enhanced synchronization
private async Task ProcessFinishedRacesAsync(CancellationToken stoppingToken)
{
    // ... existing race finding logic ...

    foreach (var race in finishedRaces)
    {
        if (stoppingToken.IsCancellationRequested) break;

        try
        {
            // Check if we have results synced
            var syncStatus = await _context.RaceSyncStatuses
                .FirstOrDefaultAsync(s => s.RaceId == race.Id);

            if (syncStatus == null || syncStatus.ResultsSyncedAt == null)
            {
                // Try to sync results from OpenF1
                _logger.LogInformation("Attempting to sync results for race {RaceId}", race.Id);
                var syncResult = await _synchronizationService.SyncRaceResultsAsync(race.OpenF1RaceId);

                if (!syncResult.Success)
                {
                    _logger.LogWarning("Failed to sync results for race {RaceId}: {Error}",
                        race.Id, syncResult.ErrorMessage);
                    continue;
                }
            }

            // Proceed with bet processing
            await _bettingService.ProcessRaceResultsAsync(race.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing results for race ID {RaceId}.", race.Id);
        }
    }
}
```

## API Endpoints

### Add OpenF1Controller
```csharp
[ApiController]
[Route("api/admin/openf1")]
[Authorize(Roles = "Admin")]
public class OpenF1Controller : ControllerBase
{
    private readonly IOpenF1SynchronizationService _syncService;
    private readonly IOpenF1ApiClient _apiClient;

    public OpenF1Controller(
        IOpenF1SynchronizationService syncService,
        IOpenF1ApiClient apiClient)
    {
        _syncService = syncService;
        _apiClient = apiClient;
    }

    [HttpGet("sync/status")]
    public async Task<ActionResult<SyncStatusDto>> GetSyncStatus()
    {
        var status = await _syncService.GetSyncStatusAsync();
        return Ok(status);
    }

    [HttpPost("sync/races")]
    public async Task<ActionResult<SyncResultDto>> SyncUpcomingRaces()
    {
        var result = await _syncService.SyncUpcomingRacesAsync();
        return Ok(result);
    }

    [HttpPost("sync/results/{raceId}")]
    public async Task<ActionResult<SyncResultDto>> SyncRaceResults(string raceId)
    {
        var result = await _syncService.SyncRaceResultsAsync(raceId);
        return Ok(result);
    }

    [HttpPost("sync/standings/{season}")]
    public async Task<ActionResult<SyncResultDto>> SyncDriverStandings(int season)
    {
        var result = await _syncService.SyncDriverStandingsAsync(season);
        return Ok(result);
    }

    [HttpPost("sync/season/{season}")]
    public async Task<ActionResult<SyncResultDto>> SyncHistoricalSeason(int season)
    {
        var result = await _syncService.SyncHistoricalSeasonAsync(season);
        return Ok(result);
    }

    [HttpPost("sync/all")]
    public async Task<ActionResult<SyncResultDto>> SyncAllMissingData()
    {
        var result = await _syncService.SyncAllMissingDataAsync();
        return Ok(result);
    }

    [HttpGet("sync/logs")]
    public async Task<ActionResult<PagedResult<SyncLogDto>>> GetSyncLogs(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var logs = await _syncService.GetSyncLogsAsync(fromDate, toDate);
        var pagedResult = new PagedResult<SyncLogDto>(
            logs.Skip(offset).Take(limit),
            logs.Count(),
            offset,
            limit);

        return Ok(pagedResult);
    }

    [HttpPost("resolve/{raceId}")]
    public async Task<ActionResult<SyncResultDto>> ResolveDiscrepancies(int raceId)
    {
        var result = await _syncService.ResolveDataDiscrepanciesAsync(raceId);
        return Ok(result);
    }

    [HttpPost("override/{raceId}")]
    public async Task<ActionResult<SyncResultDto>> ManualOverride(
        int raceId,
        [FromBody] ManualOverrideDto overrideData)
    {
        var result = await _syncService.ManualOverrideRaceResultsAsync(raceId, overrideData);
        return Ok(result);
    }

    [HttpGet("api/status")]
    public async Task<ActionResult<OpenF1ApiStatus>> GetApiStatus()
    {
        var status = await _apiClient.GetApiStatusAsync();
        return Ok(status);
    }
}
```

## DTOs

### SyncResultDto
```csharp
public class SyncResultDto
{
    public string Operation { get; set; }
    public string EntityId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } // "Completed", "Failed", "Partial", "NoChanges"
    public int ItemsProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string ErrorMessage { get; set; }
    public List<SyncItemResultDto> ItemResults { get; set; } = new();
}

public class SyncItemResultDto
{
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string Status { get; set; }
    public string Error { get; set; }
}
```

### SyncStatusDto
```csharp
public class SyncStatusDto
{
    public DateTime LastFullSync { get; set; }
    public DateTime LastRaceSync { get; set; }
    public DateTime LastStandingsSync { get; set; }
    public int RacesInSync { get; set; }
    public int RacesNeedingSync { get; set; }
    public int RacesWithDiscrepancies { get; set; }
    public bool IsSyncing { get; set; }
    public string CurrentSyncOperation { get; set; }
    public DateTime? CurrentSyncStartTime { get; set; }
    public OpenF1ApiStatus ApiStatus { get; set; }
}

public class OpenF1ApiStatus
{
    public bool IsAvailable { get; set; }
    public DateTime LastChecked { get; set; }
    public string StatusMessage { get; set; }
    public int ResponseTimeMs { get; set; }
    public string ApiVersion { get; set; }
}
```

### ManualOverrideDto
```csharp
public class ManualOverrideDto
{
    public List<ManualResultOverrideDto> Results { get; set; } = new();
    public string Notes { get; set; }
    public bool ForceOverride { get; set; }
}

public class ManualResultOverrideDto
{
    public int DriverId { get; set; }
    public int Position { get; set; }
    public int Points { get; set; }
    public string Status { get; set; }
    public TimeSpan? FastestLap { get; set; }
}
```

## Data Consistency Features

### Conflict Resolution
```csharp
public async Task<SyncResultDto> ResolveDataDiscrepanciesAsync(int raceId)
{
    var result = new SyncResultDto
    {
        Operation = "ResolveDiscrepancies",
        EntityId = raceId.ToString(),
        StartTime = DateTime.UtcNow
    };

    try
    {
        var race = await _context.Races.FindAsync(raceId);
        if (race == null)
        {
            result.Status = "Failed";
            result.ErrorMessage = "Race not found";
            return result;
        }

        // Get current results from OpenF1
        var openF1Results = await _apiClient.GetRaceResultsAsync(race.OpenF1RaceId);
        if (openF1Results == null || !openF1Results.Any())
        {
            result.Status = "Failed";
            result.ErrorMessage = "No results available from OpenF1";
            return result;
        }

        // Get current local results
        var localResults = await _context.Results
            .Where(r => r.RaceId == raceId)
            .ToListAsync();

        // Compare and identify discrepancies
        var discrepancies = FindDiscrepancies(localResults, openF1Results);

        if (!discrepancies.Any())
        {
            result.Status = "NoDiscrepancies";
            return result;
        }

        // Log discrepancies for admin review
        var syncStatus = await _context.RaceSyncStatuses
            .FirstOrDefaultAsync(s => s.RaceId == raceId) ??
            new RaceSyncStatus { RaceId = raceId };

        syncStatus.HasDiscrepancies = true;
        syncStatus.DiscrepancyNotes = JsonSerializer.Serialize(discrepancies);
        syncStatus.LastSyncAttempt = DateTime.UtcNow;
        syncStatus.SyncAttempts++;

        if (syncStatus.SyncAttempts >= 3)
        {
            // Notify admins after 3 failed attempts
            await _notificationService.SendAdminNotificationAsync(
                "OpenF1 Data Discrepancy",
                $"Race {race.Name} has persistent data discrepancies. Manual review required.");
        }

        await _context.SaveChangesAsync();

        result.Status = "DiscrepanciesFound";
        result.ItemsProcessed = discrepancies.Count;
        result.ItemResults = discrepancies.Select(d => new SyncItemResultDto
        {
            ItemId = d.DriverId.ToString(),
            ItemName = d.DriverName,
            Status = "Discrepancy",
            Error = d.DiscrepancyDetails
        }).ToList();

        return result;
    }
    catch (Exception ex)
    {
        result.Status = "Failed";
        result.ErrorMessage = ex.Message;
        _logger.LogError(ex, "Error resolving discrepancies for race {RaceId}", raceId);
        return result;
    }
    finally
    {
        result.EndTime = DateTime.UtcNow;
    }
}

private List<DiscrepancyDto> FindDiscrepancies(
    List<Result> localResults,
    IEnumerable<OpenF1RaceResult> openF1Results)
{
    var discrepancies = new List<DiscrepancyDto>();

    foreach (var openF1Result in openF1Results)
    {
        var localResult = localResults.FirstOrDefault(r =>
            r.Driver.OpenF1DriverId == openF1Result.DriverId.ToString());

        if (localResult == null)
        {
            discrepancies.Add(new DiscrepancyDto
            {
                DriverId = openF1Result.DriverId,
                DriverName = openF1Result.DriverName,
                DiscrepancyType = "MissingLocalResult",
                DiscrepancyDetails = "Result exists in OpenF1 but not locally"
            });
            continue;
        }

        // Compare key fields
        if (localResult.Position != openF1Result.Position)
        {
            discrepancies.Add(new DiscrepancyDto
            {
                DriverId = openF1Result.DriverId,
                DriverName = openF1Result.DriverName,
                DiscrepancyType = "PositionMismatch",
                DiscrepancyDetails = $"Local: {localResult.Position}, OpenF1: {openF1Result.Position}"
            });
        }

        if (localResult.Points != openF1Result.Points)
        {
            discrepancies.Add(new DiscrepancyDto
            {
                DriverId = openF1Result.DriverId,
                DriverName = openF1Result.DriverName,
                DiscrepancyType = "PointsMismatch",
                DiscrepancyDetails = $"Local: {localResult.Points}, OpenF1: {openF1Result.Points}"
            });
        }

        // Add more comparisons as needed...
    }

    return discrepancies;
}
```

### Idempotent Operations
```csharp
// Example of idempotent race result sync
public async Task<SyncResultDto> SyncRaceResultsAsync(string raceId)
{
    // Check if we've already successfully synced this race
    var existingSync = await _context.OpenF1SyncLogs
        .Where(s => s.EntityType == "RaceResults" && s.EntityId == raceId && s.Success)
        .OrderByDescending(s => s.SyncDate)
        .FirstOrDefaultAsync();

    if (existingSync != null && existingSync.SyncDate > DateTime.UtcNow.AddHours(-1))
    {
        return new SyncResultDto
        {
            Operation = "SyncRaceResults",
            EntityId = raceId,
            Status = "NoChanges",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            ErrorMessage = "Race results already synced recently"
        };
    }

    // Proceed with sync...
}
```

## Monitoring and Alerting

### Add Health Checks
```csharp
public class OpenF1HealthCheck : IHealthCheck
{
    private readonly IOpenF1ApiClient _apiClient;

    public OpenF1HealthCheck(IOpenF1ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _apiClient.GetApiStatusAsync();

            if (status.IsAvailable && status.ResponseTimeMs < 2000)
            {
                return HealthCheckResult.Healthy(
                    $"OpenF1 API is healthy. Response time: {status.ResponseTimeMs}ms");
            }

            return HealthCheckResult.Degraded(
                $"OpenF1 API is degraded. Response time: {status.ResponseTimeMs}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"OpenF1 API is unavailable: {ex.Message}");
        }
    }
}
```

### Configure in Program.cs
```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<OpenF1HealthCheck>("OpenF1_API")
    .AddDbContextCheck<AppDbContext>("Database");

// Add health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        });

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});
```

## Testing Requirements

### Unit Tests
- Test synchronization logic for different scenarios
- Test conflict detection and resolution
- Test idempotent operations
- Test retry logic

### Integration Tests
- Test full synchronization workflow
- Test API client error handling
- Test background worker behavior
- Test health checks

### Load Tests
- Test performance with large historical data sets
- Test rate limiting behavior
- Test concurrent synchronization requests

## Success Criteria
- Synchronization completes successfully for 95%+ of races
- Data discrepancies are detected and flagged for review
- API failures are handled gracefully with retries
- Manual override functionality works correctly
- Health checks accurately reflect system status

## Out of Scope

### Do NOT Modify
- **Core Race Processing Logic**: The existing race status workflows and bet processing in `RaceStatusMonitorJob`
- **Bet Settlement Logic**: The core bet settlement and payout calculation mechanisms
- **User Points System**: The fundamental user points system and transaction mechanisms
- **Authentication System**: User authentication, authorization, and identity management
- **Database Migrations**: Existing database migrations for current tables
- **Entity Framework Core**: The existing DbContext configuration and entity relationships

### Avoid Changes To
- **Existing API Endpoints**: Do not modify current race-related or bet-related endpoints
- **Current Frontend Components**: Do not rewrite existing race listing or detail pages
- **Caching Infrastructure**: Use existing caching patterns, don't replace the caching system
- **Logging Framework**: Use existing logging services and patterns
- **Error Handling**: Use existing error handling middleware and patterns
- **Notification System**: Use existing notification service, extend it but don't rebuild

### Integration Only
- **RaceStatusMonitorJob**: Enhance with OpenF1 synchronization calls, don't rewrite core logic
- **IRaceService**: Extend with synchronization-related methods, don't modify existing ones
- **Existing Services**: Use existing services (BettingService, UserService) as-is
- **Frontend Services**: Use existing API service patterns for new synchronization endpoints
- **Background Workers**: Add new synchronization worker, don't modify existing workers
- **API Architecture**: Follow existing controller patterns and routing conventions

## Estimated Effort
- Database: 3 days
- API Client Enhancement: 3 days
- Synchronization Service: 5 days
- Background Workers: 4 days
- API Endpoints: 2 days
- Monitoring/Alerting: 2 days
- Testing: 4 days
- **Total: 23 days**
