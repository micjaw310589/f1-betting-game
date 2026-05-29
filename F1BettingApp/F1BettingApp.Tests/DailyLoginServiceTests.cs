using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Events;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using F1BettingApp.Tests.Builders;
using Moq;
using Xunit;

namespace F1BettingApp.Tests;

/// <summary>
/// Unit tests for DailyLoginService, covering:
/// - New user login (creates streak, awards 10 points)
/// - Consecutive day login (increments streak, awards points)
/// - Same-day re-login (no points awarded, returns early)
/// - Missed day (streak resets to 1, awards 10 points)
/// - Streak day 3 (multiplier x1.5 applied)
/// - Streak day 7 (multiplier x2.5 applied)
/// - Concurrent login from two devices (only one awards points)
/// - GetStreakInfoAsync returns correct DTO
/// </summary>
public class DailyLoginServiceTests
{
    private readonly Mock<IDailyLoginStreakRepository> _streakRepoMock;
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IDomainEventPublisher> _eventPublisherMock;
    private readonly DailyLoginService _service;

    public DailyLoginServiceTests()
    {
        _streakRepoMock = new Mock<IDailyLoginStreakRepository>();
        _userRepoMock = new Mock<IRepository<User>>();
        _eventPublisherMock = new Mock<IDomainEventPublisher>();
        _service = new DailyLoginService(
            _streakRepoMock.Object,
            _userRepoMock.Object,
            _eventPublisherMock.Object);
    }

    #region ProcessDailyLoginAsync Tests

    [Fact]
    public async Task ProcessDailyLoginAsync_NewUser_CreatesStreakAndAwardsPoints()
    {
        // Arrange
        var userId = 1;
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((DailyLoginStreak?)null);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(10, points);
        _streakRepoMock.Verify(r => r.UpsertAsync(It.Is<DailyLoginStreak>(s =>
            s.UserId == userId &&
            s.CurrentStreak == 1 &&
            s.ClaimedToday == true
        )), Times.Once);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Points == 10010)), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.Is<PointsAwardedEvent>(e =>
            e.UserId == userId && e.Points == 10 && e.Reason.Contains("day 1")
        )), Times.Once);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_ConsecutiveDay_IncrementsStreakAndAwardsPoints()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 3,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(15, points); // Day 4 -> x1.5 multiplier -> 15 points
        Assert.Equal(4, existingStreak.CurrentStreak);
        Assert.True(existingStreak.ClaimedToday);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Points == 10015)), Times.Once);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.Is<PointsAwardedEvent>(e =>
            e.Points == 15
        )), Times.Once);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_SameDayAlreadyClaimed_ReturnsZeroPoints()
    {
        // Arrange
        var userId = 1;
        var today = DateTime.UtcNow.Date;

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 5,
            LastLoginDate = today,
            ClaimedToday = true
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(0, points);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<PointsAwardedEvent>()), Times.Never);
        _streakRepoMock.Verify(r => r.UpsertAsync(It.IsAny<DailyLoginStreak>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_MissedDay_ResetsStreakToOne()
    {
        // Arrange
        var userId = 1;
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 5,
            LastLoginDate = twoDaysAgo,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(10, points); // Reset to day 1 -> x1.0 multiplier -> 10 points
        Assert.Equal(1, existingStreak.CurrentStreak);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Points == 10010)), Times.Once);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_Day3_Multiplier1_5Applied()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 2,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(15, points); // Day 3 -> x1.5 multiplier -> 15 points
        Assert.Equal(3, existingStreak.CurrentStreak);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_Day7_Multiplier2_5Applied()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 6,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(25, points); // Day 7 -> x2.5 multiplier -> 25 points
        Assert.Equal(7, existingStreak.CurrentStreak);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_Day8_Multiplier2_5Applied()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 7,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingStreak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(25, points); // Day 8 -> x2.5 multiplier -> 25 points
        Assert.Equal(8, existingStreak.CurrentStreak);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_ConcurrentLoginOnlyOneAwardsPoints()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        // First call: streak not yet claimed
        var existingStreak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 2,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };

        // Simulate first login claiming the streak
        _streakRepoMock.SetupSequence(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(existingStreak)  // First call: not claimed
            .ReturnsAsync(new DailyLoginStreak  // Second call: already claimed
            {
                Id = 1,
                UserId = userId,
                CurrentStreak = 3,
                LastLoginDate = DateTime.UtcNow.Date,
                ClaimedToday = true
            });

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act - simulate concurrent logins
        var task1 = _service.ProcessDailyLoginAsync(userId);
        var task2 = _service.ProcessDailyLoginAsync(userId);
        var results = await Task.WhenAll(task1, task2);

        // Assert - only one should award points (the one that got ClaimedToday=false)
        var totalPoints = results.Sum();
        Assert.True(totalPoints == 15 || totalPoints == 25,
            $"Expected one login to award points (15 or 25), but got total: {totalPoints}");
    }

    #endregion

    #region GetStreakInfoAsync Tests

    [Fact]
    public async Task GetStreakInfoAsync_NoStreakRecord_ReturnsNull()
    {
        // Arrange
        var userId = 1;
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((DailyLoginStreak?)null);

        // Act
        var result = await _service.GetStreakInfoAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStreakInfoAsync_WithStreak_ReturnsCorrectInfo()
    {
        // Arrange
        var userId = 1;
        var lastLogin = DateTime.UtcNow.Date.AddDays(-2);

        var streak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 5,
            LastLoginDate = lastLogin,
            ClaimedToday = true
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.GetStreakInfoAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.CurrentStreak);
        Assert.Equal(20, result.PointsToday); // Day 5 -> x2.0 -> 20 points
        Assert.Equal(7, result.NextBonusMilestone);
        Assert.Equal(25, result.PointsAtNextMilestone);
    }

    [Fact]
    public async Task GetStreakInfoAsync_MaxStreak_ReturnsNullMilestones()
    {
        // Arrange
        var userId = 1;

        var streak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 10,
            LastLoginDate = DateTime.UtcNow.Date,
            ClaimedToday = true
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.GetStreakInfoAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.CurrentStreak);
        Assert.Equal(25, result.PointsToday); // Max multiplier
        Assert.Null(result.NextBonusMilestone);
        Assert.Null(result.PointsAtNextMilestone);
    }

    [Fact]
    public async Task GetStreakInfoAsync_NotClaimedToday_ReturnsZeroPointsToday()
    {
        // Arrange
        var userId = 1;

        var streak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = 3,
            LastLoginDate = DateTime.UtcNow.Date.AddDays(-1),
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.GetStreakInfoAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.CurrentStreak);
        Assert.Equal(0, result.PointsToday); // Not claimed today
        Assert.Equal(5, result.NextBonusMilestone);
        Assert.Equal(20, result.PointsAtNextMilestone);
    }

    #endregion

    #region Point Calculation Tests

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(3, 15)]
    [InlineData(4, 15)]
    [InlineData(5, 20)]
    [InlineData(6, 20)]
    [InlineData(7, 25)]
    [InlineData(10, 25)]
    [InlineData(30, 25)]
    public void CalculatePoints_CorrectMultiplier(int streakDays, int expectedPoints)
    {
        // Arrange & Act - we test via the service's behavior
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var streak = new DailyLoginStreak
        {
            Id = 1,
            UserId = userId,
            CurrentStreak = streakDays - 1,
            LastLoginDate = yesterday,
            ClaimedToday = false
        };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        var user = new UserBuilder().WithId(userId).WithPoints(10000).Build();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var points = _service.ProcessDailyLoginAsync(userId).Result;

        // Assert
        Assert.Equal(expectedPoints, points);
    }

    #endregion
}
