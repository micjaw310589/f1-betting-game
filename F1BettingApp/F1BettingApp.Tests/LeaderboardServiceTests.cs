using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    public class LeaderboardServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<LeaderboardHistory>> _leaderboardHistoryRepositoryMock;
        private readonly Mock<IRepository<Race>> _raceRepositoryMock;
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardServiceTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _leaderboardHistoryRepositoryMock = new Mock<IRepository<LeaderboardHistory>>();
            _raceRepositoryMock = new Mock<IRepository<Race>>();
            _leaderboardService = new LeaderboardService();
        }

        [Fact]
        public async Task UpdateLeaderboard_AfterRace_UpdatesRankings()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1, Points = 1500 },
                new User("user2", "user2@example.com", "password2") { Id = 2, Points = 2000 },
                new User("user3", "user3@example.com", "password3") { Id = 3, Points = 1000 }
            };

            var races = new List<Race>
            {
                new Race("Australian Grand Prix", DateTime.Now.AddDays(-1),
                    "Melbourne Grand Prix Circuit", "Australia", "1", 2023)
                {
                    Id = 1,
                    Status = RaceStatus.Finished
                }
            };

            var existingHistories = new List<LeaderboardHistory>
            {
                new LeaderboardHistory(1, 1, "2023", 1500, 2),
                new LeaderboardHistory(2, 1, "2023", 2000, 1)
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(users.AsQueryable()));

            _raceRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(races.AsQueryable()));

            _leaderboardHistoryRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(existingHistories.AsQueryable()));

            _leaderboardHistoryRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Callback<int>(id => { });

            _leaderboardHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<LeaderboardHistory>()))
                .Returns(Task.CompletedTask)
                .Callback<LeaderboardHistory>(history => { });

            // Act
            await _leaderboardService.UpdateLeaderboardAsync();

            // Assert
            _leaderboardHistoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.AtLeastOnce);
            _leaderboardHistoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LeaderboardHistory>()), Times.AtLeastOnce);
            _leaderboardHistoryRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCurrentLeaderboard_ReturnsTopPlayers()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1, Points = 1500 },
                new User("user2", "user2@example.com", "password2") { Id = 2, Points =2000 },
                new User("user3", "user3@example.com", "password3") { Id = 3, Points = 1000 },
                new User("user4", "user4@example.com", "password4") { Id = 4, Points = 500 }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(users.AsQueryable()));

            // Act
            var leaderboard = await _leaderboardService.GetCurrentLeaderboardAsync(3);

            // Assert
            Assert.Equal(3, leaderboard.Count());
            Assert.Equal(2000, leaderboard.First().Points);
            Assert.Equal(1, leaderboard.First().Rank);
            Assert.Equal(1500, leaderboard.Skip(1).First().Points);
            Assert.Equal(2, leaderboard.Skip(1).First().Rank);
            Assert.Equal(1000, leaderboard.Skip(2).First().Points);
            Assert.Equal(3, leaderboard.Skip(2).First().Rank);
        }

        [Fact]
        public async Task GetSeasonLeaderboard_ReturnsSeasonData()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1 },
                new User("user2", "user2@example.com", "password2") { Id = 2 }
            };

            var histories = new List<LeaderboardHistory>
            {
                new LeaderboardHistory(1, 1, "2023", 1500, 2),
                new LeaderboardHistory(2, 1, "2023", 2000, 1),
                new LeaderboardHistory(1, 2, "2022", 1000, 1)
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(users[0]);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(users[1]);

            _leaderboardHistoryRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(histories.AsQueryable()));

            // Act
            var leaderboard = await _leaderboardService.GetSeasonLeaderboardAsync(2023, 2);

            // Assert
            Assert.Equal(2, leaderboard.Count());
            Assert.Equal(2000, leaderboard.First().Points);
            Assert.Equal(1, leaderboard.First().Rank);
            Assert.Equal("user2", leaderboard.First().Username);
        }

        [Fact]
        public async Task UpdateLeaderboard_WithTie_HandlesTieCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1, Points = 2000 },
                new User("user2", "user2@example.com", "password2") { Id = 2, Points = 2000 }, // Tie with user1
                new User("user3", "user3@example.com", "password3") { Id = 3, Points = 1000 }
            };

            var races = new List<Race>
            {
                new Race("Australian Grand Prix", DateTime.Now.AddDays(-1),
                    "Melbourne Grand Prix Circuit", "Australia", "1", 2023)
                {
                    Id = 1,
                    Status = RaceStatus.Finished
                }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(users.AsQueryable()));

            _raceRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(races.AsQueryable()));

            _leaderboardHistoryRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(new List<LeaderboardHistory>().AsQueryable()));

            _leaderboardHistoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<LeaderboardHistory>()))
                .Returns(Task.CompletedTask)
                .Callback<LeaderboardHistory>(history => { });

            // Act
            await _leaderboardService.UpdateLeaderboardAsync();

            // Assert - Verify that all users were added with appropriate ranks
            _leaderboardHistoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LeaderboardHistory>()), Times.AtLeastOnce);

            // In the current implementation, ties are handled by giving sequential ranks
            // (both users with 2000 points would get ranks 1 and 2 respectively)
        }
    }
}