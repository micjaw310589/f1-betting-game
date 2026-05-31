//cd F1BettingApp/F1BettingApp.Tests
//dotnet test --filter "F1BettingApp.Tests.UserServiceTests"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Moq;
using Xunit;

namespace F1BettingApp.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<Bet>> _betRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IRepository<UserBetStatisticsCache>> _cacheRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IRepository<User>>();
            _betRepositoryMock = new Mock<IRepository<Bet>>();
            _cacheRepositoryMock = new Mock<IRepository<UserBetStatisticsCache>>();
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Jwt:SecretKey", "test-secret-key-test-secret-key-test-secret-key"},
                    {"Jwt:Issuer", "test-issuer"},
                    {"Jwt:Audience", "test-audience"}
                })
                .Build();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _betRepositoryMock.Object,
                _cacheRepositoryMock.Object,
                configuration);
        }

        [Fact]
        public async Task GetEnhancedUserStatisticsAsync_ShouldCalculateStats()
        {
            // Arrange
            var userId = 1;
            var bets = new List<Bet>
            {
                new Bet { Id = 1, UserId = userId, Amount = 100, Status = BetStatus.Won, Winnings = 50, DriverId = 1, CreatedAt = DateTime.Now.AddDays(-10) },
                new Bet { Id = 2, UserId = userId, Amount = 50, Status = BetStatus.Lost, Winnings = 0, DriverId = 2, CreatedAt = DateTime.Now.AddDays(-5) },
                new Bet { Id = 3, UserId = userId, Amount = 75, Status = BetStatus.Won, Winnings = 50, DriverId = 1, CreatedAt = DateTime.Now.AddDays(-1) },
                new Bet { Id = 4, UserId = userId, Amount = 25, Status = BetStatus.Push, Winnings = 0, DriverId = 3, CreatedAt = DateTime.Now.AddDays(-2) }
            };

            _betRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(bets.AsQueryable());
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("testuser", "test@example.com", "passwordhash") { Id = userId });

            // Act
            var result = await _userService.GetEnhancedUserStatisticsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("testuser", result.Username);
            Assert.Equal(4, result.TotalBets);
            Assert.Equal(2, result.WinningBets);
            Assert.Equal(1, result.LosingBets);
            Assert.Equal(1, result.PushBets);
            Assert.Equal(50, result.WinRate);
            Assert.Equal(100, result.TotalWinnings); // 50 + 50 = 100
            Assert.Equal(250, result.TotalAmountBet); // 100 + 50 + 75 + 25 = 250
            Assert.Equal(1, result.FavoriteDriverId);
            Assert.Equal("Driver 1", result.FavoriteDriverName);
        }

        [Fact]
        public async Task GetUserStatisticsAsync_ShouldCalculateStats()
        {
            // Arrange
            var userId = 1;
            var bets = new List<Bet>
            {
                new Bet { Id = 1, UserId = userId, Amount = 100, Status = BetStatus.Won, Winnings = 50, DriverId = 1, CreatedAt = DateTime.Now.AddDays(-10) },
                new Bet { Id = 2, UserId = userId, Amount = 50, Status = BetStatus.Lost, Winnings = 0, DriverId = 2, CreatedAt = DateTime.Now.AddDays(-5) },
                new Bet { Id = 3, UserId = userId, Amount = 75, Status = BetStatus.Won, Winnings = 50, DriverId = 1, CreatedAt = DateTime.Now.AddDays(-1) },
                new Bet { Id = 4, UserId = userId, Amount = 25, Status = BetStatus.Push, Winnings = 0, DriverId = 3, CreatedAt = DateTime.Now.AddDays(-2) }
            };

            _betRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(bets.AsQueryable());
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("testuser", "test@example.com", "passwordhash") { Id = userId });

            // Act
            var result = await _userService.GetUserStatisticsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("testuser", result.Username);
            Assert.Equal(4, result.TotalBets);
            Assert.Equal(2, result.WinningBets);
            Assert.Equal(50, result.WinRate);
            Assert.Equal(100, result.TotalWinnings); // 50 + 50 = 100
        }

        [Fact]
        public async Task GetUserBetHistoryAsync_ShouldReturnPaginatedResults()
        {
            // Arrange
            var userId = 1;
            var allBets = Enumerable.Range(1, 50).Select(i => new Bet
            {
                Id = i,
                UserId = userId,
                Amount = 10,
                Status = BetStatus.Won,
                Winnings = 5,
                DriverId = 1,
                CreatedAt = DateTime.Now.AddDays(-i)
            }).ToList();

            _betRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(allBets.AsQueryable());

            // Act
            var result = await _userService.GetUserBetHistoryAsync(userId, 1, 20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Bets.Count());
        }

        [Fact]
        public async Task GetUserStatisticsByTimeRangeAsync_ShouldFilterByDateRange()
        {
            // Arrange
            var userId = 1;
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;
            var bets = new List<Bet>
            {
                new Bet { Id = 1, UserId = userId, Amount = 100, Status = BetStatus.Won, Winnings = 50, CreatedAt = DateTime.Now.AddDays(-40) },
                new Bet { Id = 2, UserId = userId, Amount = 50, Status = BetStatus.Won, Winnings = 25, CreatedAt = DateTime.Now.AddDays(-20) },
                new Bet { Id = 3, UserId = userId, Amount = 75, Status = BetStatus.Won, Winnings = 50, CreatedAt = DateTime.Now.AddDays(-10) }
            };

            _betRepositoryMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(bets.AsQueryable());
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User("testuser", "test@example.com", "passwordhash") { Id = userId });

            // Act
            var result = await _userService.GetUserStatisticsByTimeRangeAsync(userId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalBets); // Only bets 2 and 3 are in range
            Assert.Equal(75, result.TotalWinnings); // 25 + 50 = 75
        }
    }
}