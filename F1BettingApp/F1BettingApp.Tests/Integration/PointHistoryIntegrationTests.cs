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
/// Integration tests for the point history system.
/// </summary>
public class PointHistoryIntegrationTests
{
    private PointsSystemTestFactory CreateFactory() => new();

    [Fact]
    public async Task BetPlacement_CreatesNegativeHistoryEntry()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_1");
        var pointHistoryService = factory.CreatePointHistoryService();

        await pointHistoryService.RecordPointChangeAsync(
            user.Id, -500, "BetPlacement", "Bet on Test GP", "Bet", null);

        var entry = await factory.CreateDbContext().PointHistories
            .FirstOrDefaultAsync(ph => ph.UserId == user.Id && ph.Category == "BetPlacement");
        Assert.NotNull(entry);
        Assert.Equal(-500, entry.Points);
        Assert.Equal("BetPlacement", entry.Category);
        Assert.Equal("Bet", entry.Source);
    }

    [Fact]
    public async Task BetWin_CreatesPositiveHistoryEntry()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_2");
        var pointHistoryService = factory.CreatePointHistoryService();

        await pointHistoryService.RecordPointChangeAsync(
            user.Id, 1000, "BetWin", "Won bet on Test GP", "Bet", 1);

        var entry = await factory.CreateDbContext().PointHistories
            .FirstOrDefaultAsync(ph => ph.UserId == user.Id && ph.Category == "BetWin");
        Assert.NotNull(entry);
        Assert.Equal(1000, entry.Points);
        Assert.Equal("BetWin", entry.Category);
    }

    [Fact]
    public async Task DailyLogin_CreatesHistoryEntry()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_3");
        await factory.SimulateLoginAsync(user.Id);

        var entry = await factory.CreateDbContext().PointHistories
            .FirstOrDefaultAsync(ph => ph.UserId == user.Id && ph.Category == "DailyLogin");
        Assert.NotNull(entry);
        Assert.Equal(10, entry.Points);
        Assert.Equal("DailyLogin", entry.Category);
        Assert.Equal("System", entry.Source);
    }

    [Fact]
    public async Task QuestCompletion_CreatesHistoryEntry()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_4");
        var pointHistoryService = factory.CreatePointHistoryService();

        await pointHistoryService.RecordPointChangeAsync(
            user.Id, 200, "Quest", "Quest: First Bet", "System", null);

        var entry = await factory.CreateDbContext().PointHistories
            .FirstOrDefaultAsync(ph => ph.UserId == user.Id && ph.Category == "Quest");
        Assert.NotNull(entry);
        Assert.Equal(200, entry.Points);
        Assert.Equal("Quest", entry.Category);
        Assert.Equal("System", entry.Source);
    }

    [Fact]
    public async Task HistoryPaginatedCorrectly()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_5");
        var pointHistoryService = factory.CreatePointHistoryService();

        for (int i = 1; i <= 15; i++)
        {
            await pointHistoryService.RecordPointChangeAsync(
                user.Id, i * 10, "TestCategory", $"Test entry {i}", "System");
        }

        var page1 = await pointHistoryService.GetUserPointHistoryAsync(user.Id, page: 1, pageSize: 5);
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(5, page1.PageSize);
        Assert.Equal(5, page1.Items.Count());
        Assert.Equal(3, page1.TotalPages);
        Assert.True(page1.HasNextPage);
        Assert.False(page1.HasPreviousPage);

        var page2 = await pointHistoryService.GetUserPointHistoryAsync(user.Id, page: 2, pageSize: 5);
        Assert.Equal(3, page2.TotalPages);
        Assert.Equal(2, page2.PageNumber);
        Assert.Equal(5, page2.Items.Count());
        Assert.True(page2.HasNextPage);
        Assert.True(page2.HasPreviousPage);

        var page3 = await pointHistoryService.GetUserPointHistoryAsync(user.Id, page: 3, pageSize: 5);
        Assert.Equal(5, page3.Items.Count());
        Assert.False(page3.HasNextPage);
        Assert.True(page3.HasPreviousPage);
    }

    [Fact]
    public async Task WeeklySummary_CalculatesCorrectTotals()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_6");
        var pointHistoryService = factory.CreatePointHistoryService();

        var (weekNumber, year) = factory.GetCurrentIsoWeek();

        await pointHistoryService.RecordPointChangeAsync(user.Id, 100, "DailyLogin", "Login", "System");
        await pointHistoryService.RecordPointChangeAsync(user.Id, 50, "Quest", "Quest reward", "System");
        await pointHistoryService.RecordPointChangeAsync(user.Id, -200, "BetPlacement", "Bet placed", "Bet");
        await pointHistoryService.RecordPointChangeAsync(user.Id, 300, "BetWin", "Bet won", "Bet");

        var summary = await pointHistoryService.GetWeeklyPointSummaryAsync(user.Id, weekNumber, year);
        Assert.Equal(450, summary.TotalEarned);
        Assert.Equal(200, summary.TotalSpent);
    }

    [Fact]
    public async Task HistoryEntriesOrderedNewestFirst()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_7");
        var pointHistoryService = factory.CreatePointHistoryService();

        await pointHistoryService.RecordPointChangeAsync(user.Id, 10, "Category1", "First", "System");
        await pointHistoryService.RecordPointChangeAsync(user.Id, 20, "Category2", "Second", "System");
        await pointHistoryService.RecordPointChangeAsync(user.Id, 30, "Category3", "Third", "System");

        var history = await pointHistoryService.GetUserPointHistoryAsync(user.Id, page: 1, pageSize: 10);
        var items = history.Items.ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal("Third", items[0].Description);
        Assert.Equal("Second", items[1].Description);
        Assert.Equal("First", items[2].Description);
    }

    [Fact]
    public async Task EmptyHistory_ReturnsEmptyResult()
    {
        using var factory = CreateFactory();
        var user = await factory.CreateTestUserAsync("history_user_8");
        var pointHistoryService = factory.CreatePointHistoryService();

        var history = await pointHistoryService.GetUserPointHistoryAsync(user.Id, page: 1, pageSize: 10);
        Assert.Empty(history.Items);
        Assert.Equal(0, history.TotalCount);
        Assert.Equal(1, history.PageNumber);
        Assert.Equal(0, history.TotalPages);
    }
}
