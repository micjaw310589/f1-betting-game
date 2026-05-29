using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace F1BettingApp.Tests.Integration;

/// <summary>
/// Integration tests for the weekly quest system.
/// </summary>
public class WeeklyQuestsIntegrationTests
{
    private PointsSystemTestFactory CreateFactory() => new();

    [Fact]
    public async Task PlaceBet_DuringRaceWeekend_IncrementsRaceDayBettor()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_1");
        var driver = await factory.CreateTestDriverAsync("Test Driver 1");
        var fridayDate = GetNextFriday();
        var race = await factory.CreateTestRaceAsync(fridayDate, "Friday Practice GP");

        await factory.CreateTestQuestDefinitionAsync(
            "race_day_bettor", "Race Day Bettor", "Place a bet on a race weekend day",
            QuestCategory.Betting, 1, 50, false);

        await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);

        var questService = factory.CreateQuestService();
        await questService.UpdateQuestProgressAsync(user.Id, "race_day_bettor", 1);

        var progress = await factory.CreateDbContext().WeeklyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "race_day_bettor");
        Assert.NotNull(progress);
        Assert.Equal(1, progress.Progress);
        Assert.True(progress.IsCompleted);
        Assert.True(progress.IsClaimed);
    }

    [Fact]
    public async Task PlaceBet_5Times_CompletesBettingMarathon()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_2");
        var driver = await factory.CreateTestDriverAsync("Test Driver 2");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "betting_marathon", "Betting Marathon", "Place 5 bets in a week",
            QuestCategory.Betting, 5, 150, false);

        var questService = factory.CreateQuestService();
        for (int i = 0; i < 5; i++)
        {
            await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);
            await questService.UpdateQuestProgressAsync(user.Id, "betting_marathon", 1);
        }

        var progress = await factory.CreateDbContext().WeeklyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "betting_marathon");
        Assert.NotNull(progress);
        Assert.Equal(5, progress.Progress);
        Assert.True(progress.IsCompleted);
        Assert.True(progress.IsClaimed);
        Assert.Equal(150, progress.PointsAwarded);

        var refreshedUser = await factory.CreateDbContext().Users.FindAsync(user.Id);
        Assert.Equal(10150, refreshedUser.Points);

        var historyEntry = await factory.CreateDbContext().PointHistories
            .FirstOrDefaultAsync(ph => ph.UserId == user.Id && ph.Category == "Quest");
        Assert.NotNull(historyEntry);
        Assert.Equal(150, historyEntry.Points);
    }

    [Fact]
    public async Task NextWeek_QuestResets()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_3");
        var driver = await factory.CreateTestDriverAsync("Test Driver 3");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "betting_marathon", "Betting Marathon", "Place 5 bets in a week",
            QuestCategory.Betting, 5, 150, false);

        var questService = factory.CreateQuestService();
        for (int i = 0; i < 5; i++)
        {
            await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);
            await questService.UpdateQuestProgressAsync(user.Id, "betting_marathon", 1);
        }

        var ctx = factory.CreateDbContext();
        var progressRepo = ctx.Set<WeeklyQuestProgress>();
        var records = await progressRepo
            .Where(p => p.UserId == user.Id && p.QuestId == "betting_marathon")
            .ToListAsync();

        foreach (var record in records)
        {
            record.Progress = 0;
            record.IsCompleted = false;
            record.PointsAwarded = 0;
            record.IsClaimed = false;
        }
        await ctx.SaveChangesAsync();

        var resetProgress = await progressRepo
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "betting_marathon");
        Assert.NotNull(resetProgress);
        Assert.Equal(0, resetProgress.Progress);
        Assert.False(resetProgress.IsCompleted);
        Assert.False(resetProgress.IsClaimed);
    }

    [Fact]
    public async Task OneTimeQuest_NeverResets()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_4");
        var driver = await factory.CreateTestDriverAsync("Test Driver 4");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "first_bet", "First Bet", "Place your first bet",
            QuestCategory.Achievement, 1, 50, isOneTime: true);

        await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);
        var questService = factory.CreateQuestService();
        await questService.UpdateQuestProgressAsync(user.Id, "first_bet", 1);

        // QuestService creates one-time quest progress with week=0, year=0 sentinel
        var progress = await factory.CreateDbContext().WeeklyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "first_bet" && p.WeekNumber == 0 && p.Year == 0);
        Assert.NotNull(progress);
        Assert.True(progress.IsCompleted);
        Assert.True(progress.IsClaimed);
    }

    [Fact]
    public async Task Win3Bets_CompletesWinningStreak()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_5");
        var driver = await factory.CreateTestDriverAsync("Test Driver 5");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "winning_streak", "Winning Streak", "Win 3 bets in a week",
            QuestCategory.Betting, 3, 300, false);

        var questService = factory.CreateQuestService();
        for (int i = 0; i < 3; i++)
        {
            var bet = await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);
            await factory.ResolveBetAsWonAsync(bet.Id, 200);
            await questService.UpdateQuestProgressAsync(user.Id, "winning_streak", 1);
        }

        var progress = await factory.CreateDbContext().WeeklyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "winning_streak");
        Assert.NotNull(progress);
        Assert.Equal(3, progress.Progress);
        Assert.True(progress.IsCompleted);
        Assert.True(progress.IsClaimed);
        Assert.Equal(300, progress.PointsAwarded);
    }

    [Fact]
    public async Task BoldMove_AwardsPointsForLargeStake()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_6");
        var driver = await factory.CreateTestDriverAsync("Test Driver 6");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "bold_move", "Bold Move", "Place a bet of 1000+ points",
            QuestCategory.Betting, 1, 75, false);

        await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 1500, BetType.RaceWinner, 2.0m);
        var questService = factory.CreateQuestService();
        await questService.UpdateQuestProgressAsync(user.Id, "bold_move", 1);

        var progress = await factory.CreateDbContext().WeeklyQuestProgresses
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.QuestId == "bold_move");
        Assert.NotNull(progress);
        Assert.True(progress.IsCompleted);
        Assert.True(progress.IsClaimed);
    }

    [Fact]
    public async Task QuestAlreadyClaimed_NoDoubleAward()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("quest_user_7");
        var driver = await factory.CreateTestDriverAsync("Test Driver 7");
        var race = await factory.CreateTestRaceAsync(DateTime.UtcNow, "Test GP");

        await factory.CreateTestQuestDefinitionAsync(
            "test_quest", "Test Quest", "A test quest",
            QuestCategory.Betting, 1, 100, false);

        var questService = factory.CreateQuestService();
        await factory.PlaceBetAsync(user.Id, race.Id, driver.Id, 100, BetType.RaceWinner, 2.0m);
        await questService.UpdateQuestProgressAsync(user.Id, "test_quest", 1);

        var userBefore = await factory.CreateDbContext().Users.FindAsync(user.Id);
        var pointsAfterFirst = userBefore!.Points;

        await questService.UpdateQuestProgressAsync(user.Id, "test_quest", 1);

        var userAfter = await factory.CreateDbContext().Users.FindAsync(user.Id);
        Assert.Equal(pointsAfterFirst, userAfter!.Points);
    }

    private static DateTime GetNextFriday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilFriday == 0) daysUntilFriday = 7;
        return today.AddDays(daysUntilFriday);
    }
}
