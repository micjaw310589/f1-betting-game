using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.OpenF1;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    public class RaceServiceTests
    {
        private readonly Mock<IRepository<Race>> _raceRepositoryMock;
        private readonly Mock<IRepository<Result>> _resultRepositoryMock;
        private readonly Mock<IOpenF1ApiClient> _openF1ApiClientMock;
        private readonly RaceService _raceService;

        public RaceServiceTests()
        {
            _raceRepositoryMock = new Mock<IRepository<Race>>();
            _resultRepositoryMock = new Mock<IRepository<Result>>();
            _openF1ApiClientMock = new Mock<IOpenF1ApiClient>();
            _raceService = new RaceService(
                _raceRepositoryMock.Object,
                _resultRepositoryMock.Object,
                _openF1ApiClientMock.Object);
        }

        [Fact]
        public async Task SyncRaceData_FromOpenF1_Succeeds()
        {
            // Arrange
            var openF1Races = new List<OpenF1Race>
            {
                new OpenF1Race
                {
                    Id = "1",
                    Name = "Australian Grand Prix",
                    Date = DateTime.Now.AddDays(7),
                    Circuit = "Melbourne Grand Prix Circuit",
                    Country = "Australia",
                    Season = 2023
                },
                new OpenF1Race
                {
                    Id = "2",
                    Name = "Monaco Grand Prix",
                    Date = DateTime.Now.AddDays(30),
                    Circuit = "Circuit de Monaco",
                    Country = "Monaco",
                    Season = 2023
                }
            };

            _openF1ApiClientMock.Setup(x => x.GetRacesAsync())
                .ReturnsAsync(openF1Races);

            _raceRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(new List<Race>().AsQueryable()));

            // Act
            await _raceService.SyncRaceDataFromOpenF1Async();

            // Assert
            _raceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Race>()), Times.Exactly(2));
            _raceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncRaceData_WithApiFailure_UsesCache()
        {
            // Arrange
            var existingRaces = new List<Race>
            {
                new Race("Australian Grand Prix", DateTime.Now.AddDays(7),
                    "Melbourne Grand Prix Circuit", "Australia", "1", 2023),
                new Race("Monaco Grand Prix", DateTime.Now.AddDays(30),
                    "Circuit de Monaco", "Monaco", "2", 2023)
            };

            _openF1ApiClientMock.Setup(x => x.GetRacesAsync())
                .ThrowsAsync(new Exception("API failure"));

            _raceRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(existingRaces.AsQueryable()));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _raceService.SyncRaceDataFromOpenF1Async());

            // Verify no changes were made to repository
            _raceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Race>()), Times.Never);
            _raceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Race>()), Times.Never);
            _raceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateRaceStatus_ToFinished_TriggersProcessing()
        {
            // Arrange
            var race = new Race("Australian Grand Prix", DateTime.Now.AddDays(-1),
                "Melbourne Grand Prix Circuit", "Australia", "1", 2023)
            {
                Id = 1,
                Status = RaceStatus.InProgress
            };

            _raceRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(race);

            // Act
            await _raceService.UpdateRaceStatusAsync(1, "Finished");

            // Assert
            _raceRepositoryMock.Verify(x => x.UpdateAsync(race), Times.Once);
            _raceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            Assert.Equal(RaceStatus.Finished, race.Status);
        }

        [Fact]
        public async Task GetUpcomingRaces_ReturnsOnlyFutureRaces()
        {
            // Arrange
            var allRaces = new List<Race>
            {
                new Race("Australian Grand Prix", DateTime.Now.AddDays(7),
                    "Melbourne Grand Prix Circuit", "Australia", "1", 2023)
                {
                    Id = 1,
                    Status = RaceStatus.Scheduled
                },
                new Race("Monaco Grand Prix", DateTime.Now.AddDays(-1),
                    "Circuit de Monaco", "Monaco", "2", 2023)
                {
                    Id = 2,
                    Status = RaceStatus.Finished
                },
                new Race("British Grand Prix", DateTime.Now.AddDays(30),
                    "Silverstone Circuit", "UK", "3", 2023)
                {
                    Id = 3,
                    Status = RaceStatus.Scheduled
                }
            };

            _raceRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(allRaces.AsQueryable()));

            // Act
            var result = await _raceService.GetUpcomingRacesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, race => Assert.Equal(RaceStatus.Scheduled, race.Status));
            Assert.Contains(result, r => r.Name == "Australian Grand Prix");
            Assert.Contains(result, r => r.Name == "British Grand Prix");
            Assert.DoesNotContain(result, r => r.Name == "Monaco Grand Prix");
        }

        [Fact]
        public async Task GetRaceWithResults_ReturnsCompleteData()
        {
            // Arrange
            var race = new Race("Australian Grand Prix", DateTime.Now.AddDays(-1),
                "Melbourne Grand Prix Circuit", "Australia", "1", 2023)
            {
                Id = 1,
                Status = RaceStatus.Finished
            };

            _raceRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(race);

            // Act
            var result = await _raceService.GetRaceByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Australian Grand Prix", result.Name);
            Assert.Equal("Melbourne Grand Prix Circuit", result.Circuit);
            Assert.Equal("Australia", result.Country);
            Assert.Equal(RaceStatus.Finished, result.Status);
        }
    }
}