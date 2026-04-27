using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using F1BettingApp.Tests.Builders;
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
        private readonly Mock<IBetRepositoryExtensions> _mockBetRepository;
        private readonly Mock<IRepository<User>> _mockUserRepository;
        private readonly Mock<IRaceRepositoryExtensions> _mockRaceRepository;
        private readonly Mock<IRepository<Driver>> _mockDriverRepository;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IRaceService> _mockRaceService;
        private readonly BettingService _bettingService;

        public BettingServiceTests()
        {
            _mockBetRepository = new Mock<IBetRepositoryExtensions>();
            _mockUserRepository = new Mock<IRepository<User>>();
            _mockRaceRepository = new Mock<IRaceRepositoryExtensions>();
            _mockDriverRepository = new Mock<IRepository<Driver>>();
            _mockUserService = new Mock<IUserService>();
            _mockRaceService = new Mock<IRaceService>();

            _bettingService = new BettingService(
                _mockBetRepository.Object,
                _mockUserRepository.Object,
                _mockRaceRepository.Object,
                _mockDriverRepository.Object,
                _mockUserService.Object,
                _mockRaceService.Object);
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

        // New required test cases using builders for better readability

        [Fact]
        public async Task PlaceBetAsync_AfterRaceStart_ShouldFail()
        {
            // Arrange
            var user = new UserBuilder()
                .WithId(1)
                .WithPoints(1000)
                .Build();

            var race = new RaceBuilder()
                .WithId(1)
                .AsInProgress() // Race already started
                .Build();

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.PlaceBetAsync(1, 1, 1, 100));
        }

        [Fact]
        public async Task CancelBetAsync_AfterRaceStart_ShouldFail()
        {
            // Arrange
            var bet = new BetBuilder()
                .WithId(1)
                .WithUserId(1)
                .WithRaceId(1)
                .AsWon() // Bet already processed
                .Build();

            var user = new UserBuilder()
                .WithId(1)
                .WithPoints(500)
                .Build();

            _mockBetRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(bet);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.CancelBetAsync(1));
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_WithWinningBets_UpdatesPoints()
        {
            // Arrange
            var raceId = 1;
            var race = new RaceBuilder()
                .WithId(raceId)
                .AsFinished()
                .Build();

            var winningBet = new BetBuilder()
                .WithId(1)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(1)
                .WithBetType(BetType.RaceWinner)
                .WithAmount(100)
                .WithOdds(2.5m)
                .AsPending()
                .Build();

            var losingBet = new BetBuilder()
                .WithId(2)
                .WithUserId(2)
                .WithRaceId(raceId)
                .WithDriverId(2)
                .WithBetType(BetType.RaceWinner)
                .WithAmount(50)
                .WithOdds(3.0m)
                .AsPending()
                .Build();

            var bets = new List<Bet> { winningBet, losingBet };

            var results = new ResultBuilder()
                .BuildRaceResults()
                .Where(r => r.RaceId == raceId)
                .ToList();

            // Ensure driver 1 wins (position 1)
            results.Add(new ResultBuilder()
                .WithRaceId(raceId)
                .WithDriverId(1)
                .AsPodiumFinish(1)
                .Build());

            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(raceId)).ReturnsAsync(race);
            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());
            _mockResultRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(results.AsQueryable());
            _mockBetRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.ProcessRaceResultsAsync(raceId);

            // Assert
            _mockBetRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Bet>()), Times.Exactly(2));
            Assert.Equal(BetStatus.Won, winningBet.Status);
            Assert.Equal(BetStatus.Lost, losingBet.Status);
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_WithPartialWins_UpdatesPoints()
        {
            // Arrange
            var raceId = 1;
            var race = new RaceBuilder()
                .WithId(raceId)
                .AsFinished()
                .Build();

            // Create bets with different types
            var raceWinnerBet = new BetBuilder()
                .WithId(1)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(1)
                .WithBetType(BetType.RaceWinner)
                .AsPending()
                .Build();

            var podiumFinishBet = new BetBuilder()
                .WithId(2)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(2)
                .WithBetType(BetType.PodiumFinish)
                .AsPending()
                .Build();

            var top10FinishBet = new BetBuilder()
                .WithId(3)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(3)
                .WithBetType(BetType.Top10Finish)
                .AsPending()
                .Build();

            var losingBet = new BetBuilder()
                .WithId(4)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(4)
                .WithBetType(BetType.RaceWinner)
                .AsPending()
                .Build();

            var bets = new List<Bet> { raceWinnerBet, podiumFinishBet, top10FinishBet, losingBet };

            // Driver 1 wins, Driver 2 gets position 3 (podium), Driver 3 gets position 8 (top 10), Driver 4 gets position 11 (no points)
            var results = new List<Result>
            {
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(1)
                    .AsPodiumFinish(1)
                    .Build(),
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(2)
                    .AsPodiumFinish(3)
                    .Build(),
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(3)
                    .WithPosition(8)
                    .WithPoints(4)
                    .Build(),
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(4)
                    .WithPosition(11)
                    .WithPoints(0)
                    .Build()
            };

            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(raceId)).ReturnsAsync(race);
            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());
            _mockResultRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(results.AsQueryable());
            _mockBetRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.ProcessRaceResultsAsync(raceId);

            // Assert
            _mockBetRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Bet>()), Times.Exactly(4));
            Assert.Equal(BetStatus.Won, raceWinnerBet.Status);      // Driver 1 won
            Assert.Equal(BetStatus.Won, podiumFinishBet.Status);    // Driver 2 got podium
            Assert.Equal(BetStatus.Won, top10FinishBet.Status);     // Driver 3 got top 10
            Assert.Equal(BetStatus.Lost, losingBet.Status);         // Driver 4 didn't finish in points
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_WithLosingBets_NoPointsUpdate()
        {
            // Arrange
            var raceId = 1;
            var race = new RaceBuilder()
                .WithId(raceId)
                .AsFinished()
                .Build();

            var losingBet1 = new BetBuilder()
                .WithId(1)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(1)
                .WithBetType(BetType.RaceWinner)
                .AsPending()
                .Build();

            var losingBet2 = new BetBuilder()
                .WithId(2)
                .WithUserId(1)
                .WithRaceId(raceId)
                .WithDriverId(2)
                .WithBetType(BetType.PodiumFinish)
                .AsPending()
                .Build();

            var bets = new List<Bet> { losingBet1, losingBet2 };

            // Both drivers finish outside points positions
            var results = new List<Result>
            {
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(1)
                    .WithPosition(11)
                    .WithPoints(0)
                    .Build(),
                new ResultBuilder()
                    .WithRaceId(raceId)
                    .WithDriverId(2)
                    .WithPosition(12)
                    .WithPoints(0)
                    .Build()
            };

            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(raceId)).ReturnsAsync(race);
            _mockBetRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bets.AsQueryable());
            _mockResultRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(results.AsQueryable());
            _mockBetRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);

            // Act
            await _bettingService.ProcessRaceResultsAsync(raceId);

            // Assert
            _mockBetRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Bet>()), Times.Exactly(2));
            Assert.Equal(BetStatus.Lost, losingBet1.Status);
            Assert.Equal(BetStatus.Lost, losingBet2.Status);
        }

        [Fact]
        public async Task PlaceBetAsync_WithDifferentBetTypes_ShouldSucceed()
        {
            // Test all major bet types
            var betTypes = new List<BetType>
            {
                BetType.RaceWinner,
                BetType.PodiumFinish,
                BetType.Top10Finish,
                BetType.FastestLap,
                BetType.FastestPitStop,
                BetType.DriverVsDriver,
                BetType.TeamVsTeam
            };

            foreach (var betType in betTypes)
            {
                // Arrange
                var user = new UserBuilder()
                    .WithId(1)
                    .WithPoints(1000)
                    .Build();

                var race = new RaceBuilder()
                    .WithId(1)
                    .BuildUpcomingRace();

                _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
                _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);
                _mockBetRepository.Setup(repo => repo.AddAsync(It.IsAny<Bet>())).Returns(Task.CompletedTask);
                _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

                // Act
                await _bettingService.PlaceBetAsync(1, 1, 1, 100);

                // Assert
                _mockBetRepository.Verify(repo => repo.AddAsync(It.IsAny<Bet>()), Times.Once);
                _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);

                // Reset for next iteration
                _mockBetRepository.Reset();
                _mockUserRepository.Reset();
            }
        }

        [Fact]
        public async Task PlaceBetAsync_WithZeroAmount_ShouldFail()
        {
            // Arrange
            var user = new UserBuilder()
                .WithId(1)
                .WithPoints(1000)
                .Build();

            var race = new RaceBuilder()
                .WithId(1)
                .BuildUpcomingRace();

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _bettingService.PlaceBetAsync(1, 1, 1, 0));
        }

        [Fact]
        public async Task PlaceBetAsync_WithNegativeAmount_ShouldFail()
        {
            // Arrange
            var user = new UserBuilder()
                .WithId(1)
                .WithPoints(1000)
                .Build();

            var race = new RaceBuilder()
                .WithId(1)
                .BuildUpcomingRace();

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _bettingService.PlaceBetAsync(1, 1, 1, -50));
        }

        [Fact]
        public async Task CancelBetAsync_NonExistentBet_ShouldFail()
        {
            // Arrange
            _mockBetRepository.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Bet)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.CancelBetAsync(999));
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_NonExistentRace_ShouldFail()
        {
            // Arrange
            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Race)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.ProcessRaceResultsAsync(999));
        }

        [Fact]
        public async Task ProcessRaceResultsAsync_RaceNotFinished_ShouldFail()
        {
            // Arrange
            var race = new RaceBuilder()
                .WithId(1)
                .WithStatus(RaceStatus.Scheduled) // Not finished
                .Build();

            _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _bettingService.ProcessRaceResultsAsync(1));
        }
    }
}
