using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Domain.Events;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Tests.Integration;

/// <summary>
/// Factory for creating integration test fixtures with an in-memory database.
/// Provides helper methods for setting up test data and exercising the full pipeline.
/// Services are created manually to avoid complex DI resolution.
/// </summary>
public class PointsSystemTestFactory : IDisposable
{
    private readonly string _dbName;
    private AppDbContext? _context;

    public PointsSystemTestFactory()
    {
        _dbName = $"F1BettingTestDb_{Guid.NewGuid():N}";
        _context = CreateDbContext(_dbName);
    }

    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = new AppDbContext(options);
        // Ensure database is seeded
        ctx.Database.EnsureCreated();
        return ctx;
    }

    public AppDbContext CreateDbContext()
    {
        return _context!;
    }

    public IDailyLoginService CreateDailyLoginService()
    {
        return new DailyLoginService(
            new DailyLoginStreakRepository(_context!),
            new UserRepository(_context!, null!),
            new NoOpDomainEventPublisher(),
            new PointHistoryService(new PointHistoryRepository(_context!)));
    }

    public IQuestService CreateQuestService()
    {
        return new QuestService(
            new QuestDefinitionRepository(_context!),
            new WeeklyQuestProgressRepository(_context!),
            new UserRepository(_context!, null!),
            new NoOpDomainEventPublisher(),
            new PointHistoryService(new PointHistoryRepository(_context!)));
    }

    public IPointHistoryService CreatePointHistoryService()
    {
        return new PointHistoryService(new PointHistoryRepository(_context!));
    }

    public IQuestDefinitionService CreateQuestDefinitionService()
    {
        return new QuestDefinitionService(
            new QuestDefinitionRepository(_context!),
            new WeeklyQuestProgressRepository(_context!));
    }

    /// <summary>
    /// Creates a test user with 10,000 starting points.
    /// </summary>
    public async Task<User> CreateTestUserAsync(string? username = null, string? email = null)
    {
        var user = new User(
            username ?? $"testuser_{Guid.NewGuid().ToString("N")[..6]}",
            email ?? $"testuser_{Guid.NewGuid().ToString("N")[..6]}@test.com",
            "hashedpassword",
            isActive: true,
            isAdmin: false)
        {
            Id = _context!.Users.Any() ? _context.Users.Max(u => u.Id) + 1 : 1,
            Points = 10000,
            CreatedAt = DateTime.UtcNow
        };
        await _context!.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Creates a test race with a specific date.
    /// </summary>
    public async Task<Race> CreateTestRaceAsync(DateTime date, string? name = null)
    {
        var race = new Race(
            name ?? $"Grand Prix {Guid.NewGuid().ToString("N")[..4]}",
            date,
            "Test Circuit",
            "Test Country",
            $"f1_race_{Guid.NewGuid().ToString("N")[..8]}",
            2024)
        {
            Id = _context!.Races.Any() ? _context.Races.Max(r => r.Id) + 1 : 1
        };
        await _context!.Races.AddAsync(race);
        await _context.SaveChangesAsync();
        return race;
    }

    /// <summary>
    /// Creates a test driver.
    /// </summary>
    public async Task<Driver> CreateTestDriverAsync(string name, string? teamName = null)
    {
        var teamId = 1;
        if (!_context!.Teams.Any())
        {
            var team = new Team("Test Team", "Test Country", "f1_team_test") { Id = 1 };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
        }
        else
        {
            teamId = _context.Teams.First().Id;
        }

        var driver = new Driver(name, "Test", $"f1_driver_{Guid.NewGuid().ToString("N")[..8]}", teamId)
        {
            Id = _context!.Drivers.Any() ? _context.Drivers.Max(d => d.Id) + 1 : 1
        };
        await _context!.Drivers.AddAsync(driver);
        await _context.SaveChangesAsync();
        return driver;
    }

    /// <summary>
    /// Creates a test quest definition.
    /// </summary>
    public async Task<QuestDefinition> CreateTestQuestDefinitionAsync(
        string questId,
        string name,
        string description,
        QuestCategory category,
        int target,
        int pointsReward,
        bool isOneTime = false,
        bool isActive = true)
    {
        var quest = new QuestDefinition
        {
            Id = _context!.QuestDefinitions.Any() ? _context.QuestDefinitions.Max(q => q.Id) + 1 : 1,
            QuestId = questId,
            Name = name,
            Description = description,
            Category = category,
            Target = target,
            PointsReward = pointsReward,
            IsOneTime = isOneTime,
            IsActive = isActive,
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _context!.QuestDefinitions.AddAsync(quest);
        await _context.SaveChangesAsync();
        return quest;
    }

    /// <summary>
    /// Simulates a user login, triggering the daily login streak processing.
    /// </summary>
    public async Task<int> SimulateLoginAsync(int userId)
    {
        var service = CreateDailyLoginService();
        return await service.ProcessDailyLoginAsync(userId);
    }

    /// <summary>
    /// Places a bet for a user on a race.
    /// </summary>
    public async Task<Bet> PlaceBetAsync(int userId, int raceId, int driverId, decimal amount, BetType betType, decimal odds)
    {
        var bet = new Bet(userId, raceId, driverId, amount, betType, odds);
        await _context!.Bets.AddAsync(bet);
        await _context.SaveChangesAsync();
        return bet;
    }

    /// <summary>
    /// Resolves a bet as won.
    /// </summary>
    public async Task<Bet> ResolveBetAsWonAsync(int betId, decimal winnings)
    {
        var bet = await _context!.Bets.FindAsync(betId);
        if (bet == null) throw new InvalidOperationException($"Bet {betId} not found.");

        bet.Status = BetStatus.Resolved;
        bet.Winnings = winnings;
        bet.ResolvedAt = DateTime.UtcNow;
        bet.ResolveBet();

        var user = await _context!.Users.FindAsync(bet.UserId);
        if (user != null)
        {
            user.AddPoints((int)winnings);
        }

        await _context.SaveChangesAsync();
        return bet;
    }

    /// <summary>
    /// Resolves a bet as lost.
    /// </summary>
    public async Task<Bet> ResolveBetAsLostAsync(int betId)
    {
        var bet = await _context!.Bets.FindAsync(betId);
        if (bet == null) throw new InvalidOperationException($"Bet {betId} not found.");

        bet.Status = BetStatus.Resolved;
        bet.Winnings = 0;
        bet.ResolvedAt = DateTime.UtcNow;
        bet.ResolveBet();

        await _context.SaveChangesAsync();
        return bet;
    }

    /// <summary>
    /// Gets the current ISO week number and year.
    /// </summary>
    public (int WeekNumber, int Year) GetCurrentIsoWeek()
    {
        var calendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
        var weekNumber = calendar.GetWeekOfYear(
            DateTime.UtcNow,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
        var year = DateTime.UtcNow.Year;

        if (weekNumber == 1 && DateTime.UtcNow.Month == 12)
            year++;
        else if (weekNumber >= 52 && DateTime.UtcNow.Month == 1)
            year--;

        return (weekNumber, year);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

/// <summary>
/// No-op domain event publisher for tests (events are not critical for integration tests).
/// </summary>
public class NoOpDomainEventPublisher : IDomainEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        return Task.CompletedTask;
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        // No-op for tests
    }

    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        // No-op for tests
    }
}
