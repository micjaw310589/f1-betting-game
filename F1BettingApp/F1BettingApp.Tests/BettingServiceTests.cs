using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;

namespace F1BettingApp.Tests
{
    public class BettingServiceTests
    {
        private readonly Mock<IRepository<Bet>> _betRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly BettingService _bettingService;

        public BettingServiceTests()
        {
            _betRepositoryMock = new Mock<IRepository<Bet>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _bettingService = new BettingService(_betRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task PlaceBetAsync_AddsBet_WhenUserHasBalance()
        {
            // Arrange
            var user = new User { Id = 1, Points = 100 };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            var bets = new List<Bet>();
            _betRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Bet>())).Callback<Bet>(b => bets.Add(b));

            // Act
            await _bettingService.PlaceBetAsync(1, 1, 1, 50);

            // Assert
            Assert.Single(bets);
            Assert.Equal(1, bets[0].UserId);
            Assert.Equal(50, bets[0].Amount);
        }

        [Fact]
        public async Task PlaceBetAsync_ThrowsException_WhenInsufficientBalance()
        {
            // Arrange
            var user = new User { Id = 1, Points = 10 };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _bettingService.PlaceBetAsync(1, 1, 1, 50));
        }

        [Fact]
        public async Task GetUserBetsAsync_ReturnsUserBets()
        {
            // Arrange
            var bets = new List<Bet>
            {
                new Bet { Id = 1, UserId = 1, RaceId = 1, DriverId = 1, Amount = 50, Status = BetStatus.Pending, CreatedAt = DateTime.UtcNow },
                new Bet { Id = 2, UserId = 2, RaceId = 1, DriverId = 2, Amount = 30, Status = BetStatus.Pending, CreatedAt = DateTime.UtcNow }
            };
            _betRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(bets);

            // Act
            var result = await _bettingService.GetUserBetsAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }
    }
}