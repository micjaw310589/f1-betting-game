# Driver's Championship Ranking - Implementation Task

## Overview
Implement a complete Driver's Championship ranking system that stores and displays current and historical season standings with detailed race-by-race performance.

## Requirements
- Store and display current season Driver's Championship standings
- Show driver points, position, team, and race-by-race performance
- Update rankings automatically when race results are processed
- Support historical season data

## Database Changes

### New Tables
```csharp
public class DriverChampionship
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public Driver Driver { get; set; }
    public int Season { get; set; }
    public int Points { get; set; }
    public int Position { get; set; }
    public DateTime LastUpdated { get; set; }
    public ICollection<DriverChampionshipRace> RaceResults { get; set; }
}

public class DriverChampionshipRace
{
    public int Id { get; set; }
    public int DriverChampionshipId { get; set; }
    public DriverChampionship DriverChampionship { get; set; }
    public int RaceId { get; set; }
    public Race Race { get; set; }
    public int PointsEarned { get; set; }
    public int Position { get; set; }
}
```

### Database Migration
- Create migration for DriverChampionship and DriverChampionshipRace tables
- Add foreign key constraints and indexes
- Add unique constraint for (DriverId, Season) combination

## Service Layer

### Extend IRaceService
```csharp
Task UpdateDriverChampionshipAsync(int raceId);
Task<IEnumerable<DriverChampionshipDto>> GetCurrentDriverChampionshipAsync();
Task<DriverChampionshipDto> GetDriverChampionshipDetailsAsync(int driverId);
Task<IEnumerable<DriverChampionshipDto>> GetHistoricalChampionshipAsync(int season);
```

### Implement RaceService Methods
```csharp
public async Task UpdateDriverChampionshipAsync(int raceId)
{
    // 1. Get race results for the specified race
    // 2. Calculate points for each driver based on F1 points system
    // 3. Update or create DriverChampionship records
    // 4. Recalculate positions based on total points
    // 5. Store race-by-race results in DriverChampionshipRace
}

public async Task<IEnumerable<DriverChampionshipDto>> GetCurrentDriverChampionshipAsync()
{
    // Get current season from configuration
    // Query DriverChampionship for current season
    // Map to DTO and return sorted by position
}
```

## Background Worker Integration

### Modify RaceStatusMonitorJob
```csharp
// In ProcessFinishedRacesAsync method, after processing race results:
await _raceService.UpdateDriverChampionshipAsync(race.Id);
```

### Ensure Idempotency
- Check if championship data already exists for the race
- Only update if results have changed
- Log all updates for audit purposes

## API Endpoints

### Add to RacesController
```csharp
[HttpGet("championship/current")]
public async Task<ActionResult<IEnumerable<DriverChampionshipDto>>> GetCurrentChampionship()
{
    var championship = await _raceService.GetCurrentDriverChampionshipAsync();
    return Ok(championship);
}

[HttpGet("championship/driver/{driverId}")]
public async Task<ActionResult<DriverChampionshipDto>> GetDriverChampionship(int driverId)
{
    var details = await _raceService.GetDriverChampionshipDetailsAsync(driverId);
    return Ok(details);
}

[HttpGet("championship/season/{season}")]
public async Task<ActionResult<IEnumerable<DriverChampionshipDto>>> GetSeasonChampionship(int season)
{
    var championship = await _raceService.GetHistoricalChampionshipAsync(season);
    return Ok(championship);
}
```

## DTOs

### DriverChampionshipDto
```csharp
public class DriverChampionshipDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; }
    public string TeamName { get; set; }
    public int Season { get; set; }
    public int Points { get; set; }
    public int Position { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public DateTime LastUpdated { get; set; }
    public IEnumerable<DriverChampionshipRaceDto> RaceResults { get; set; }
}

public class DriverChampionshipRaceDto
{
    public int RaceId { get; set; }
    public string RaceName { get; set; }
    public int Position { get; set; }
    public int PointsEarned { get; set; }
    public DateTime RaceDate { get; set; }
}
```

## Frontend Integration

### Championship Standings Page
- Create `/championship` route
- Display sortable table with columns: Position, Driver, Team, Points, Wins, Podiums
- Add team color indicators
- Implement pagination for historical seasons

### Driver Detail View
- Create `/championship/driver/{driverId}` route
- Show driver profile with championship statistics
- Display race-by-race results in a responsive grid
- Add chart visualization of points progression

### Real-time Updates
- Implement SignalR hub for championship updates
- Add WebSocket connection on championship pages
- Update UI automatically when new results are processed

## Testing Requirements

### Unit Tests
- Test championship calculation logic
- Test position sorting algorithms
- Test DTO mapping

### Integration Tests
- Test full flow from race result processing to championship update
- Test API endpoints with various scenarios
- Test idempotency of championship updates

### UI Tests
- Test responsive design
- Test real-time update functionality
- Test historical season navigation

## Success Criteria
- Championship standings update automatically within 5 minutes of race result processing
- Historical data can be queried efficiently
- UI displays correctly on all device sizes
- Real-time updates work without page refresh

## Dependencies
- OpenF1 API synchronization must be implemented first
- Race results processing must be working correctly
- Driver and Team entities must be properly populated

## Out of Scope

### Do NOT Modify
- **Existing Bet Processing Logic**: The core bet placement and settlement logic in `BettingService` should remain unchanged
- **User Points System**: The fundamental points system and `User.AddPoints()`/`User.DeductPoints()` methods
- **Race Status Workflow**: The existing race status transitions and `RaceStatusMonitorJob` core logic
- **Authentication System**: User authentication and authorization mechanisms
- **Database Schema**: Existing tables (User, Race, Driver, Result, Bet, Team) - only add new tables
- **API Architecture**: Existing controller base classes and routing conventions

### Avoid Changes To
- **Existing DTOs**: Do not modify current DTO structures unless extending them
- **Current Frontend Routes**: Do not change existing page URLs or navigation structure
- **Notification System**: Use existing notification service, don't rebuild it
- **Caching Infrastructure**: Use existing caching mechanisms, don't replace them
- **Logging Framework**: Use existing logging patterns and services

### Integration Only
- **RaceStatusMonitorJob**: Only add calls to championship update, don't rewrite the job
- **IRaceService**: Extend the interface, don't modify existing methods
- **Frontend Services**: Use existing API service patterns for new endpoints

## Estimated Effort
- Database: 2 days
- Service Layer: 3 days
- API Endpoints: 1 day
- Frontend: 4 days
- Testing: 2 days
- **Total: 12 days**
