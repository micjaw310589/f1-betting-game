using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<Bet>> _betRepositoryMock;
        private readonly Mock<IRepository<Result>> _resultRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _betRepositoryMock = new Mock<IRepository<Bet>>();
            _resultRepositoryMock = new Mock<IRepository<Result>>();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _betRepositoryMock.Object,
                _resultRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserLeaderboardPosition_ReturnsCorrectPosition()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1, Points = 1500 },
                new User("user2", "user2@example.com", "password2") { Id = 2, Points = 2000 },
                new User("user3", "user3@example.com", "password3") { Id = 3, Points = 1000 }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(users.AsQueryable()));

            // Act
            var position = await _userService.GetUserLeaderboardPositionAsync(2);

            // Assert
            Assert.Equal(1, position); // user2 has highest points (2000)
        }

        [Fact]
        public async Task UpdateUserPoints_WithPositiveAmount_Succeeds()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1, Points = 1000 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.UpdateUserPointsAsync(1, 500);

            // Assert
            Assert.Equal(1500, user.Points);
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserPoints_WithNegativeAmount_Succeeds()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1, Points = 1000 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.UpdateUserPointsAsync(1, -300);

            // Assert
            Assert.Equal(700, user.Points);
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserPoints_WithNegativeAmount_EnsuresNonNegative()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1, Points = 200 };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.UpdateUserPointsAsync(1, -300);

            // Assert
            Assert.Equal(0, user.Points); // Should not go below 0
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserStatistics_ReturnsAccurateData()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1, Points = 1500 };
            var bets = new List<Bet>
            {
                new Bet(1, 1, 1, 100, Domain.Enums.BetType.RaceWinner, 2.5m) { Id = 1, Status = Domain.Enums.BetStatus.Won, PotentialWinnings = 150 },
                new Bet(1, 2, 2, 50, Domain.Enums.BetType.PodiumFinish, 1.8m) { Id = 2, Status = Domain.Enums.BetStatus.Lost, PotentialWinnings = 45 },
                new Bet(1, 3, 3, 75, Domain.Enums.BetType.Top10Finish, 1.5m) { Id = 3, Status = Domain.Enums.BetStatus.Won, PotentialWinnings = 56.25m }
            };

            var otherUsers = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 2, Points = 2000 },
                new User("user2", "user2@example.com", "password2") { Id = 3, Points = 1000 }
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(otherUsers.Concat(new[] { user }).AsQueryable()));

            _betRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(bets.AsQueryable()));

            // Act
            var statistics = await _userService.GetUserStatisticsAsync(1);

            // Assert
            Assert.NotNull(statistics);
            Assert.Equal(1, statistics.UserId);
            Assert.Equal("testuser", statistics.Username);
            Assert.Equal(3, statistics.TotalBets);
            Assert.Equal(2, statistics.WinningBets);
            Assert.Equal(66.67m, Math.Round(statistics.WinRate, 2)); // 2/3 * 100 = 66.67%
            Assert.Equal(206.25m, statistics.TotalWinnings); // 150 + 56.25
            Assert.Equal(1500, statistics.Points);
            Assert.Equal(2, statistics.Rank); // user1 has 2000 points (rank 1), testuser has 1500 (rank 2)
        }

        [Fact]
        public async Task GetUserByEmail_ReturnsCorrectUser()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "password1") { Id = 1 },
                new User("user2", "user2@example.com", "password2") { Id = 2 },
                new User("user3", "test@example.com", "password3") { Id = 3, Points = 1000 }
            };

            _userRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(users.AsQueryable()));

            // Act
            var result = await _userService.GetUserByUsernameAsync("user2");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("user2", result.Username);
            Assert.Equal("user2@example.com", result.Email);
            Assert.Equal(10000, result.Points); // Default points for new user
        }
    }
}