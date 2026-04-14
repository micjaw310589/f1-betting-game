using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using Xunit;

namespace F1BettingApp.Tests
{
    public class RaceServiceTests
    {
        private readonly Mock<IRepository<Race>> _raceRepositoryMock;
        private readonly RaceService _raceService;

        public RaceServiceTests()
        {
            _raceRepositoryMock = new Mock<IRepository<Race>>();
            _raceService = new RaceService(_raceRepositoryMock.Object);
        }

        [Fact]
        public async Task GetRaceByIdAsync_ReturnsRaceDto_WhenRaceExists()
        {
            // Arrange
            var race = new Race { Id = 1, Name = "Monaco GP", Date = DateTime.UtcNow, Status = RaceStatus.Scheduled };
            _raceRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(race);

            // Act
            var result = await _raceService.GetRaceByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Monaco GP", result.Name);
        }

        [Fact]
        public async Task GetAllRacesAsync_ReturnsAllRaces()
        {
            // Arrange
            var races = new List<Race>
            {
                new Race { Id = 1, Name = "Race1", Date = DateTime.UtcNow, Status = RaceStatus.Scheduled },
                new Race { Id = 2, Name = "Race2", Date = DateTime.UtcNow, Status = RaceStatus.Finished }
            };
            _raceRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(races);

            // Act
            var result = await _raceService.GetAllRacesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUpcomingRacesAsync_ReturnsOnlyUpcomingRaces()
        {
            // Arrange
            var races = new List<Race>
            {
                new Race { Id = 1, Name = "Race1", Date = DateTime.UtcNow, Status = RaceStatus.Scheduled },
                new Race { Id = 2, Name = "Race2", Date = DateTime.UtcNow, Status = RaceStatus.Finished }
            };
            _raceRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(races);

            // Act
            var result = await _raceService.GetUpcomingRacesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Race1", result.First().Name);
        }
    }
}