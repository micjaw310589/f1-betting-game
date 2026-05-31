# Race Results Display - Implementation Task

## Overview
Display race results for finished races on the race detail page and allow admin overrides, scoped to the current season only.

## Requirements
- Store only race results for finished races from the current season in the database.
- Display race results on `/races/{id}` route, below the race description, but only when the race status is `Finished` or `ResultsProcessed`.
- Admin race results override feature must functionally match the existing override workflow (positions + optional fastest lap).

## Database Changes

### RaceResult Entity
Create a new entity to store only finished race results for the current season:

```csharp
public class RaceResult
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public int Season { get; set; }
    public ICollection<RaceResultPosition> Positions { get; set; } = new List<RaceResultPosition>();
    public int? FastestLapDriverId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RaceResultPosition
{
    public int Id { get; set; }
    public int RaceResultId { get; set; }
    public int Position { get; set; }
    public int DriverId { get; set; }
    public int TeamId { get; set; }
    public int Points { get; set; }
}
```

### Constraints
- Only one `RaceResult` per race (`RaceId` unique index).
- Only results for the current season are stored.
- No historical cross-season queries.

### Migration
- Add `RaceResult` and `RaceResultPosition` tables.
- Add `RaceResultId` foreign key to `Race` entity (optional, nullable) to link results.

## Service Layer

### Extend IRaceService
```csharp
/// <summary>
/// Get race results for a race (current season only).
/// Returns null if the race has no results or is not from the current season.
/// </summary>
Task<RaceResultDto?> GetRaceResultDtoAsync(int raceId);

/// <summary>
/// Override race results manually (admin only).
/// Sets IsManuallyOverridden on the race.
/// </summary>
Task OverrideRaceResultAsync(int raceId, OverrideRaceResultDto dto);

/// <summary>
/// Store race results in the database for finished races from the current season.
/// Called after race completion or via admin override.
/// </summary>
Task StoreRaceResultAsync(int raceId, List<RaceResultPositionDto> positions, int? fastestLapDriverId = null);
```

### Implement RaceService Methods

```csharp
public async Task<RaceResultDto?> GetRaceResultDtoAsync(int raceId)
{
    var currentSeason = DateTime.UtcNow.Year;
    
    var raceResult = await _dbContext.RaceResults
        .Include(r => r.Positions)
        .Where(r => r.RaceId == raceId && r.Season == currentSeason)
        .FirstOrDefaultAsync();
    
    if (raceResult == null)
        return null;
    
    return MapToRaceResultDto(raceResult);
}

public async Task OverrideRaceResultAsync(int raceId, OverrideRaceResultDto dto)
{
    var race = await _dbContext.Races.FindAsync(raceId);
    if (race == null)
        throw new RaceNotFoundException($"Race with ID {raceId} not found");
    
    if (dto.Positions == null || !dto.Positions.Any())
        throw new ArgumentException("At least one position entry is required.");
    
    // Mark as manually overridden
    race.IsManuallyOverridden = true;
    
    // Store/Update race results
    var currentSeason = DateTime.UtcNow.Year;
    var raceResult = await _dbContext.RaceResults
        .Include(r => r.Positions)
        .FirstOrDefaultAsync(r => r.RaceId == raceId);
    
    if (raceResult == null)
    {
        raceResult = new RaceResult
        {
            RaceId = raceId,
            Season = currentSeason,
            Positions = new List<RaceResultPosition>()
        };
        _dbContext.RaceResults.Add(raceResult);
    }
    
    // Update positions
    raceResult.Positions.Clear();
    foreach (var entry in dto.Positions.OrderBy(p => p.Position))
    {
        raceResult.Positions.Add(new RaceResultPosition
        {
            Position = entry.Position,
            DriverId = entry.DriverId
        });
    }
    
    raceResult.FastestLapDriverId = dto.FastestLapDriverId;
    raceResult.UpdatedAt = DateTime.UtcNow;
    
    await _dbContext.SaveChangesAsync();
}

public async Task StoreRaceResultAsync(int raceId, List<RaceResultPositionDto> positions, int? fastestLapDriverId = null)
{
    var race = await _dbContext.Races.FindAsync(raceId);
    if (race == null)
        throw new RaceNotFoundException($"Race with ID {raceId} not found");
    
    var currentSeason = DateTime.UtcNow.Year;
    
    // Only store results for current season
    if (race.Season != currentSeason)
        return;
    
    var raceResult = await _dbContext.RaceResults
        .Include(r => r.Positions)
        .FirstOrDefaultAsync(r => r.RaceId == raceId);
    
    if (raceResult == null)
    {
        raceResult = new RaceResult
        {
            RaceId = raceId,
            Season = currentSeason,
            Positions = new List<RaceResultPosition>(),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.RaceResults.Add(raceResult);
    }
    
    raceResult.Positions.Clear();
    foreach (var pos in positions.OrderBy(p => p.Position))
    {
        raceResult.Positions.Add(new RaceResultPosition
        {
            Position = pos.Position,
            DriverId = pos.DriverId,
            TeamId = pos.TeamId,
            Points = pos.Points
        });
    }
    
    raceResult.FastestLapDriverId = fastestLapDriverId;
    raceResult.UpdatedAt = DateTime.UtcNow;
    
    await _dbContext.SaveChangesAsync();
}
```

## API Endpoints

### Existing Endpoints (No Changes Required)

#### Admin: Get Race Results
```
GET /api/admin/races/{raceId}/results
```
Returns `RaceResultDto` for admin view.

#### Admin: Override Race Results
```
PUT /api/admin/races/{raceId}/results
```
Accepts `OverrideRaceResultDto` with `Positions` and optional `FastestLapDriverId`.

#### Public: Get Race Results
```
GET /api/races/{raceId}/results
```
Returns `RaceResultDto` if the race has results.

### New Endpoint: Store Race Results Automatically

```
POST /api/races/{raceId}/results
```

```csharp
[HttpPost("{raceId}/results")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult> StoreRaceResults(int raceId, [FromBody] StoreRaceResultsDto dto)
{
    await _raceService.StoreRaceResultAsync(raceId, dto.Positions, dto.FastestLapDriverId);
    return Ok(new { message = "Race results stored successfully" });
}
```

```csharp
public class StoreRaceResultsDto
{
    [JsonPropertyName("positions")]
    public List<PositionEntryDto> Positions { get; set; } = new();
    
    [JsonPropertyName("fastestLapDriverId")]
    public int? FastestLapDriverId { get; set; }
}
```

## DTOs

### RaceResultDto (existing, no changes)
```csharp
public class RaceResultDto
{
    public int RaceId { get; set; }
    public string RaceName { get; set; }
    public string Circuit { get; set; }
    public string Country { get; set; }
    public DateTime RaceDate { get; set; }
    public int WinnerDriverId { get; set; }
    public string WinnerDriverName { get; set; }
    public int WinnerTeamId { get; set; }
    public string WinnerTeamName { get; set; }
    [JsonPropertyName("fastestLapDriverId")]
    public int? FastestLapDriverId { get; set; }
    [JsonPropertyName("fastestLapDriverName")]
    public string FastestLapDriverName { get; set; }
    [JsonPropertyName("positions")]
    public List<PositionDto> Positions { get; set; } = new();
}

public class PositionDto
{
    [JsonPropertyName("position")]
    public int Position { get; set; }
    [JsonPropertyName("driverId")]
    public int DriverId { get; set; }
    [JsonPropertyName("driverName")]
    public string DriverName { get; set; } = string.Empty;
    [JsonPropertyName("teamId")]
    public int TeamId { get; set; }
    [JsonPropertyName("teamName")]
    public string TeamName { get; set; } = string.Empty;
    [JsonPropertyName("points")]
    public int Points { get; set; }
    [JsonPropertyName("fastestLap")]
    public TimeSpan? FastestLap { get; set; }
}
```

## Frontend Integration

### Race Detail Page (`/races/{id}`)
- Display race results below the race description card.
- Only render the race results section if: the race status is `Finished` or `ResultsProcessed` AND the race is from the current season.
- Display a results table with columns: Position, Driver, Team, Points.
- Show fastest lap indicator next to the driver's name.
- No expandable rows, no comparison feature, no visualizations.

### Admin Panel Override
- The existing admin override feature at `/api/admin/races/{raceId}/results` (PUT) must functionally:
  - Accept a list of finishing positions with driver IDs.
  - Accept an optional fastest lap driver ID.
  - Set `IsManuallyOverridden` to `true` on the race.
  - Store/update the `RaceResult` entity in the database.
- No changes to the admin override API contract; only ensure database persistence matches.

### No Other Frontend Features
- No `/results/history` route.
- No "Compare Drivers" feature.
- No circuit maps, lap charts, or delta charts.
- No infinite scroll, pagination, or filtering for results.

## Database Maintenance

### Season Purge Job
- Add a periodic background job that:
  - Identifies `RaceResult` entries for seasons older than the current season.
  - Deletes those entries and their positions.
  - Runs at the start of a new year.

```csharp
public class RaceResultPurgeJob : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Run on a schedule
        await PurgeOldResults();
    }
    
    private async Task PurgeOldResults()
    {
        var currentSeason = DateTime.UtcNow.Year;
        var oldResults = await _dbContext.RaceResults
            .Where(r => r.Season < currentSeason)
            .ToListAsync();
        
        _dbContext.RaceResults.RemoveRange(oldResults);
        await _dbContext.SaveChangesAsync();
    }
}
```

## Caching Strategy

### Implementation
```csharp
// In RaceService
public async Task<RaceResultDto?> GetRaceResultDtoAsync(int raceId)
{
    var cacheKey = $"race_result_{raceId}";
    var cachedResult = await _cacheService.GetAsync<RaceResultDto>(cacheKey);
    
    if (cachedResult != null)
        return cachedResult;
    
    var result = await FetchRaceResultFromDb(raceId);
    
    // Cache for 1 hour
    await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
    
    return result;
}
```

### Cache Invalidation
- Invalidate `race_result_{raceId}` cache when:
  - Admin overrides results (PUT `/api/admin/races/{raceId}/results`).
  - Results are stored (POST `/api/races/{raceId}/results`).

## Testing Requirements

### Unit Tests
- Test `GetRaceResultDtoAsync` returns null for races without results.
- Test `GetRaceResultDtoAsync` filters by current season only.
- Test `OverrideRaceResultAsync` sets `IsManuallyOverridden = true`.
- Test `StoreRaceResultAsync` skips old season races.
- Test cache invalidation logic.

### Integration Tests
- Test full result retrieval flow (API → Service → DB).
- Test override flow persists to database.
- Test season purge deletes old results.

## Success Criteria
- Race results display on `/races/{id}` only for `Finished`/`ResultsProcessed` races from current season.
- Admin override correctly stores and persists results.
- Database stores only current season results.
- Results load within 2 seconds with caching.

## Dependencies
- Race entity must have `Status` and `Season` fields.
- Driver and Team data must be available for display.
- OpenF1 synchronization populates raw data before results are stored.

## Out of Scope

### Do NOT Modify
- **Core Race Entity**: The fundamental `Race` entity structure and relationships (except adding `RaceResultId` FK).
- **Bet Processing Logic**: The core bet settlement logic.
- **Race Status Workflow**: Existing race status transitions.
- **Authentication System**: User authentication and authorization mechanisms.

### Avoid Changes To
- **Existing API Endpoints**: Do not modify current admin endpoint contracts.
- **Existing Frontend Routing**: Only add to the race detail page; do not create new routes.
- **Caching Infrastructure**: Use existing caching patterns.
- **Logging Framework**: Use existing logging services.
- **Error Handling**: Use existing error handling middleware.

### Integration Only
- **IRaceService**: Extend with new methods, don't modify existing ones.
- **Frontend**: Only add results display to `/races/{id}`; no additional pages.

## Estimated Effort
- Database: 1 day
- Service Layer: 2 days
- API Endpoints: 1 day
- Frontend: 2 days
- Testing: 1 day
- **Total: 7 days**