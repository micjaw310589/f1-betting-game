# User Bet Statistics - Implementation Task

## Overview
Create a comprehensive user bet statistics system that provides detailed analytics, performance metrics, and betting history for users.

## Requirements
- Display number of bets won/lost
- Show win ratio (percentage)
- Track all-time winnings
- Provide advanced metrics: streaks, ROI, favorite drivers, etc.
- Support filtering and time-based analysis

## Database Changes

### Extend UserStatisticsDto
```csharp
public class EnhancedUserStatisticsDto : UserStatisticsDto
{
    // Existing fields from UserStatisticsDto
    public int UserId { get; set; }
    public string Username { get; set; }
    public int TotalBets { get; set; }
    public int WinningBets { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWinnings { get; set; }
    public int Points { get; set; }
    public int Rank { get; set; }

    // New enhanced fields
    public int LosingBets { get; set; }
    public int PushBets { get; set; } // Refunded bets
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
    public decimal TotalAmountBet { get; set; }
    public int BetsThisWeek { get; set; }
    public int BetsThisMonth { get; set; }
}
```

### Add UserBetStatisticsCache Table
```csharp
public class UserBetStatisticsCache
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int TotalBets { get; set; }
    public int WinningBets { get; set; }
    public int LosingBets { get; set; }
    public int PushBets { get; set; }
    public decimal TotalWinnings { get; set; }
    public decimal TotalAmountBet { get; set; }
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int LongestWinStreak { get; set; }
    public DateTime LastUpdated { get; set; }
    public int FavoriteDriverId { get; set; }
    public decimal LargestWin { get; set; }
    public decimal LargestLoss { get; set; }
}
```

## Service Layer

### Extend IUserService
```csharp
Task<EnhancedUserStatisticsDto> GetEnhancedUserStatisticsAsync(int userId);
Task<IEnumerable<BetHistoryDto>> GetBetHistoryAsync(int userId, int limit = 50, int offset = 0);
Task<IEnumerable<UserBetAnalysisDto>> GetUserBetAnalysisAsync(int userId);
Task<EnhancedUserStatisticsDto> GetUserStatisticsByTimeRangeAsync(int userId, DateTime startDate, DateTime endDate);
Task UpdateUserStatisticsCacheAsync(int userId);
Task RecalculateAllUserStatisticsAsync();
```

### Implement UserService Methods
```csharp
public async Task<EnhancedUserStatisticsDto> GetEnhancedUserStatisticsAsync(int userId)
{
    // 1. Check cache first
    var cachedStats = await CheckStatisticsCache(userId);
    if (cachedStats != null) return cachedStats;

    // 2. Calculate from bet history
    var stats = await CalculateUserStatisticsFromBets(userId);

    // 3. Update cache
    await UpdateStatisticsCache(userId, stats);

    return stats;
}

private async Task<EnhancedUserStatisticsDto> CalculateUserStatisticsFromBets(int userId)
{
    // Query user's bet history
    var bets = await _betRepository.GetUserBetsWithResultsAsync(userId);

    // Calculate basic statistics
    var totalBets = bets.Count();
    var winningBets = bets.Count(b => b.Status == BetStatus.Won);
    var losingBets = bets.Count(b => b.Status == BetStatus.Lost);
    var pushBets = bets.Count(b => b.Status == BetStatus.Push);

    // Calculate financial metrics
    var totalWinnings = bets.Where(b => b.Status == BetStatus.Won)
                           .Sum(b => b.PayoutAmount - b.Amount);
    var totalAmountBet = bets.Sum(b => b.Amount);

    // Calculate streaks
    var (currentWinStreak, currentLoseStreak, longestWinStreak) = CalculateStreaks(bets);

    // Calculate ROI
    var roi = totalAmountBet > 0 ? (totalWinnings / totalAmountBet) * 100 : 0;

    // Find favorite driver
    var favoriteDriver = FindFavoriteDriver(bets);

    // Find largest win/loss
    var (largestWin, largestLoss) = FindLargestBets(bets);

    // Return complete DTO
    return new EnhancedUserStatisticsDto
    {
        // Populate all fields
    };
}
```

## Background Worker

### Create UserStatisticsUpdaterJob
```csharp
public class UserStatisticsUpdaterJob : BackgroundService
{
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting user statistics update...");

                // Update statistics for all active users
                var activeUsers = await _userService.GetActiveUsersAsync();
                foreach (var user in activeUsers)
                {
                    await _userService.UpdateUserStatisticsCacheAsync(user.Id);
                }

                _logger.LogInformation("User statistics update completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user statistics");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}
```

### Event-Based Updates
```csharp
// In BettingService.ProcessRaceResultsAsync
foreach (var bet in raceBets)
{
    await _bettingService.SettleBetAsync(bet.Id);
    // Trigger statistics update for this user
    await _userService.UpdateUserStatisticsCacheAsync(bet.UserId);
}
```

## API Endpoints

### Add to UsersController
```csharp
[HttpGet("{userId}/stats")]
public async Task<ActionResult<EnhancedUserStatisticsDto>> GetEnhancedStatistics(int userId)
{
    var stats = await _userService.GetEnhancedUserStatisticsAsync(userId);
    return Ok(stats);
}

[HttpGet("{userId}/bets/history")]
public async Task<ActionResult<PagedResult<BetHistoryDto>>> GetBetHistory(
    int userId,
    [FromQuery] int limit = 50,
    [FromQuery] int offset = 0,
    [FromQuery] BetStatus? status = null,
    [FromQuery] int? driverId = null)
{
    var history = await _userService.GetBetHistoryAsync(userId, limit, offset, status, driverId);
    return Ok(history);
}

[HttpGet("{userId}/bets/analysis")]
public async Task<ActionResult<UserBetAnalysisDto>> GetBetAnalysis(int userId)
{
    var analysis = await _userService.GetUserBetAnalysisAsync(userId);
    return Ok(analysis);
}

[HttpGet("{userId}/stats/range")]
public async Task<ActionResult<EnhancedUserStatisticsDto>> GetStatisticsByRange(
    int userId,
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate)
{
    var stats = await _userService.GetUserStatisticsByTimeRangeAsync(userId, startDate, endDate);
    return Ok(stats);
}
```

## DTOs

### BetHistoryDto
```csharp
public class BetHistoryDto
{
    public int BetId { get; set; }
    public int RaceId { get; set; }
    public string RaceName { get; set; }
    public DateTime RaceDate { get; set; }
    public string DriverName { get; set; }
    public string TeamName { get; set; }
    public BetType BetType { get; set; }
    public decimal Amount { get; set; }
    public decimal? PayoutAmount { get; set; }
    public BetStatus Status { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public decimal? Odds { get; set; }
    public int? DriverPosition { get; set; }
}
```

### UserBetAnalysisDto
```csharp
public class UserBetAnalysisDto
{
    public int UserId { get; set; }
    public Dictionary<BetType, BetTypeAnalysisDto> BetTypeAnalysis { get; set; }
    public Dictionary<int, DriverAnalysisDto> DriverAnalysis { get; set; }
    public Dictionary<int, TeamAnalysisDto> TeamAnalysis { get; set; }
    public MonthlyAnalysisDto[] MonthlyAnalysis { get; set; }
    public TimeOfDayAnalysisDto TimeOfDayAnalysis { get; set; }
}

public class BetTypeAnalysisDto
{
    public int TotalBets { get; set; }
    public int WinningBets { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalWinnings { get; set; }
    public decimal ROI { get; set; }
}

public class DriverAnalysisDto
{
    public string DriverName { get; set; }
    public int TotalBets { get; set; }
    public int WinningBets { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalWinnings { get; set; }
}

public class MonthlyAnalysisDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalBets { get; set; }
    public int WinningBets { get; set; }
    public decimal TotalWinnings { get; set; }
}
```

## Frontend Integration

### Statistics Dashboard
- Create `/profile/stats` route
- Display key metrics in dashboard cards:
  - Total Bets, Win Rate, ROI, Current Streak
  - Largest Win/Loss, Favorite Driver
  - Weekly/Monthly activity charts

### Bet History Page
- Create `/profile/bets` route
- Implement data table with sorting and filtering:
  - Columns: Date, Race, Driver, Bet Type, Amount, Status, Winnings
  - Filters: Status, Driver, Bet Type, Date Range
  - Export to CSV functionality

### Advanced Analytics
- Create `/profile/analytics` route
- Implement interactive charts:
  - Win/loss trends over time
  - Performance by bet type
  - Driver/team success rates
  - ROI by race circuit

### Visualizations
- Add sparklines for recent performance
- Implement win/loss heatmap by day of week
- Create betting pattern radar chart

## Testing Requirements

### Unit Tests
- Test statistics calculation logic
- Test streak calculation algorithms
- Test ROI and financial metric calculations
- Test DTO mapping

### Integration Tests
- Test full statistics retrieval flow
- Test bet history pagination
- Test time-range filtering
- Test cache behavior

### UI Tests
- Test dashboard responsiveness
- Test chart rendering
- Test filter functionality
- Test export features

## Success Criteria
- Statistics page loads within 1.5 seconds (cached) or 3 seconds (uncached)
- Bet history supports efficient pagination with 10,000+ bets
- Analytics charts render correctly on all devices
- Real-time updates work when bets are settled
- All calculations are mathematically accurate

## Dependencies
- Bet and Result entities must be properly populated
- User authentication must be working
- Race data must be available

## Out of Scope

### Do NOT Modify
- **Core Bet Entity**: The fundamental `Bet` entity structure and status workflow
- **User Entity Foundation**: Existing `User` entity primary fields and relationships
- **Bet Processing Logic**: The core bet placement, settlement, and payout calculation logic
- **Points System**: The fundamental user points system and transaction mechanisms
- **Authentication System**: User authentication, authorization, and identity management
- **Database Migrations**: Existing database migrations for current tables

### Avoid Changes To
- **Existing API Endpoints**: Do not modify current bet-related or user-related endpoints
- **Current Frontend Components**: Do not rewrite existing bet placement or user profile pages
- **Caching Infrastructure**: Use existing caching patterns, don't replace the caching system
- **Logging Framework**: Use existing logging services and patterns
- **Error Handling**: Use existing error handling middleware and patterns
- **Existing DTOs**: Do not modify current DTO structures unless extending them

### Integration Only
- **IUserService**: Extend the interface with new methods, don't modify existing ones
- **UsersController**: Add new endpoints, don't change existing ones
- **BettingService**: Use existing bet processing, only add statistics triggers
- **Frontend Routing**: Add new routes (`/profile/stats`, `/profile/bets`), don't modify existing navigation
- **Existing Services**: Use existing services (RaceService, NotificationService) as-is

## Estimated Effort
- Database: 2 days
- Service Layer: 4 days
- Background Worker: 2 days
- API Endpoints: 2 days
- Frontend: 6 days
- Testing: 3 days
- **Total: 19 days**
