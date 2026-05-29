# F1 Betting Game - Phase 4 Implementation Plan

## Overview

This document outlines the technical implementation plan for Phase 4 features, including Driver's Championship ranking, race results display, user bet statistics, achievements system, daily/monthly tasks, and OpenF1 API synchronization improvements.

## Current System Analysis

### Existing Services and Components

1. **Background Workers**: RaceStatusMonitorJob already exists for processing finished races
2. **OpenF1 Integration**: IOpenF1ApiClient interface with basic race and driver data fetching
3. **Data Model**: Entities for User, Race, Driver, Result, Bet, Team, LeaderboardHistory
4. **Services**: BettingService, RaceService, UserService, LeaderboardService, NotificationService
5. **Controllers**: API endpoints for races, bets, users, leaderboard, etc.

### Key Integrity Points

1. **Data Consistency**: Race results must be accurately synchronized from OpenF1
2. **Bet Processing**: Bets must be settled correctly based on official race results
3. **Points System**: User points must be calculated and updated accurately
4. **Idempotency**: Background jobs must handle duplicate processing gracefully
5. **Performance**: Championship rankings and statistics must be computed efficiently

## Feature Implementation Details

### 1. Driver's Championship Ranking

#### Requirements
- Store and display current season Driver's Championship standings
- Show driver points, position, team, and race-by-race performance
- Update rankings automatically when race results are processed

#### Implementation Plan

**Database Changes:**
- Add `DriverChampionship` table:
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

**Service Layer:**
- Extend `IRaceService` with championship methods:
  ```csharp
  Task UpdateDriverChampionshipAsync(int raceId);
  Task<IEnumerable<DriverChampionshipDto>> GetCurrentDriverChampionshipAsync();
  Task<DriverChampionshipDto> GetDriverChampionshipDetailsAsync(int driverId);
  ```

**Background Worker Integration:**
- Modify `RaceStatusMonitorJob` to call `UpdateDriverChampionshipAsync` after processing race results
- Ensure idempotent updates (check if championship data already exists for race)

**API Endpoints:**
- `GET /api/championship/current` - Get current season standings
- `GET /api/championship/driver/{driverId}` - Get driver's championship details
- `GET /api/championship/season/{season}` - Get historical season standings

**Frontend Integration:**
- Create championship standings page with sortable table
- Add driver detail view with race-by-race breakdown
- Implement real-time updates via SignalR or polling

### 2. Race Results Display for Finished Races

#### Requirements
- Display detailed results for completed races
- Show driver positions, points earned, fastest laps, etc.
- Include historical race results

#### Implementation Plan

**Extend Existing Result Entity:**
- Current `Result` entity already has necessary fields (Position, Points, FastestLap, etc.)
- Add missing fields if needed:
  ```csharp
  public TimeSpan? TimeBehindLeader { get; set; }
  public int LapsCompleted { get; set; }
  public string Status { get; set; } // "Finished", "DNF", "DSQ", etc.
  ```

**Service Layer:**
- Extend `IRaceService`:
  ```csharp
  Task<IEnumerable<RaceResultDto>> GetRaceResultsAsync(int raceId);
  Task<RaceResultDetailsDto> GetRaceResultDetailsAsync(int raceId);
  Task<IEnumerable<RaceSummaryDto>> GetHistoricalRaceResultsAsync(int season, int limit = 10);
  ```

**API Endpoints:**
- `GET /api/races/{raceId}/results` - Get race results
- `GET /api/races/{raceId}/results/detailed` - Get detailed results with lap times
- `GET /api/races/season/{season}/results` - Get all results for a season

**Frontend Integration:**
- Create race results page with expandable details
- Add visual indicators for podium finishes
- Implement result comparison between drivers

**Caching Strategy:**
- Cache race results for 24 hours after race completion
- Invalidate cache when new results are processed

### 3. User Bet Statistics Page

#### Requirements
- Number of bets won/lost
- Win ratio (percentage)
- All-time winnings
- Additional: Streaks, favorite drivers, ROI, etc.

#### Implementation Plan

**Extend UserStatisticsDto:**
```csharp
public class EnhancedUserStatisticsDto : UserStatisticsDto
{
    // Existing fields: TotalBets, WinningBets, WinRate, TotalWinnings, Points, Rank

    // New fields
    public int LosingBets { get; set; }
    public int PushBets { get; set; } // Bets that were refunded
    public decimal ReturnOnInvestment { get; set; } // ROI percentage
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int LongestWinStreak { get; set; }
    public int FavoriteDriverId { get; set; }
    public string FavoriteDriverName { get; set; }
    public decimal AverageBetAmount { get; set; }
    public decimal LargestWin { get; set; }
    public decimal LargestLoss { get; set; }
    public DateTime? LastBetDate { get; set; }
}
```

**Service Layer:**
- Extend `IUserService`:
  ```csharp
  Task<EnhancedUserStatisticsDto> GetEnhancedUserStatisticsAsync(int userId);
  Task<IEnumerable<BetHistoryDto>> GetBetHistoryAsync(int userId, int limit = 50, int offset = 0);
  Task<IEnumerable<UserBetAnalysisDto>> GetUserBetAnalysisAsync(int userId);
  ```

**Database Optimization:**
- Add materialized view or cached statistics table:
  ```csharp
  public class UserBetStatisticsCache
  {
      public int UserId { get; set; }
      public User User { get; set; }
      public int TotalBets { get; set; }
      public int WinningBets { get; set; }
      public int LosingBets { get; set; }
      public decimal TotalWinnings { get; set; }
      public decimal TotalAmountBet { get; set; }
      public DateTime LastUpdated { get; set; }
  }
  ```

**Background Worker:**
- Create `UserStatisticsUpdaterJob` to recalculate statistics nightly
- Trigger immediate update when bets are settled

**API Endpoints:**
- `GET /api/users/{userId}/stats` - Get enhanced statistics
- `GET /api/users/{userId}/bets/history` - Get bet history with pagination
- `GET /api/users/{userId}/bets/analysis` - Get betting patterns and insights

**Frontend Integration:**
- Create comprehensive stats dashboard with charts
- Add filters (time range, bet type, driver, etc.)
- Implement visualizations for win/loss trends

### 4. Achievements System with Point Rewards

#### Requirements
- Define various achievements
- Award points when achievements are unlocked
- Display user achievements and progress

#### Implementation Plan

**Database Changes:**
```csharp
public class AchievementDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public AchievementType Type { get; set; } // OneTime, Recurring, Tiered
    public string Criteria { get; set; } // JSON configuration
    public bool IsActive { get; set; }
}

public class UserAchievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int AchievementDefinitionId { get; set; }
    public AchievementDefinition AchievementDefinition { get; set; }
    public DateTime UnlockedAt { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime? ClaimedAt { get; set; }
}

public enum AchievementType
{
    OneTime,
    Recurring,
    Tiered
}
```

**Service Layer:**
- Create `IAchievementService`:
  ```csharp
  Task CheckAndAwardAchievementsAsync(int userId, string eventType, object eventData = null);
  Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(int userId);
  Task<IEnumerable<AchievementDefinitionDto>> GetAvailableAchievementsAsync(int userId);
  Task ClaimAchievementRewardAsync(int userId, int achievementId);
  Task<IEnumerable<AchievementDefinitionDto>> GetAllAchievementDefinitionsAsync();
  ```

**Achievement Criteria Examples:**
```json
// First Bet
{
  "type": "OneTime",
  "trigger": "BetPlaced",
  "condition": "TotalBets == 1"
}

// Win Streak
{
  "type": "Tiered",
  "trigger": "BetWon",
  "tiers": [
    {"target": 3, "reward": 100},
    {"target": 5, "reward": 250},
    {"target": 10, "reward": 500}
  ]
}

// Big Winner
{
  "type": "OneTime",
  "trigger": "BetSettled",
  "condition": "SingleWinAmount > 1000"
}
```

**Integration Points:**
- Call `CheckAndAwardAchievementsAsync` in:
  - `BettingService.PlaceBetAsync` (BetPlaced event)
  - `BettingService.ProcessRaceResultsAsync` (BetWon/BetLost events)
  - `UserService` methods for profile updates
  - `RaceService` for race-related achievements

**API Endpoints:**
- `GET /api/achievements` - Get all achievement definitions
- `GET /api/users/{userId}/achievements` - Get user's achievements
- `POST /api/users/{userId}/achievements/{achievementId}/claim` - Claim reward
- `GET /api/achievements/progress` - Get achievement progress summary

**Frontend Integration:**
- Create achievements dashboard with progress bars
- Add notifications for newly unlocked achievements
- Implement achievement detail modal with requirements

### 5. Daily and Monthly Tasks with Point Rewards

#### Requirements
- Simple tasks users can complete for points
- Daily tasks reset every 24 hours
- Monthly tasks reset at month start
- Prevent exploitation and ensure fairness

#### Implementation Plan

**Database Changes:**
```csharp
public class TaskDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public TaskType Type { get; set; } // Daily, Weekly, Monthly
    public TaskCategory Category { get; set; } // Betting, Social, Engagement
    public string Criteria { get; set; } // JSON configuration
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}

public class UserTask
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int TaskDefinitionId { get; set; }
    public TaskDefinition TaskDefinition { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
}

public class UserTaskHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int TaskDefinitionId { get; set; }
    public TaskDefinition TaskDefinition { get; set; }
    public DateTime CompletedAt { get; set; }
    public int PointsEarned { get; set; }
}

public enum TaskType
{
    Daily,
    Weekly,
    Monthly
}

public enum TaskCategory
{
    Betting,
    Social,
    Engagement,
    Learning,
    Community
}
```

**Service Layer:**
- Create `ITaskService`:
  ```csharp
  Task<IEnumerable<UserTaskDto>> GetUserTasksAsync(int userId);
  Task<UserTaskDto> CompleteTaskAsync(int userId, int taskId);
  Task ClaimTaskRewardAsync(int userId, int taskId);
  Task ResetDailyTasksAsync();
  Task ResetMonthlyTasksAsync();
  Task CheckAndAssignTasksAsync(int userId);
  Task<IEnumerable<TaskDefinitionDto>> GetTaskDefinitionsAsync(TaskType? type = null);
  ```

**Task Examples:**
```json
// Daily Tasks
[
  {
    "name": "Place Your First Bet",
    "description": "Place at least one bet today",
    "type": "Daily",
    "category": "Betting",
    "reward": 50,
    "criteria": {"minBets": 1}
  },
  {
    "name": "Social Butterfly",
    "description": "Like or comment on 3 race discussions",
    "type": "Daily",
    "category": "Social",
    "reward": 30,
    "criteria": {"minInteractions": 3}
  }
]

// Monthly Tasks
[
  {
    "name": "Consistent Better",
    "description": "Place bets on 5 different races this month",
    "type": "Monthly",
    "category": "Betting",
    "reward": 200,
    "criteria": {"minRaces": 5}
  },
  {
    "name": "Knowledge Seeker",
    "description": "Read 10 race previews or analyses",
    "type": "Monthly",
    "category": "Learning",
    "reward": 150,
    "criteria": {"minArticles": 10}
  }
]
```

**Background Workers:**
- Create `TaskResetJob` for daily/monthly resets
- Create `TaskAssignmentJob` to assign new tasks to users

**Integration Points:**
- Call `CheckAndAssignTasksAsync` on user login
- Trigger task completion checks in relevant services:
  - `BettingService` for betting-related tasks
  - `NotificationService` for engagement tasks
  - Frontend for social interaction tasks

**API Endpoints:**
- `GET /api/tasks` - Get available task definitions
- `GET /api/users/{userId}/tasks` - Get user's current tasks
- `POST /api/users/{userId}/tasks/{taskId}/complete` - Mark task as complete
- `POST /api/users/{userId}/tasks/{taskId}/claim` - Claim task reward
- `GET /api/users/{userId}/tasks/history` - Get task completion history

**Frontend Integration:**
- Create tasks dashboard with daily/monthly tabs
- Add progress indicators and countdowns to reset
- Implement task completion notifications

### 6. OpenF1 API Synchronization with Background Worker

#### Requirements
- Robust synchronization of race data from OpenF1
- Handle API failures and rate limits gracefully
- Ensure data consistency between OpenF1 and local database
- Support historical data synchronization

#### Implementation Plan

**Enhanced OpenF1 Client:**
- Extend `IOpenF1ApiClient`:
  ```csharp
  Task<IEnumerable<OpenF1RaceResult>> GetRaceResultsAsync(string raceId);
  Task<IEnumerable<OpenF1DriverStanding>> GetDriverStandingsAsync(int season);
  Task<IEnumerable<OpenF1ConstructorStanding>> GetConstructorStandingsAsync(int season);
  Task<OpenF1RaceDetails> GetRaceDetailsAsync(string raceId);
  Task<IEnumerable<OpenF1Session>> GetRaceSessionsAsync(string raceId);
  Task SyncHistoricalDataAsync(int season);
  ```

**New Data Models:**
```csharp
public class OpenF1RaceResult
{
    public int RaceId { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; }
    public string TeamName { get; set; }
    public int Position { get; set; }
    public int Points { get; set; }
    public TimeSpan? FastestLapTime { get; set; }
    public int LapsCompleted { get; set; }
    public string Status { get; set; }
    public TimeSpan? TimeBehindLeader { get; set; }
}

public class OpenF1DriverStanding
{
    public int Position { get; set; }
    public int DriverId { get; set; }
    public string DriverName { get; set; }
    public string TeamName { get; set; }
    public int Points { get; set; }
    public int Wins { get; set; }
}

public class OpenF1RaceDetails
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public string Circuit { get; set; }
    public string Country { get; set; }
    public int Season { get; set; }
    public string CircuitId { get; set; }
    public DateTime? QualifyingDate { get; set; }
    public DateTime? SprintDate { get; set; }
    public string CircuitLayoutUrl { get; set; }
}
```

**Synchronization Service:**
- Create `IOpenF1SynchronizationService`:
  ```csharp
  Task SyncUpcomingRacesAsync();
  Task SyncRaceResultsAsync(string raceId);
  Task SyncDriverStandingsAsync(int season);
  Task SyncHistoricalSeasonAsync(int season);
  Task SyncAllMissingDataAsync();
  Task<SyncResultDto> GetSyncStatusAsync();
  ```

**Enhanced Background Worker:**
- Create `OpenF1SynchronizationJob`:
  ```csharp
  public class OpenF1SynchronizationJob : BackgroundService
  {
      private readonly TimeSpan _syncInterval;
      private readonly TimeSpan _raceCheckInterval;

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
          while (!stoppingToken.IsCancellationRequested)
          {
              try
              {
                  await SyncUpcomingRacesAsync(stoppingToken);
                  await SyncRecentRaceResultsAsync(stoppingToken);
                  await CheckForLiveRacesAsync(stoppingToken);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "OpenF1 synchronization failed");
              }

              await Task.Delay(_syncInterval, stoppingToken);
          }
      }
  }
  ```

**Data Consistency Features:**
- **Conflict Resolution**: Timestamp-based or version-based conflict resolution
- **Idempotent Operations**: Ensure same data can be processed multiple times safely
- **Error Handling**: Retry logic with exponential backoff for API failures
- **Rate Limiting**: Respect OpenF1 API rate limits with circuit breakers
- **Data Validation**: Validate OpenF1 data before persisting

**Database Changes:**
- Add synchronization tracking:
  ```csharp
  public class OpenF1SyncLog
  {
      public int Id { get; set; }
      public string EntityType { get; set; } // Race, Result, Standing
      public string EntityId { get; set; }
      public DateTime SyncDate { get; set; }
      public bool Success { get; set; }
      public string ErrorMessage { get; set; }
      public string SourceData { get; set; } // JSON snapshot
  }

  public class RaceSyncStatus
  {
      public int RaceId { get; set; }
      public Race Race { get; set; }
      public DateTime? ResultsSyncedAt { get; set; }
      public DateTime? StandingsSyncedAt { get; set; }
      public bool HasDiscrepancies { get; set; }
      public string DiscrepancyNotes { get; set; }
  }
  ```

**API Endpoints:**
- `GET /api/admin/openf1/sync/status` - Get synchronization status
- `POST /api/admin/openf1/sync/races` - Trigger race synchronization
- `POST /api/admin/openf1/sync/results/{raceId}` - Sync specific race results
- `POST /api/admin/openf1/sync/season/{season}` - Sync entire season
- `GET /api/admin/openf1/sync/logs` - Get synchronization logs

**Integration with Existing Systems:**
- Modify `RaceStatusMonitorJob` to use enhanced synchronization
- Update `RaceService` to validate data against OpenF1 before manual overrides
- Add admin notifications for synchronization failures

## Potential Conflicts and Resolution Strategies

### 1. Data Consistency Conflicts

**Scenario**: OpenF1 data changes after we've processed race results and settled bets

**Resolution**:
- Implement data versioning and timestamp tracking
- Store OpenF1 data snapshots when processing results
- Add admin override capability with audit logging
- Implement discrepancy detection and notification

### 2. Race Status Conflicts

**Scenario**: OpenF1 shows race as finished but our system hasn't processed it yet

**Resolution**:
- Add race status reconciliation in synchronization job
- Implement status change detection and automatic processing
- Add manual override capability for edge cases

### 3. Driver/Team Mapping Conflicts

**Scenario**: OpenF1 driver IDs don't match our local driver entities

**Resolution**:
- Implement robust driver mapping system with multiple identifiers
- Add admin interface for manual driver mapping
- Implement fuzzy matching for driver names
- Store OpenF1 IDs alongside local IDs

### 4. Points Calculation Conflicts

**Scenario**: OpenF1 points don't match expected F1 points system

**Resolution**:
- Implement our own points calculation based on positions
- Use OpenF1 points as validation, not source of truth
- Add points calculation audit logs
- Implement admin override for exceptional cases

### 5. Concurrent Processing Conflicts

**Scenario**: Multiple background workers trying to process same race

**Resolution**:
- Implement distributed locking for race processing
- Add processing status flags to races
- Ensure idempotent processing operations
- Implement transaction isolation levels appropriately

## Implementation Timeline and Priorities

### Phase 1: Foundation (2-3 weeks)
1. **OpenF1 API Enhancements** - Extend client and add synchronization service
2. **Database Schema Updates** - Add tables for championships, achievements, tasks
3. **Background Worker Improvements** - Enhance existing worker and add new ones
4. **Basic API Endpoints** - CRUD operations for new entities

### Phase 2: Core Features (3-4 weeks)
1. **Driver's Championship** - Full implementation with frontend
2. **Race Results Display** - Enhanced results pages
3. **User Bet Statistics** - Extended statistics with visualizations
4. **Basic Achievement System** - Core functionality without all achievements

### Phase 3: Engagement Features (2-3 weeks)
1. **Daily/Monthly Tasks** - Full task system with rewards
2. **Advanced Achievements** - Complete achievement definitions
3. **Gamification UI** - Badges, progress bars, notifications

### Phase 4: Polish and Optimization (2 weeks)
1. **Performance Optimization** - Caching, query optimization
2. **Error Handling** - Robust error recovery
3. **Admin Tools** - Monitoring and override interfaces
4. **Comprehensive Testing** - Edge cases and failure scenarios

## Technical Considerations

### Performance Optimization

1. **Caching Strategy**:
   - Cache championship standings (5-minute cache)
   - Cache race results (24-hour cache for old races, 5-minute for recent)
   - Cache user statistics (1-hour cache, invalidate on bet settlement)

2. **Database Optimization**:
   - Add indexes for frequently queried fields
   - Consider materialized views for complex statistics
   - Implement read replicas for reporting queries

3. **Background Processing**:
   - Implement priority queues for synchronization tasks
   - Add rate limiting for OpenF1 API calls
   - Implement exponential backoff for failed operations

### Security Considerations

1. **Achievement/Task System**:
   - Prevent client-side manipulation of progress
   - Validate all achievement criteria server-side
   - Implement rate limiting for reward claims

2. **Data Integrity**:
   - Use transactions for points updates
   - Implement audit logging for sensitive operations
   - Add admin approval for large point adjustments

3. **API Security**:
   - Rate limit public endpoints
   - Add authentication for sensitive operations
   - Implement proper authorization checks

### Monitoring and Observability

1. **Logging**:
   - Structured logging for all background operations
   - Error logging with context for debugging
   - Performance metrics for slow operations

2. **Metrics**:
   - Track synchronization success/failure rates
   - Monitor background worker health and performance
   - Track user engagement with new features

3. **Alerting**:
   - Alert on synchronization failures
   - Alert on data consistency issues
   - Monitor for unusual activity patterns

## Migration Strategy

### Data Migration

1. **Historical Race Results**:
   - Implement batch processing for historical data
   - Start with current season, then work backwards
   - Validate data quality before making it available

2. **User Statistics**:
   - Backfill statistics for existing users
   - Implement background job to calculate initial values
   - Provide progress indicators for long-running migrations

3. **Achievements**:
   - Award retroactive achievements where applicable
   - Implement grandfathering for early adopters
   - Clear communication about new features

### Feature Rollout

1. **Phased Release**:
   - Release backend services first
   - Add frontend components incrementally
   - Monitor performance at each stage

2. **Feature Flags**:
   - Use feature flags for gradual rollout
   - Enable A/B testing for engagement features
   - Allow quick rollback if issues arise

3. **User Communication**:
   - Announce new features via in-app notifications
   - Provide tutorials and guides
   - Gather feedback through surveys

## Success Metrics

1. **User Engagement**:
   - Increase in daily active users
   - Higher bet placement frequency
   - Longer session durations

2. **Feature Adoption**:
   - Percentage of users completing daily tasks
   - Achievement unlock rates
   - Usage of statistics and championship pages

3. **Data Quality**:
   - Reduction in manual overrides needed
   - Accuracy of synchronized data
   - Timeliness of race result processing

4. **System Performance**:
   - Background worker success rates
   - API response times
   - Database query performance

## Risks and Mitigation

### Technical Risks

1. **OpenF1 API Reliability**:
   - *Risk*: API downtime or changes
   - *Mitigation*: Implement robust error handling, caching, manual override capability

2. **Performance Issues**:
   - *Risk*: Slow queries with new features
   - *Mitigation*: Load testing, query optimization, caching strategy

3. **Data Consistency**:
   - *Risk*: Conflicts between OpenF1 and local data
   - *Mitigation*: Validation layers, discrepancy detection, admin tools

### Business Risks

1. **User Adoption**:
   - *Risk*: Users don't engage with new features
   - *Mitigation*: Gamification, onboarding, clear value proposition

2. **Complexity Overload**:
   - *Risk*: Too many features overwhelm users
   - *Mitigation*: Gradual rollout, good UX design, user education

3. **Points Inflation**:
   - *Risk*: Too many reward points devalue currency
   - *Mitigation*: Careful points economy design, monitoring, adjustments

## Conclusion

This implementation plan provides a comprehensive approach to adding the requested features while maintaining system integrity and performance. The phased approach allows for incremental delivery and validation of each component, with appropriate attention to data consistency, user experience, and system reliability.