using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Xunit;
using System;
using System.Collections.Generic;

namespace F1BettingApp.Tests
{
    public class RaceTests
    {
        [Fact]
        public void Constructor_ValidInputs_CreatesRaceObject()
        {
            // Arrange
            var date = new DateTime(2024, 10, 20);
            var race = new Race("Singapore Grand Prix", date, "Marina Bay Street Circuit", "Singapore", "SGP", 2024);

            // Act
            // Assert
            Assert.Equal("Singapore Grand Prix", race.Name);
            Assert.Equal("Marina Bay Street Circuit", race.Circuit);
            Assert.Equal(2024, race.Season);
            Assert.Equal(RaceStatus.Scheduled, race.Status);
            Assert.NotNull(race.Bets);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
        {
            // Act & Assert
            var ex = Record.Exception(() => new Race(invalidName, DateTime.Now, "Circuit", "Country", "OpenF1RaceId", 2024));
            Assert.IsType<ArgumentException>(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_InvalidCircuit_ThrowsArgumentException(string invalidCircuit)
        {
            // Act & Assert
            var ex = Record.Exception(() => new Race("Name", DateTime.Now, invalidCircuit, "Country", "OpenF1RaceId", 2024));
            Assert.IsType<ArgumentException>(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_InvalidCountry_ThrowsArgumentException(string invalidCountry)
        {
            // Act & Assert
            var ex = Record.Exception(() => new Race("Name", DateTime.Now, "Circuit", invalidCountry, "OpenF1RaceId", 2024));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void Constructor_InvalidSeason_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Record.Exception(() => new Race("Name", DateTime.Now, "Circuit", "Country", "OpenF1RaceId", 0));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void CanPlaceBets_WhenScheduled_ReturnsTrue()
        {
            // Arrange
            var race = new Race("Name", DateTime.Now, "Circuit", "Country", "OpenF1RaceId", 2024);

            // Act & Assert
            Assert.True(race.CanPlaceBets());
        }

        [Fact]
        public void CanPlaceBets_WhenFinished_ReturnsFalse()
        {
            // Arrange
            var race = new Race("Name", DateTime.Now, "Circuit", "Country", "OpenF1RaceId", 2024);
            // Manually setting status for testing
            typeof(Race).GetProperty("Status").SetValue(race, RaceStatus.Finished);

            // Act & Assert
            Assert.False(race.CanPlaceBets());
        }

        [Fact]
        public void IsRaceFinished_WhenFinished_ReturnsTrue()
        {
            // Arrange
            var race = new Race("Name", DateTime.Now, "Circuit", "Country", "OpenF1RaceId", 2024);
            // Manually setting status for testing
            typeof(Race).GetProperty("Status").SetValue(race, RaceStatus.Finished);

            // Act & Assert
            Assert.True(race.IsRaceFinished());
        }
    }
}