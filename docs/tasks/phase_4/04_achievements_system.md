# Achievements System - Implementation Task

## Overview
Implement a comprehensive achievements system with point rewards to gamify user engagement and reward betting activity.

## Requirements
- Define various achievement types (one-time, recurring, tiered)
- Award points when achievements are unlocked
- Track user progress toward achievements
- Display achievements with visual indicators
- Prevent exploitation and ensure fair play

## Database Changes

### New Tables
```csharp
public class AchievementDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; } // Font Awesome or custom icon class
    public int PointsReward { get; set; }
    public AchievementType Type { get; set; } // OneTime, Recurring, Tiered
    public string Criteria { get; set; } // JSON configuration
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string Category { get; set; } // "Betting", "Social", "Engagement", etc.
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
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
    public bool IsNotified { get; set; }
}

public class AchievementAuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int AchievementDefinitionId { get; set; }
    public AchievementDefinition AchievementDefinition { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; } // JSON
    public int ProgressChange { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum AchievementType
{
    OneTime,
    Recurring,
    Tiered
}
```

### Database Seeding
```csharp
// Seed initial achievements in SeedData.cs
public static void SeedAchievements(AppDbContext context)
{
    var achievements = new List<AchievementDefinition>
    {
        new AchievementDefinition
        {
            Name = "First Bet Placed",
            Description = "Place your first bet",
            Icon = "trophy",
            PointsReward = 100,
            Type = AchievementType.OneTime,
            Criteria = JsonSerializer.Serialize(new {
                trigger = "BetPlaced",
                condition = "TotalBets >= 1"
            }),
            IsActive = true,
            DisplayOrder = 1,
            Category = "Betting"
        },
        // Add more achievements...
    };

    context.AchievementDefinitions.AddRange(achievements);
    context.SaveChanges();
}
```

## Service Layer

### Create IAchievementService
```csharp
public interface IAchievementService
{
    Task CheckAndAwardAchievementsAsync(int userId, string eventType, object eventData = null);
    Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(int userId);
    Task<IEnumerable<AchievementDefinitionDto>> GetAvailableAchievementsAsync(int userId);
    Task<AchievementDefinitionDto> GetAchievementDefinitionAsync(int achievementId);
    Task ClaimAchievementRewardAsync(int userId, int achievementId);
    Task<IEnumerable<AchievementDefinitionDto>> GetAllAchievementDefinitionsAsync();
    Task<AchievementProgressDto> GetAchievementProgressAsync(int userId, int achievementId);
    Task MarkAchievementAsNotifiedAsync(int userId, int achievementId);
    Task RecalculateUserAchievementsAsync(int userId);
}
```

### Implement AchievementService
```csharp
public class AchievementService : IAchievementService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(AppDbContext context, IUserService userService, ILogger<AchievementService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task CheckAndAwardAchievementsAsync(int userId, string eventType, object eventData = null)
    {
        // Get all active achievements that trigger on this event type
        var achievements = await _context.AchievementDefinitions
            .Where(a => a.IsActive && a.Criteria.Contains($"\"trigger\":\"{eventType}\""))
            .ToListAsync();

        foreach (var achievement in achievements)
        {
            try
            {
                await ProcessAchievementAsync(userId, achievement, eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing achievement {AchievementId} for user {UserId}", achievement.Id, userId);
            }
        }
    }

    private async Task ProcessAchievementAsync(int userId, AchievementDefinition achievement, object eventData)
    {
        // Parse criteria
        var criteria = JsonSerializer.Deserialize<AchievementCriteria>(achievement.Criteria);
        var userAchievement = await GetOrCreateUserAchievement(userId, achievement.Id);

        // Check if already completed and not recurring
        if (userAchievement.CurrentProgress >= userAchievement.TargetProgress &&
            achievement.Type != AchievementType.Recurring)
        {
            return;
        }

        // Evaluate condition
        var conditionMet = await EvaluateCondition(userId, criteria, eventData);
        if (!conditionMet) return;

        // Update progress
        userAchievement.CurrentProgress = Math.Min(userAchievement.CurrentProgress + 1, userAchievement.TargetProgress);

        // Check if newly completed
        if (userAchievement.CurrentProgress >= userAchievement.TargetProgress &&
            userAchievement.UnlockedAt == default)
        {
            userAchievement.UnlockedAt = DateTime.UtcNow;
            userAchievement.IsNotified = false;

            // Award points if not recurring or first time this period
            if (achievement.Type != AchievementType.Recurring ||
                !await HasBeenClaimedThisPeriod(userId, achievement.Id))
            {
                await _userService.AddPointsAsync(userId, achievement.PointsReward);
                _logger.LogInformation("Achievement {AchievementId} unlocked for user {UserId}. Awarded {Points} points",
                    achievement.Id, userId, achievement.PointsReward);
            }
        }

        // Save changes
        await _context.SaveChangesAsync();

        // Log audit
        await LogAchievementAudit(userId, achievement.Id, eventType, eventData);
    }

    private async Task<bool> EvaluateCondition(int userId, AchievementCriteria criteria, object eventData)
    {
        // Implement condition evaluation logic
        // This will vary based on achievement type
        switch (criteria.Trigger)
        {
            case "BetPlaced":
                return await EvaluateBetPlacedCondition(userId, criteria, eventData);
            case "BetWon":
                return await EvaluateBetWonCondition(userId, criteria, eventData);
            // Add more cases...
            default:
                return false;
        }
    }
}
```

## Achievement Criteria Examples

### JSON Configuration Examples
```json
// First Bet - OneTime
{
  "trigger": "BetPlaced",
  "type": "OneTime",
  "condition": "TotalBets >= 1",
  "targetProgress": 1
}

// Win Streak - Tiered
{
  "trigger": "BetWon",
  "type": "Tiered",
  "condition": "CurrentWinStreak >= {tier}",
  "tiers": [
    {"tier": 3, "reward": 100, "targetProgress": 3},
    {"tier": 5, "reward": 250, "targetProgress": 5},
    {"tier": 10, "reward": 500, "targetProgress": 10}
  ]
}

// Big Winner - OneTime
{
  "trigger": "BetSettled",
  "type": "OneTime",
  "condition": "SingleWinAmount > 1000",
  "targetProgress": 1
}

// Consistent Better - Recurring (Monthly)
{
  "trigger": "BetPlaced",
  "type": "Recurring",
  "resetPeriod": "Monthly",
  "condition": "BetsThisMonth >= 10",
  "targetProgress": 10
}
```

### Achievement Categories
1. **Betting Achievements** - Based on betting activity and results
2. **Social Achievements** - Based on community interaction
3. **Engagement Achievements** - Based on app usage patterns
4. **Knowledge Achievements** - Based on learning and education
5. **Seasonal Achievements** - Time-limited special achievements

## Integration Points

### BettingService Integration
```csharp
// In BettingService.PlaceBetAsync
public async Task<BetResponseDto> PlaceBetAsync(PlaceBetDto placeBetDto)
{
    // ... existing bet placement logic ...

    // Trigger achievement check
    await _achievementService.CheckAndAwardAchievementsAsync(userId, "BetPlaced", new {
        betAmount = placeBetDto.Amount,
        betType = placeBetDto.BetType
    });

    return response;
}

// In BettingService.ProcessRaceResultsAsync
foreach (var bet in raceBets)
{
    var result = await SettleBetAsync(bet.Id);

    // Trigger achievement check based on result
    await _achievementService.CheckAndAwardAchievementsAsync(
        bet.UserId,
        bet.Status == BetStatus.Won ? "BetWon" : "BetLost",
        new {
            betId = bet.Id,
            raceId = bet.RaceId,
            winnings = result.PayoutAmount - bet.Amount
        }
    );
}
```

### UserService Integration
```csharp
// In UserService methods that affect user data
public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
{
    // ... update logic ...

    // Trigger profile-related achievements
    await _achievementService.CheckAndAwardAchievementsAsync(userId, "ProfileUpdated");
}
```

## API Endpoints

### Add AchievementsController
```csharp
[ApiController]
[Route("api/achievements")]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AchievementDefinitionDto>>> GetAllAchievements()
    {
        var achievements = await _achievementService.GetAllAchievementDefinitionsAsync();
        return Ok(achievements);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserAchievementDto>>> GetUserAchievements(int userId)
    {
        var achievements = await _achievementService.GetUserAchievementsAsync(userId);
        return Ok(achievements);
    }

    [HttpGet("user/{userId}/available")]
    public async Task<ActionResult<IEnumerable<AchievementDefinitionDto>>> GetAvailableAchievements(int userId)
    {
        var achievements = await _achievementService.GetAvailableAchievementsAsync(userId);
        return Ok(achievements);
    }

    [HttpGet("user/{userId}/achievement/{achievementId}")]
    public async Task<ActionResult<AchievementProgressDto>> GetAchievementProgress(int userId, int achievementId)
    {
        var progress = await _achievementService.GetAchievementProgressAsync(userId, achievementId);
        return Ok(progress);
    }

    [HttpPost("user/{userId}/achievement/{achievementId}/claim")]
    public async Task<ActionResult> ClaimAchievement(int userId, int achievementId)
    {
        await _achievementService.ClaimAchievementRewardAsync(userId, achievementId);
        return Ok(new { success = true });
    }

    [HttpPost("user/{userId}/achievement/{achievementId}/notify")]
    public async Task<ActionResult> MarkAsNotified(int userId, int achievementId)
    {
        await _achievementService.MarkAchievementAsNotifiedAsync(userId, achievementId);
        return Ok(new { success = true });
    }
}
```

## DTOs

### AchievementDefinitionDto
```csharp
public class AchievementDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public AchievementType Type { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
    public int UserProgress { get; set; }
    public int TargetProgress { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime? UnlockedAt { get; set; }
}
```

### UserAchievementDto
```csharp
public class UserAchievementDto
{
    public int AchievementDefinitionId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public AchievementType Type { get; set; }
    public string Category { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsClaimed { get; set; }
    public bool IsNotified { get; set; }
    public DateTime UnlockedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
}
```

### AchievementProgressDto
```csharp
public class AchievementProgressDto
{
    public int AchievementId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
    public double CompletionPercentage { get; set; }
    public string NextSteps { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsClaimed { get; set; }
}
```

## Frontend Integration

### Achievements Dashboard
- Create `/achievements` route
- Display achievements in categorized tabs
- Show progress bars for in-progress achievements
- Add visual indicators for unlocked/unclaimed achievements
- Implement filtering by category and status

### Achievement Detail Modal
- Show detailed achievement information
- Display progress and requirements
- Show reward information
- Add claim button for unlocked achievements

### Notifications System
- Add achievement unlock notifications
- Implement notification center for new achievements
- Add toast notifications when achievements unlock

### Profile Integration
- Add achievements summary to user profile
- Show recent achievements on dashboard
- Display achievement points in points breakdown

## Security Considerations

### Anti-Cheating Measures
```csharp
// In AchievementService
private async Task<bool> ValidateAchievementEarned(int userId, int achievementId)
{
    // Check if user actually meets criteria
    var achievement = await _context.AchievementDefinitions.FindAsync(achievementId);
    var criteria = JsonSerializer.Deserialize<AchievementCriteria>(achievement.Criteria);

    // Re-validate the condition
    return await EvaluateCondition(userId, criteria, null);
}

// Add rate limiting to claim endpoint
[HttpPost("claim")]
[RateLimit(10, "1:hour")] // Max 10 claims per hour
public async Task<ActionResult> ClaimAchievement(int achievementId)
{
    // ... validation and claiming logic
}
```

### Data Validation
- Validate all achievement criteria server-side
- Prevent client-side manipulation of progress
- Implement proper authorization checks
- Log all achievement-related actions for audit

## Testing Requirements

### Unit Tests
- Test achievement condition evaluation logic
- Test progress tracking for different achievement types
- Test tiered achievement progression
- Test recurring achievement reset logic

### Integration Tests
- Test full achievement unlock flow
- Test points awarding and claiming
- Test notification system integration
- Test cache invalidation

### UI Tests
- Test achievements dashboard responsiveness
- Test progress visualization
- Test notification display
- Test claim functionality

## Success Criteria
- Achievements unlock correctly based on defined criteria
- Points are awarded accurately and only once per achievement
- UI shows correct progress and status for all achievements
- System prevents cheating and exploitation
- Performance remains good with 100+ achievements per user

## Achievement Examples

### Starter Achievements
1. **First Bet** - Place your first bet (100 points)
2. **Welcome to the Track** - Complete profile setup (50 points)
3. **Getting Started** - Place 3 bets in your first week (150 points)

### Betting Achievements
1. **Win Streak** - Win 3/5/10 bets in a row (100/250/500 points)
2. **Big Winner** - Win over 1000 points in a single bet (200 points)
3. **Consistent Better** - Place bets on 5 consecutive races (300 points)
4. **High Roller** - Place a bet over 5000 points (150 points)
5. **Diversified Portfolio** - Bet on 5 different drivers in a single race (200 points)

### Social Achievements
1. **Community Member** - Join the community forum (50 points)
2. **Social Butterfly** - Like 10 race discussions (100 points)
3. **Thought Leader** - Have your comment liked 5 times (150 points)

### Engagement Achievements
1. **Daily Visitor** - Log in for 7 consecutive days (200 points)
2. **Race Expert** - View 10 race previews (150 points)
3. **Statistician** - Check your stats 5 times in a week (100 points)

## Out of Scope

### Do NOT Modify
- **Core User Entity**: The fundamental `User` entity structure and points system
- **Bet Processing Logic**: The core bet placement, settlement, and payout calculation logic
- **Race Processing**: Existing race status workflows and result processing
- **Authentication System**: User authentication, authorization, and identity management
- **Points System**: The fundamental user points system and transaction mechanisms
- **Database Migrations**: Existing database migrations for current tables

### Avoid Changes To
- **Existing API Endpoints**: Do not modify current user-related or bet-related endpoints
- **Current Frontend Components**: Do not rewrite existing user profile or bet placement pages
- **Caching Infrastructure**: Use existing caching patterns, don't replace the caching system
- **Logging Framework**: Use existing logging services and patterns
- **Error Handling**: Use existing error handling middleware and patterns
- **Notification System**: Use existing notification service, extend it but don't rebuild

### Integration Only
- **BettingService**: Add achievement triggers, don't modify core bet processing
- **UserService**: Add points awarding for achievements, don't change existing points logic
- **Frontend Services**: Use existing API service patterns for new achievement endpoints
- **Existing Services**: Use existing services (RaceService, NotificationService) as-is
- **Frontend Routing**: Add new routes (`/achievements`), don't modify existing navigation

## Estimated Effort
- Database: 2 days
- Service Layer: 5 days
- API Endpoints: 2 days
- Frontend: 6 days
- Testing: 3 days
- Achievement Design: 2 days
- **Total: 20 days**
