using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    /// <summary>
    /// Unit tests for PointHistoryService.
    /// Tests recording point changes, pagination, and weekly summaries.
    /// </summary>
    public class PointHistoryServiceTests
    {
        private FakePointHistoryRepository _fakeRepo;
        private PointHistoryService _service;

        public PointHistoryServiceTests()
        {
            _fakeRepo = new FakePointHistoryRepository();
            _service = new PointHistoryService(_fakeRepo);
        }

        #region RecordPointChangeAsync Tests

        [Fact]
        public async Task RecordPointChangeAsync_PositivePoints_CreatesEntry()
        {
            // Arrange
            var userId = 1;
            var points = 50;
            var category = "DailyLogin";
            var description = "Login streak day 3";
            var source = "System";

            // Act
            await _service.RecordPointChangeAsync(userId, points, category, description, source);

            // Assert
            var entry = _fakeRepo.Entries.FirstOrDefault(e => e.UserId == userId && e.Points == points);
            Assert.NotNull(entry);
            Assert.Equal(category, entry.Category);
            Assert.Equal(description, entry.Description);
            Assert.Equal(source, entry.Source);
            Assert.Null(entry.ReferenceId);
        }

        [Fact]
        public async Task RecordPointChangeAsync_NegativePoints_CreatesEntry()
        {
            // Arrange
            var userId = 2;
            var points = -500;
            var category = "BetPlacement";
            var description = "Bet on Monaco Grand Prix";
            var source = "Bet";
            var referenceId = 100;

            // Act
            await _service.RecordPointChangeAsync(userId, points, category, description, source, referenceId);

            // Assert
            var entry = _fakeRepo.Entries.FirstOrDefault(e => e.UserId == userId && e.Points == points);
            Assert.NotNull(entry);
            Assert.Equal(category, entry.Category);
            Assert.Equal(description, entry.Description);
            Assert.Equal(source, entry.Source);
            Assert.Equal(referenceId, entry.ReferenceId);
        }

        [Fact]
        public async Task RecordPointChangeAsync_AdminAdjustment_CreatesEntry()
        {
            // Arrange
            var userId = 3;
            var points = 1000;
            var category = "AdminAdjustment";
            var description = "Bonus for reporting bug";
            var source = "Admin";

            // Act
            await _service.RecordPointChangeAsync(userId, points, category, description, source);

            // Assert
            var entry = _fakeRepo.Entries.FirstOrDefault(e => e.UserId == userId && e.Points == points);
            Assert.NotNull(entry);
            Assert.Equal(category, entry.Category);
            Assert.Equal(description, entry.Description);
            Assert.Equal(source, entry.Source);
        }

        #endregion

        #region GetUserPointHistoryAsync Tests

        [Fact]
        public async Task GetUserPointHistoryAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var userId = 1;
            _fakeRepo.AddEntry(new PointHistory { Id = 1, UserId = userId, Points = 50, Category = "DailyLogin", Description = "Login day 1", Source = "System", CreatedAt = DateTime.UtcNow.AddMinutes(-10) });
            _fakeRepo.AddEntry(new PointHistory { Id = 2, UserId = userId, Points = -100, Category = "BetPlacement", Description = "Bet on race", Source = "Bet", CreatedAt = DateTime.UtcNow.AddMinutes(-5) });
            _fakeRepo.AddEntry(new PointHistory { Id = 3, UserId = userId, Points = 200, Category = "BetWin", Description = "Won bet", Source = "Bet", CreatedAt = DateTime.UtcNow });

            var page = 1;
            var pageSize = 10;

            // Act
            var result = await _service.GetUserPointHistoryAsync(userId, page, pageSize);

            // Assert
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(3, result.Items[0].Id); // Newest first
            Assert.Equal(1, result.Items[2].Id); // Oldest last
        }

        [Fact]
        public async Task GetUserPointHistoryAsync_EmptyHistory_ReturnsEmptyResult()
        {
            // Arrange
            var userId = 99;
            var page = 1;
            var pageSize = 20;

            // Act
            var result = await _service.GetUserPointHistoryAsync(userId, page, pageSize);

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.Items.Count);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public async Task GetUserPointHistoryAsync_PaginationWorksCorrectly()
        {
            // Arrange
            var userId = 1;
            _fakeRepo.AddEntry(new PointHistory { Id = 1, UserId = userId, Points = 10, Category = "DailyLogin", Description = "Day 1", Source = "System", CreatedAt = DateTime.UtcNow.AddMinutes(-40) });
            _fakeRepo.AddEntry(new PointHistory { Id = 2, UserId = userId, Points = 20, Category = "DailyLogin", Description = "Day 2", Source = "System", CreatedAt = DateTime.UtcNow.AddMinutes(-30) });
            _fakeRepo.AddEntry(new PointHistory { Id = 3, UserId = userId, Points = 30, Category = "DailyLogin", Description = "Day 3", Source = "System", CreatedAt = DateTime.UtcNow.AddMinutes(-20) });
            _fakeRepo.AddEntry(new PointHistory { Id = 4, UserId = userId, Points = 40, Category = "DailyLogin", Description = "Day 4", Source = "System", CreatedAt = DateTime.UtcNow.AddMinutes(-10) });

            var page = 2;
            var pageSize = 2;

            // Act
            var result = await _service.GetUserPointHistoryAsync(userId, page, pageSize);

            // Assert
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.TotalPages);
            Assert.False(result.HasNextPage); // Page 2 of 2, no next page
            Assert.True(result.HasPreviousPage); // Page 2, has previous page
            Assert.Equal(2, result.Items.Count);
        }

        #endregion

        #region GetWeeklyPointSummaryAsync Tests

        [Fact]
        public async Task GetWeeklyPointSummaryAsync_ReturnsCorrectTotals()
        {
            // Arrange
            var userId = 1;
            var weekNumber = 20;
            var year = 2026;

            // Add entries in the target week
            var weekStart = GetIsoWeekStart(weekNumber, year);
            _fakeRepo.AddEntry(new PointHistory { UserId = userId, Points = 300, Category = "DailyLogin", Description = "Login", Source = "System", CreatedAt = weekStart.AddDays(1) });
            _fakeRepo.AddEntry(new PointHistory { UserId = userId, Points = 200, Category = "Quest", Description = "Quest", Source = "System", CreatedAt = weekStart.AddDays(2) });
            _fakeRepo.AddEntry(new PointHistory { UserId = userId, Points = -150, Category = "BetPlacement", Description = "Bet", Source = "Bet", CreatedAt = weekStart.AddDays(3) });
            _fakeRepo.AddEntry(new PointHistory { UserId = userId, Points = -50, Category = "BetLoss", Description = "Lost bet", Source = "Bet", CreatedAt = weekStart.AddDays(4) });

            // Act
            var result = await _service.GetWeeklyPointSummaryAsync(userId, weekNumber, year);

            // Assert
            Assert.Equal(weekNumber, result.WeekNumber);
            Assert.Equal(year, result.Year);
            Assert.Equal(500, result.TotalEarned);
            Assert.Equal(200, result.TotalSpent);
            Assert.Equal(300, result.NetChange);
        }

        [Fact]
        public async Task GetWeeklyPointSummaryAsync_ZeroActivity_ReturnsZeroTotals()
        {
            // Arrange
            var userId = 2;
            var weekNumber = 20;
            var year = 2026;

            // Act
            var result = await _service.GetWeeklyPointSummaryAsync(userId, weekNumber, year);

            // Assert
            Assert.Equal(0, result.TotalEarned);
            Assert.Equal(0, result.TotalSpent);
            Assert.Equal(0, result.NetChange);
        }

        #endregion

        private static DateTime GetIsoWeekStart(int weekNumber, int year)
        {
            var jan4 = new DateTime(year, 1, 4);
            var dayOfWeek = jan4.DayOfWeek;
            var mondayOfWeek1 = jan4.AddDays(-(dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));
            return mondayOfWeek1.AddDays((weekNumber - 1) * 7);
        }

        /// <summary>
        /// Fake repository implementation for testing.
        /// </summary>
        private class FakePointHistoryRepository : IPointHistoryRepository
        {
            private readonly List<PointHistory> _entries = new();

            public IReadOnlyList<PointHistory> Entries => _entries.AsReadOnly();

            public void AddEntry(PointHistory entry)
            {
                _entries.Add(entry);
            }

            public Task AddAsync(PointHistory entity)
            {
                _entries.Add(entity);
                return Task.CompletedTask;
            }

            public Task<IEnumerable<PointHistory>> GetByUserIdAsync(int userId, int page, int pageSize)
            {
                var query = _entries
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
                return Task.FromResult<IEnumerable<PointHistory>>(query.ToList());
            }

            public Task<int> GetCountByUserIdAsync(int userId)
            {
                return Task.FromResult(_entries.Count(ph => ph.UserId == userId));
            }

            public Task<(int TotalEarned, int TotalSpent)> GetWeeklySummaryAsync(int userId, int weekNumber, int year)
            {
                var (startDate, endDate) = GetIsoWeekBounds(weekNumber, year);
                var entries = _entries.Where(ph => ph.UserId == userId && ph.CreatedAt >= startDate && ph.CreatedAt < endDate).ToList();

                var totalEarned = entries.Where(ph => ph.Points > 0).Sum(ph => ph.Points);
                var totalSpent = Math.Abs(entries.Where(ph => ph.Points < 0).Sum(ph => ph.Points));

                return Task.FromResult((totalEarned, totalSpent));
            }

            private static (DateTime StartDate, DateTime EndDate) GetIsoWeekBounds(int weekNumber, int year)
            {
                var jan4 = new DateTime(year, 1, 4);
                var dayOfWeek = jan4.DayOfWeek;
                var mondayOfWeek1 = jan4.AddDays(-(dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));
                var startDate = mondayOfWeek1.AddDays((weekNumber - 1) * 7);
                var endDate = startDate.AddDays(7);
                return (startDate, endDate);
            }
        }
    }
}
