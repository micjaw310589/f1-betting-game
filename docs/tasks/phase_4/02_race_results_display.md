# Race Results Display - Implementation Task

## Overview
Enhance the race results display system to provide detailed, comprehensive race results for finished races with historical data support.

## Requirements
- Display detailed results for completed races
- Show driver positions, points earned, fastest laps, time behind leader, etc.
- Include historical race results with filtering options
- Support both summary and detailed views

## Database Changes

### Extend Result Entity
```csharp
// Add to existing Result entity:
public TimeSpan? TimeBehindLeader { get; set; }
public int LapsCompleted { get; set; }
public string Status { get; set; } // "Finished", "DNF", "DSQ", "Lapped", etc.
public int StartingPosition { get; set; }
public int PositionsGained { get; set; }
```

### Add RaceResultSummary Table (Optional for caching)
```csharp
public class RaceResultSummary
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public Race Race { get; set; }
    public int WinnerDriverId { get; set; }
    public string WinnerDriverName { get; set; }
    public int PolePositionDriverId { get; set; }
    public string PolePositionDriverName { get; set; }
    public TimeSpan? FastestLapTime { get; set; }
    public int FastestLapDriverId { get; set; }
    public string FastestLapDriverName { get; set; }
    public int TotalFinishers { get; set; }
    public int TotalDNF { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

## Service Layer

### Extend IRaceService
```csharp
Task<IEnumerable<RaceResultDto>> GetRaceResultsAsync(int raceId);
Task<RaceResultDetailsDto> GetRaceResultDetailsAsync(int raceId);
Task<IEnumerable<RaceSummaryDto>> GetHistoricalRaceResultsAsync(int season, int limit = 10);
Task<RaceResultSummaryDto> GetRaceResultSummaryAsync(int raceId);
Task CacheRaceResultSummaryAsync(int raceId);
```

### Implement RaceService Methods
```csharp
public async Task<IEnumerable<RaceResultDto>> GetRaceResultsAsync(int raceId)
{
    // Query results with driver and team information
    // Calculate additional fields like PositionsGained
    // Map to DTO and return sorted by position
}

public async Task<RaceResultDetailsDto> GetRaceResultDetailsAsync(int raceId)
{
    // Get full race details including:
    // - Race information
    // - Complete results with all statistics
    // - Qualifying results (if available)
    // - Fastest laps, pit stops, etc.
}
```

## API Endpoints

### Add to RacesController
```csharp
[HttpGet("{raceId}/results")]
public async Task<ActionResult<IEnumerable<RaceResultDto>>> GetRaceResults(int raceId)
{
    var results = await _raceService.GetRaceResultsAsync(raceId);
    return Ok(results);
}

[HttpGet("{raceId}/results/detailed")]
public async Task<ActionResult<RaceResultDetailsDto>> GetRaceResultDetails(int raceId)
{
    var details = await _raceService.GetRaceResultDetailsAsync(raceId);
    return Ok(details);
}

[HttpGet("season/{season}/results")]
public async Task<ActionResult<IEnumerable<RaceSummaryDto>>> GetSeasonResults(int season, [FromQuery] int limit = 10)
{
    var results = await _raceService.GetHistoricalRaceResultsAsync(season, limit);
    return Ok(results);
}

[HttpGet("{raceId}/results/summary")]
public async Task<ActionResult<RaceResultSummaryDto>> GetRaceResultSummary(int raceId)
{
    var summary = await _raceService.GetRaceResultSummaryAsync(raceId);
    return Ok(summary);
}
```

## DTOs

### RaceResultDto
```csharp
public class RaceResultDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; }
    public string TeamName { get; set; }
    public int Position { get; set; }
    public int StartingPosition { get; set; }
    public int PositionsGained { get; set; }
    public int Points { get; set; }
    public TimeSpan? FastestLap { get; set; }
    public TimeSpan? TimeBehindLeader { get; set; }
    public int LapsCompleted { get; set; }
    public string Status { get; set; }
    public bool IsPodium { get; set; }
    public bool IsFastestLap { get; set; }
}
```

### RaceResultDetailsDto
```csharp
public class RaceResultDetailsDto
{
    public int RaceId { get; set; }
    public string RaceName { get; set; }
    public DateTime RaceDate { get; set; }
    public string Circuit { get; set; }
    public string Country { get; set; }
    public IEnumerable<RaceResultDto> Results { get; set; }
    public RaceResultSummaryDto Summary { get; set; }
    public IEnumerable<QualifyingResultDto> QualifyingResults { get; set; }
}
```

### RaceSummaryDto
```csharp
public class RaceSummaryDto
{
    public int RaceId { get; set; }
    public string RaceName { get; set; }
    public DateTime RaceDate { get; set; }
    public string WinnerDriverName { get; set; }
    public string WinnerTeamName { get; set; }
    public string Circuit { get; set; }
    public string Country { get; set; }
}
```

## Frontend Integration

### Race Results Page
- Create `/races/{raceId}/results` route
- Display race header with circuit information
- Show results table with columns: Position, Driver, Team, Points, Time Behind, Status
- Add visual indicators for podium finishes (gold/silver/bronze)
- Implement expandable rows for detailed driver statistics

### Historical Results Page
- Create `/results/history` route
- Add season selector dropdown
- Display grid of races with winner information
- Implement infinite scroll or pagination
- Add filtering by circuit, driver, team

### Result Comparison Feature
- Add "Compare Drivers" button that opens modal
- Allow selecting 2-4 drivers to compare
- Show side-by-side comparison of race performance
- Highlight key differences

### Enhanced Visualizations
- Add circuit map with driver positions overlay
- Implement lap chart showing position changes
- Add time delta chart for race leaders

## Caching Strategy

### Implementation
```csharp
// In RaceService
public async Task<RaceResultDetailsDto> GetRaceResultDetailsAsync(int raceId)
{
    var cacheKey = $"race_results_{raceId}";
    var cachedResult = await _cacheService.GetAsync<RaceResultDetailsDto>(cacheKey);

    if (cachedResult != null)
        return cachedResult;

    // Fetch and process data
    var result = await FetchAndProcessRaceResults(raceId);

    // Cache for 24 hours for old races, 5 minutes for recent races
    var cacheDuration = IsRecentRace(raceId) ? TimeSpan.FromMinutes(5) : TimeSpan.FromHours(24);
    await _cacheService.SetAsync(cacheKey, result, cacheDuration);

    return result;
}
```

### Cache Invalidation
- Invalidate cache when race results are updated
- Invalidate cache when manual overrides occur
- Implement cache stampede protection

## Testing Requirements

### Unit Tests
- Test result calculation logic (positions gained, etc.)
- Test DTO mapping with various scenarios
- Test caching behavior

### Integration Tests
- Test full result retrieval flow
- Test historical data queries
- Test cache invalidation

### UI Tests
- Test responsive design on different devices
- Test result comparison functionality
- Test visualizations render correctly

## Success Criteria
- Race results load within 2 seconds for cached data, 5 seconds for uncached
- Historical results can be filtered and searched efficiently
- Visualizations enhance user understanding of race dynamics
- UI works correctly on mobile, tablet, and desktop

## Dependencies
- Race and Result entities must be properly populated
- Driver and Team data must be available
- OpenF1 synchronization should be implemented for complete data

## Out of Scope

### Do NOT Modify
- **Core Race Entity**: The fundamental `Race` entity structure and relationships
- **Result Entity Foundation**: Existing `Result` entity primary fields (Id, RaceId, DriverId, Position, Points)
- **Bet Processing Logic**: The core bet settlement logic that depends on race results
- **Race Status Workflow**: Existing race status transitions and processing
- **Database Migrations**: Existing database migrations for current tables
- **Authentication System**: User authentication and authorization mechanisms

### Avoid Changes To
- **Existing API Endpoints**: Do not modify current race-related endpoints, only add new ones
- **Current Frontend Components**: Do not rewrite existing race listing or detail pages
- **Caching Infrastructure**: Use existing caching patterns, don't replace the caching system
- **Logging Framework**: Use existing logging services and patterns
- **Error Handling**: Use existing error handling middleware and patterns

### Integration Only
- **IRaceService**: Extend the interface with new methods, don't modify existing ones
- **RaceController**: Add new endpoints, don't change existing ones
- **Frontend Routing**: Add new routes, don't modify existing navigation structure
- **Existing Services**: Use existing services (BettingService, UserService) as-is

## Estimated Effort
- Database: 1 day
- Service Layer: 2 days
- API Endpoints: 1 day
- Frontend: 5 days
- Testing: 2 days
- **Total: 11 days**
