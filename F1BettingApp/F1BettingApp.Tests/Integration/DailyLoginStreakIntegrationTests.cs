using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace F1BettingApp.Tests.Integration;

/// <summary>
/// Integration tests for the daily login streak system, covering:
/// - First login (creates streak, awards 10 points)
/// - Consecutive day login (increments streak)
/// - Missed day (resets streak to 1)
/// - Same-day re-login (no duplicate points)
/// - Streak multipliers (day 3 → ×1.5, day 7 → ×2.5)
/// - Point history entries created for each login
/// </summary>
public class DailyLoginStreakIntegrationTests
{
    private PointsSystemTestFactory CreateFactory()
    {
        return new PointsSystemTestFactory();
    }

    [Fact]
    public async Task FirstLogin_CreatesStreakAndAwardsPoints()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_1");

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        var streak = await factory.CreateDbContext().DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        Assert.NotNull(streak);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.True(streak.ClaimedToday);

        Assert.Equal(10, pointsAwarded);
        var refreshedUser = await factory.CreateDbContext().Users.FindAsync(user.Id);
        Assert.Equal(10010, refreshedUser.Points);

        var historyEntries = await factory.CreateDbContext().PointHistories
            .Where(ph => ph.UserId == user.Id && ph.Category == "DailyLogin")
            .ToListAsync();
        Assert.Single(historyEntries);
        Assert.Equal(10, historyEntries[0].Points);
        Assert.Equal("System", historyEntries[0].Source);
    }

    [Fact]
    public async Task ConsecutiveLogin_IncrementsStreak()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_2");
        await factory.SimulateLoginAsync(user.Id);

        // Reload streak to ensure we have the tracked instance
        var ctx = factory.CreateDbContext();
        var streak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        streak!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        streak.ClaimedToday = false;
        await ctx.SaveChangesAsync();

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        var refreshedStreak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        Assert.NotNull(refreshedStreak);
        Assert.Equal(2, refreshedStreak.CurrentStreak);
        Assert.Equal(10, pointsAwarded);

        var refreshedUser = await ctx.Users.FindAsync(user.Id);
        Assert.Equal(10020, refreshedUser.Points);
    }

    [Fact]
    public async Task MissedDay_ResetsStreakToOne()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_3");
        await factory.SimulateLoginAsync(user.Id);

        // Reload streak to ensure we have the tracked instance
        var ctx = factory.CreateDbContext();
        var streak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        streak!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-3);
        streak.ClaimedToday = false;
        await ctx.SaveChangesAsync();

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        var refreshedStreak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        Assert.NotNull(refreshedStreak);
        Assert.Equal(1, refreshedStreak.CurrentStreak);
        Assert.Equal(10, pointsAwarded);
    }

    [Fact]
    public async Task SameDayLogin_NoDuplicatePoints()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_4");
        await factory.SimulateLoginAsync(user.Id);

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        Assert.Equal(0, pointsAwarded);
        var refreshedUser = await factory.CreateDbContext().Users.FindAsync(user.Id);
        Assert.Equal(10010, refreshedUser.Points);

        var historyEntries = await factory.CreateDbContext().PointHistories
            .Where(ph => ph.UserId == user.Id && ph.Category == "DailyLogin")
            .ToListAsync();
        Assert.Single(historyEntries);
    }

    [Fact]
    public async Task StreakDay3_AppliesMultiplier()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_5");
        await factory.SimulateLoginAsync(user.Id);

        // Reload streak to ensure we have the tracked instance
        var ctx = factory.CreateDbContext();
        var streak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        streak!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        streak.ClaimedToday = false;
        streak.CurrentStreak = 2;
        await ctx.SaveChangesAsync();

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        Assert.Equal(15, pointsAwarded);
        var refreshedUser = await ctx.Users.FindAsync(user.Id);
        Assert.Equal(10025, refreshedUser.Points);
    }

    [Fact]
    public async Task StreakDay7_AppliesMaxMultiplier()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_6");
        await factory.SimulateLoginAsync(user.Id);

        // Reload streak to ensure we have the tracked instance
        var ctx = factory.CreateDbContext();
        var streak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        streak!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        streak.ClaimedToday = false;
        streak.CurrentStreak = 6;
        await ctx.SaveChangesAsync();

        var pointsAwarded = await factory.SimulateLoginAsync(user.Id);

        Assert.Equal(25, pointsAwarded);
        var refreshedUser = await ctx.Users.FindAsync(user.Id);
        Assert.Equal(10035, refreshedUser.Points);
    }

    [Fact]
    public async Task StreakHistoryEntryCreated()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("streak_user_7");
        await factory.SimulateLoginAsync(user.Id);

        // Reload streak to ensure we have the tracked instance
        var ctx = factory.CreateDbContext();
        var streak = await ctx.DailyLoginStreaks
            .FirstOrDefaultAsync(s => s.UserId == user.Id);
        streak!.LastLoginDate = DateTime.UtcNow.Date.AddDays(-1);
        streak.ClaimedToday = false;
        await ctx.SaveChangesAsync();

        await factory.SimulateLoginAsync(user.Id);

        var historyEntries = await ctx.PointHistories
            .Where(ph => ph.UserId == user.Id && ph.Category == "DailyLogin")
            .OrderBy(ph => ph.CreatedAt)
            .ToListAsync();

        Assert.Equal(2, historyEntries.Count);
        Assert.Equal(10, historyEntries[0].Points);
        Assert.Equal(10, historyEntries[1].Points);
        Assert.Equal("DailyLogin", historyEntries[0].Category);
        Assert.Equal("DailyLogin", historyEntries[1].Category);
        Assert.Contains("login", historyEntries[0].Description.ToLower());
        Assert.Contains("login", historyEntries[1].Description.ToLower());
    }
}
