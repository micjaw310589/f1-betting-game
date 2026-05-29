# Daily and Monthly Tasks System - Implementation Task

## Overview
Implement a gamified task system with daily and monthly tasks that reward users with points for completing specific actions, driving engagement and regular usage.

## Requirements
- Create daily tasks that reset every 24 hours
- Create monthly tasks that reset at month start
- Support different task categories (betting, social, engagement, etc.)
- Track task progress and completion
- Award points when tasks are completed
- Prevent exploitation and ensure fairness

## Database Changes

### New Tables
```csharp
public class TaskDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; } // Icon class or image URL
    public int PointsReward { get; set; }
    public TaskType Type { get; set; } // Daily, Weekly, Monthly
    public TaskCategory Category { get; set; } // Betting, Social, Engagement, etc.
    public string Criteria { get; set; } // JSON configuration
    public bool IsActive { get; set; }
    public int Priority { get; set; } // For task selection algorithm
    public int DifficultyLevel { get; set; } // 1-5 scale
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
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
    public string Status { get; set; } // "Assigned", "InProgress", "Completed", "Expired"
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
    public string Period { get; set; } // "2024-05", etc.
}

public class UserTaskResetLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public TaskType ResetType { get; set; }
    public DateTime ResetAt { get; set; }
    public int TasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
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
    Community,
    Profile
}

public enum TaskStatus
{
    Assigned,
    InProgress,
    Completed,
    Expired,
    Claimed
}
```

### Database Seeding
```csharp
public static void SeedTaskDefinitions(AppDbContext context)
{
    var tasks = new List<TaskDefinition>
    {
        // Daily Tasks
        new TaskDefinition
        {
            Name = "Place Your First Bet",
            Description = "Place at least one bet today",
            Icon = "bet",
            PointsReward = 50,
            Type = TaskType.Daily,
            Category = TaskCategory.Betting,
            Criteria = JsonSerializer.Serialize(new {
                minBets = 1,
                resetType = "daily"
            }),
            IsActive = true,
            Priority = 1,
            DifficultyLevel = 1
        },

        new TaskDefinition
        {
            Name = "Social Butterfly",
            Description = "Like or comment on 3 race discussions",
            Icon = "comment",
            PointsReward = 30,
            Type = TaskType.Daily,
            Category = TaskCategory.Social,
            Criteria = JsonSerializer.Serialize(new {
                minInteractions = 3,
                interactionTypes = new[] { "like", "comment" },
                resetType = "daily"
            }),
            IsActive = true,
            Priority = 2,
            DifficultyLevel = 2
        },

        // Monthly Tasks
        new TaskDefinition
        {
            Name = "Consistent Better",
            Description = "Place bets on 5 different races this month",
            Icon = "calendar",
            PointsReward = 200,
            Type = TaskType.Monthly,
            Category = TaskCategory.Betting,
            Criteria = JsonSerializer.Serialize(new {
                minRaces = 5,
                uniqueRaces = true,
                resetType = "monthly"
            }),
            IsActive = true,
            Priority = 1,
            DifficultyLevel = 3
        }
    };

    context.TaskDefinitions.AddRange(tasks);
    context.SaveChanges();
}
```

## Service Layer

### Create ITaskService
```csharp
public interface ITaskService
{
    // User Tasks
    Task<IEnumerable<UserTaskDto>> GetUserTasksAsync(int userId, TaskType? type = null);
    Task<UserTaskDto> GetUserTaskAsync(int userId, int taskId);
    Task<UserTaskDto> CompleteTaskAsync(int userId, int taskId);
    Task ClaimTaskRewardAsync(int userId, int taskId);
    Task<IEnumerable<UserTaskHistoryDto>> GetTaskHistoryAsync(int userId, int limit = 20);

    // Task Management
    Task<IEnumerable<TaskDefinitionDto>> GetTaskDefinitionsAsync(TaskType? type = null);
    Task<TaskDefinitionDto> GetTaskDefinitionAsync(int taskId);
    Task CreateTaskDefinitionAsync(CreateTaskDefinitionDto dto);
    Task UpdateTaskDefinitionAsync(int taskId, UpdateTaskDefinitionDto dto);
    Task DeactivateTaskDefinitionAsync(int taskId);

    // Background Operations
    Task ResetDailyTasksAsync();
    Task ResetMonthlyTasksAsync();
    Task CheckAndAssignTasksAsync(int userId);
    Task RecalculateUserTaskProgressAsync(int userId);

    // Admin
    Task<TaskStatisticsDto> GetTaskStatisticsAsync();
    Task<IEnumerable<UserTaskCompletionDto>> GetUserTaskCompletionsAsync(int taskId);
}
```

### Implement TaskService
```csharp
public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(AppDbContext context, IUserService userService, ILogger<TaskService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IEnumerable<UserTaskDto>> GetUserTasksAsync(int userId, TaskType? type = null)
    {
        var query = _context.UserTasks
            .Where(ut => ut.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(ut => ut.TaskDefinition.Type == type.Value);
        }

        return await query
            .Include(ut => ut.TaskDefinition)
            .Select(ut => new UserTaskDto
            {
                TaskId = ut.TaskDefinitionId,
                Name = ut.TaskDefinition.Name,
                Description = ut.TaskDefinition.Description,
                Icon = ut.TaskDefinition.Icon,
                PointsReward = ut.TaskDefinition.PointsReward,
                Type = ut.TaskDefinition.Type,
                Category = ut.TaskDefinition.Category,
                CurrentProgress = ut.CurrentProgress,
                TargetProgress = ut.TargetProgress,
                Status = ut.Status,
                AssignedAt = ut.AssignedAt,
                CompletedAt = ut.CompletedAt,
                IsClaimed = ut.IsClaimed
            })
            .ToListAsync();
    }

    public async Task<UserTaskDto> CompleteTaskAsync(int userId, int taskId)
    {
        var userTask = await _context.UserTasks
            .Include(ut => ut.TaskDefinition)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TaskDefinitionId == taskId);

        if (userTask == null)
        {
            throw new NotFoundException("Task not found or not assigned to user");
        }

        if (userTask.Status == TaskStatus.Completed || userTask.Status == TaskStatus.Claimed)
        {
            throw new InvalidOperationException("Task already completed");
        }

        // Validate task can be completed
        var criteria = JsonSerializer.Deserialize<TaskCriteria>(userTask.TaskDefinition.Criteria);
        var canComplete = await ValidateTaskCompletion(userId, userTask.TaskDefinitionId, criteria);

        if (!canComplete)
        {
            throw new InvalidOperationException("Task completion requirements not met");
        }

        // Update task status
        userTask.Status = TaskStatus.Completed;
        userTask.CompletedAt = DateTime.UtcNow;
        userTask.CurrentProgress = userTask.TargetProgress;

        await _context.SaveChangesAsync();

        // Log to history
        await LogTaskCompletion(userId, userTask);

        return await GetUserTaskAsync(userId, taskId);
    }

    public async Task ClaimTaskRewardAsync(int userId, int taskId)
    {
        var userTask = await _context.UserTasks
            .Include(ut => ut.TaskDefinition)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TaskDefinitionId == taskId);

        if (userTask == null || userTask.Status != TaskStatus.Completed)
        {
            throw new InvalidOperationException("Task not completed or not found");
        }

        if (userTask.IsClaimed)
        {
            throw new InvalidOperationException("Reward already claimed");
        }

        // Award points
        await _userService.AddPointsAsync(userId, userTask.TaskDefinition.PointsReward);

        // Update task
        userTask.IsClaimed = true;
        userTask.ClaimedAt = DateTime.UtcNow;
        userTask.Status = TaskStatus.Claimed;

        await _context.SaveChangesAsync();

        // Log to history
        await LogTaskClaim(userId, userTask);
    }
}
```

## Background Workers

### Create TaskResetJob
```csharp
public class TaskResetJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskResetJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);

    public TaskResetJob(IServiceScopeFactory scopeFactory, ILogger<TaskResetJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                // Check for daily reset (midnight UTC)
                if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute < 15)
                {
                    _logger.LogInformation("Starting daily task reset...");
                    await taskService.ResetDailyTasksAsync();
                }

                // Check for monthly reset (1st of month, midnight UTC)
                if (DateTime.UtcNow.Day == 1 && DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute < 15)
                {
                    _logger.LogInformation("Starting monthly task reset...");
                    await taskService.ResetMonthlyTasksAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in task reset job");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
```

### Create TaskAssignmentJob
```csharp
public class TaskAssignmentJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskAssignmentJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public TaskAssignmentJob(IServiceScopeFactory scopeFactory, ILogger<TaskAssignmentJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                // Get users who need task assignment
                var users = await userService.GetActiveUsersAsync();

                foreach (var user in users)
                {
                    try
                    {
                        await taskService.CheckAndAssignTasksAsync(user.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error assigning tasks to user {UserId}", user.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in task assignment job");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
```

## Task Assignment Algorithm
```csharp
public async Task CheckAndAssignTasksAsync(int userId)
{
    // Get user's current tasks
    var currentTasks = await _context.UserTasks
        .Where(ut => ut.UserId == userId)
        .ToListAsync();

    // Get available task definitions
    var taskDefinitions = await _context.TaskDefinitions
        .Where(td => td.IsActive)
        .ToListAsync();

    // Daily tasks - ensure user has 3-5 daily tasks
    var dailyTasks = currentTasks.Where(ut => ut.TaskDefinition.Type == TaskType.Daily).ToList();
    if (dailyTasks.Count < 3)
    {
        await AssignTasksOfType(userId, TaskType.Daily, 5 - dailyTasks.Count, taskDefinitions);
    }

    // Monthly tasks - ensure user has 2-3 monthly tasks
    var monthlyTasks = currentTasks.Where(ut => ut.TaskDefinition.Type == TaskType.Monthly).ToList();
    if (monthlyTasks.Count < 2)
    {
        await AssignTasksOfType(userId, TaskType.Monthly, 3 - monthlyTasks.Count, taskDefinitions);
    }
}

private async Task AssignTasksOfType(int userId, TaskType taskType, int count, List<TaskDefinition> allDefinitions)
{
    // Get eligible task definitions
    var eligibleTasks = allDefinitions
        .Where(td => td.Type == taskType)
        .Where(td => !_context.UserTasks.Any(ut =>
            ut.UserId == userId &&
            ut.TaskDefinitionId == td.Id &&
            ut.TaskDefinition.Type == taskType))
        .OrderBy(td => td.Priority)
        .ThenBy(td => Guid.NewGuid()) // Add randomness
        .Take(count)
        .ToList();

    foreach (var taskDef in eligibleTasks)
    {
        var criteria = JsonSerializer.Deserialize<TaskCriteria>(taskDef.Criteria);

        var userTask = new UserTask
        {
            UserId = userId,
            TaskDefinitionId = taskDef.Id,
            AssignedAt = DateTime.UtcNow,
            Status = TaskStatus.Assigned,
            CurrentProgress = 0,
            TargetProgress = criteria.targetProgress ?? 1
        };

        _context.UserTasks.Add(userTask);
    }

    await _context.SaveChangesAsync();
}
```

## API Endpoints

### Add TasksController
```csharp
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<TaskDefinitionDto>>> GetTaskDefinitions(
        [FromQuery] TaskType? type = null)
    {
        var tasks = await _taskService.GetTaskDefinitionsAsync(type);
        return Ok(tasks);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserTaskDto>>> GetUserTasks(
        int userId,
        [FromQuery] TaskType? type = null)
    {
        var tasks = await _taskService.GetUserTasksAsync(userId, type);
        return Ok(tasks);
    }

    [HttpGet("user/{userId}/task/{taskId}")]
    public async Task<ActionResult<UserTaskDto>> GetUserTask(int userId, int taskId)
    {
        var task = await _taskService.GetUserTaskAsync(userId, taskId);
        return Ok(task);
    }

    [HttpPost("user/{userId}/task/{taskId}/complete")]
    public async Task<ActionResult<UserTaskDto>> CompleteTask(int userId, int taskId)
    {
        var task = await _taskService.CompleteTaskAsync(userId, taskId);
        return Ok(task);
    }

    [HttpPost("user/{userId}/task/{taskId}/claim")]
    public async Task<ActionResult> ClaimTaskReward(int userId, int taskId)
    {
        await _taskService.ClaimTaskRewardAsync(userId, taskId);
        return Ok(new { success = true });
    }

    [HttpGet("user/{userId}/history")]
    public async Task<ActionResult<PagedResult<UserTaskHistoryDto>>> GetTaskHistory(
        int userId,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        var history = await _taskService.GetTaskHistoryAsync(userId, limit, offset);
        return Ok(history);
    }

    [HttpPost("admin/reset/daily")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ResetDailyTasks()
    {
        await _taskService.ResetDailyTasksAsync();
        return Ok(new { success = true });
    }

    [HttpPost("admin/reset/monthly")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ResetMonthlyTasks()
    {
        await _taskService.ResetMonthlyTasksAsync();
        return Ok(new { success = true });
    }
}
```

## DTOs

### TaskDefinitionDto
```csharp
public class TaskDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public TaskType Type { get; set; }
    public TaskCategory Category { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public int DifficultyLevel { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskDefinitionDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public TaskType Type { get; set; }
    public TaskCategory Category { get; set; }
    public string Criteria { get; set; }
    public int Priority { get; set; }
    public int DifficultyLevel { get; set; }
}
```

### UserTaskDto
```csharp
public class UserTaskDto
{
    public int TaskId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsReward { get; set; }
    public TaskType Type { get; set; }
    public TaskCategory Category { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetProgress { get; set; }
    public string Status { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsClaimed { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public double CompletionPercentage => TargetProgress > 0
        ? (double)CurrentProgress / TargetProgress * 100
        : 0;
}
```

### UserTaskHistoryDto
```csharp
public class UserTaskHistoryDto
{
    public int TaskId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int PointsEarned { get; set; }
    public TaskType Type { get; set; }
    public TaskCategory Category { get; set; }
    public DateTime CompletedAt { get; set; }
    public string Period { get; set; }
}
```

## Frontend Integration

### Tasks Dashboard
- Create `/tasks` route
- Implement tabbed interface for Daily/Monthly tasks
- Display tasks in cards with progress indicators
- Show countdown timers to next reset
- Add task completion buttons

### Task Detail View
- Show detailed task information
- Display progress bar with percentage
- Show reward information
- Add claim button for completed tasks

### Task Completion Flow
- Implement task completion confirmation
- Add visual feedback when tasks are completed
- Show points awarded animation

### Notifications
- Add task completion reminders
- Implement new task assignment notifications
- Add reset countdown notifications

### Profile Integration
- Add task progress summary to dashboard
- Show recent task completions
- Display task points in points breakdown

## Task Examples

### Daily Tasks
```json
[
  {
    "name": "Place Your First Bet",
    "description": "Place at least one bet today",
    "type": "Daily",
    "category": "Betting",
    "reward": 50,
    "criteria": {
      "minBets": 1,
      "resetType": "daily"
    }
  },
  {
    "name": "Social Butterfly",
    "description": "Like or comment on 3 race discussions",
    "type": "Daily",
    "category": "Social",
    "reward": 30,
    "criteria": {
      "minInteractions": 3,
      "interactionTypes": ["like", "comment"],
      "resetType": "daily"
    }
  },
  {
    "name": "Race Researcher",
    "description": "View 2 race previews or analyses",
    "type": "Daily",
    "category": "Learning",
    "reward": 25,
    "criteria": {
      "minViews": 2,
      "contentTypes": ["race_preview", "analysis"],
      "resetType": "daily"
    }
  }
]
```

### Monthly Tasks
```json
[
  {
    "name": "Consistent Better",
    "description": "Place bets on 5 different races this month",
    "type": "Monthly",
    "category": "Betting",
    "reward": 200,
    "criteria": {
      "minRaces": 5,
      "uniqueRaces": true,
      "resetType": "monthly"
    }
  },
  {
    "name": "Knowledge Seeker",
    "description": "Read 10 race previews or analyses",
    "type": "Monthly",
    "category": "Learning",
    "reward": 150,
    "criteria": {
      "minArticles": 10,
      "contentTypes": ["race_preview", "analysis", "driver_profile"],
      "resetType": "monthly"
    }
  },
  {
    "name": "Community Contributor",
    "description": "Have 5 of your comments liked by others",
    "type": "Monthly",
    "category": "Social",
    "reward": 180,
    "criteria": {
      "minLikesReceived": 5,
      "resetType": "monthly"
    }
  }
]
```

## Security Considerations

### Anti-Cheating Measures
```csharp
// Validate task completion server-side
private async Task<bool> ValidateTaskCompletion(int userId, int taskId, TaskCriteria criteria)
{
    switch (criteria.trigger)
    {
        case "bets_placed":
            var betsToday = await _context.Bets
                .CountAsync(b => b.UserId == userId &&
                                b.PlacedAt >= DateTime.UtcNow.Date &&
                                b.Status != BetStatus.Cancelled);
            return betsToday >= criteria.minBets;

        case "social_interactions":
            var interactionsToday = await _context.UserInteractions
                .CountAsync(ui => ui.UserId == userId &&
                                  ui.CreatedAt >= DateTime.UtcNow.Date &&
                                  criteria.interactionTypes.Contains(ui.Type));
            return interactionsToday >= criteria.minInteractions;

        // Add more validation cases...

        default:
            return false;
    }
}

// Rate limiting
[HttpPost("complete")]
[RateLimit(20, "1:hour")] // Max 20 completions per hour
public async Task<ActionResult> CompleteTask(int taskId)
{
    // ... completion logic
}
```

### Data Integrity
- Implement proper transaction handling
- Add audit logging for all task operations
- Validate all task criteria server-side
- Prevent duplicate task completions

## Testing Requirements

### Unit Tests
- Test task assignment algorithm
- Test completion validation logic
- Test reset functionality
- Test progress calculation

### Integration Tests
- Test full task lifecycle (assignment → completion → claiming)
- Test reset processes
- Test points awarding
- Test notification triggers

### UI Tests
- Test tasks dashboard responsiveness
- Test task completion flow
- Test countdown timers
- Test notification display

## Success Criteria
- Tasks assign correctly to users
- Daily tasks reset at midnight UTC
- Monthly tasks reset on the 1st of each month
- Task completion validation works correctly
- Points are awarded accurately
- UI shows correct task status and progress
- System prevents task exploitation

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
- **BettingService**: Add task completion triggers, don't modify core bet processing
- **UserService**: Add points awarding for tasks, don't change existing points logic
- **Frontend Services**: Use existing API service patterns for new task endpoints
- **Existing Services**: Use existing services (RaceService, NotificationService) as-is
- **Frontend Routing**: Add new routes (`/tasks`), don't modify existing navigation
- **Background Workers**: Add new workers, don't modify existing RaceStatusMonitorJob

## Estimated Effort
- Database: 2 days
- Service Layer: 5 days
- Background Workers: 3 days
- API Endpoints: 2 days
- Frontend: 5 days
- Testing: 3 days
- Task Design: 2 days
- **Total: 22 days**
