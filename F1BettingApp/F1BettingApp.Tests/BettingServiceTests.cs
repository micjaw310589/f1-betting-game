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
    public class BettingServiceTests
    {
        private readonly Mock<IRepository<Bet>> _mockBetRepository;
        private readonly Mock<IRepository<User>> _mockUserRepository;
        private readonly Mock<IRepository<Race>> _mockRaceRepository;
        private readonly Mock<IRepository<Result>> _mockResultRepository;
        private readonly BettingService _bettingService;

        public BettingServiceTests()
        {
            _mockBetRepository = new Mock<IRepository<Bet>>();
            _mockUserRepository = new Mock<IRepository<User>>();
            _mockRaceRepository = new Mock<IRepository<Race>>();
            _mockResultRepository = new Mock<IRepository<Result>>();

            _bettingService = new BettingService(
                _mockBetRepository.Object,
                _mockUserRepository.Object,
                _mockRaceRepository.Object,
                _mockResultRepository.Object);
        }

        [Fact]
        public async Task PlaceBetAsync_ValidBet_ShouldCreateBetAndDeductPoints()
        {
            // Arrange
            var userId = 1;
            var raceId = 1;
            var driverId = 1;
            var amount = 100;

            var user = new User("testuser", "test@example.com", "password");
            user.Points = 1000;

            var race = new Race("Test Race", DateTime.UtcNow.AddDays(7), "Test Circuit", "Test Country", "race1", 2023);

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(raceId)).ReturnsAsync(race);
            _mockBetRepository.Setup(repo => repo.AddAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.PlaceBetAsync(userId, raceId, driverId, amount);

            // Assert
            _mockBetRepository.Verify(repo => repo.AddAsync(It.IsAny<Bet>()), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
            Assert.Equal(900, user.Points); // 1000 - 100
        }

        [Fact]
        public async Task PlaceBetAsync_InsufficientBalance_ShouldThrowException()
        {
            // Arrange
            var userId = 1;
            var raceId = 1;
            var driverId = 1;
            var amount = 1000;

            var user = new User("testuser", "test@example.com", "password");
            user.Points = 100; // Not enough for 1000 bet

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.PlaceBetAsync(userId, raceId, driverId, amount));
        }

        [Fact]
        public async Task CancelBetAsync_ValidBet_ShouldCancelBetAndRefundPoints()
        {
            // Arrange
            var betId = 1;
            var userId = 1;
            var amount = 100;

            var bet = new Bet(userId, 1, 1, amount, BetType.RaceWinner, 2.5m);
            bet.Status = BetStatus.Pending;

            var user = new User("testuser", "test@example.com", "password");
            user.Points = 500;

            _mockBetRepository.Setup(repo => repo.GetByIdAsync(betId)).ReturnsAsync(bet);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockBetRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.CancelBetAsync(betId);

            // Assert
            _mockBetRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Bet>()), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
            Assert.Equal(BetStatus.Canceled, bet.Status);
            Assert.Equal(600, user.Points); // 500 + 100 refund
        }

        [Fact]
        public async Task GetUserBetsAsync_ExistingBets_ShouldReturnBetDtos()
        {
            // Arrange
            var userId = 1;
            var bets = new List<Bet>
            {
                new Bet(userId, 1, 1, 100, BetType.RaceWinner, 2.5m),
                new Bet(userId, 2, 2, 200, BetType.PodiumFinish, 1.8m)
            };

            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());

            // Act
            var result = await _bettingService.GetUserBetsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, bet => Assert.Equal(userId, bet.UserId));
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_CompletedRace_ShouldUpdateBetStatuses()
        {
            // Arrange
            var raceId = 1;
            var userId = 1;

            var race = new Race("Test Race", DateTime.UtcNow.AddDays(-1), "Test Circuit", "Test Country", "race1", 2023);
            race.Status = RaceStatus.Finished;

            var bets = new List<Bet>
            {
                new Bet(userId, raceId, 1, 100, BetType.RaceWinner, 2.5m) { Status = BetStatus.Pending },
                new Bet(userId, raceId, 2, 200, BetType.PodiumFinish, 1.8m) { Status = BetStatus.Pending }
            };

            var results = new List<Result>
            {
                new Result(raceId, 1, 1, 25, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(20)),
                new Result(raceId, 2, 3, 18, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(22)) // Driver 2 got position 3
            };

            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(raceId)).ReturnsAsync(race);
            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());
            _mockResultRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(results.AsQueryable());
            _mockBetRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.ProcessRaceResultsAsync(raceId);

            // Assert
            _mockBetRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Bet>()), Times.Exactly(2));

            // First bet should win (driver 1 got position 1)
            Assert.Equal(BetStatus.Won, bets[0].Status);

            // Second bet should win (driver 2 got position 3, which is podium)
            Assert.Equal(BetStatus.Won, bets[1].Status);
        }

        [Fact]
        public async Task CalculateWinningsAsync_WinningBets_ShouldReturnTotalWinnings()
        {
            // Arrange
            var userId = 1;
            var raceId = 1;

            var bets = new List<Bet>
            {
                new Bet(userId, raceId, 1, 100, BetType.RaceWinner, 2.5m) { Status = BetStatus.Won, PotentialWinnings = 250 },
                new Bet(userId, raceId, 2, 200, BetType.PodiumFinish, 1.8m) { Status = BetStatus.Won, PotentialWinnings = 360 },
                new Bet(userId, raceId, 3, 50, BetType.RaceWinner, 3.0m) { Status = BetStatus.Lost, PotentialWinnings = 150 }
            };

            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());

            // Act
            var result = await _bettingService.CalculateWinningsAsync(userId, raceId);

            // Assert
            Assert.Equal(610, result); // 250 + 360
        }
    }
}