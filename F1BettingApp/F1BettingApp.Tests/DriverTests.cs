using F1BettingApp.Domain.Entities;
using Xunit;
using System;

namespace F1BettingApp.Tests
{
    public class DriverTests
    {
        [Fact]
        public void Constructor_ValidInputs_CreatesDriverObject()
        {
            // Arrange
            var team = new Team("Red Bull Racing", "Austria", "RBR");
            var driver = new Driver("Max Verstappen", "Netherlands", "VER", team.Id);

            // Act
            // Assert
            Assert.Equal("Max Verstappen", driver.Name);
            Assert.Equal("Netherlands", driver.Country);
            Assert.Equal("VER", driver.OpenF1DriverId);
            Assert.Equal(team.Id, driver.TeamId);
            Assert.Equal(team.Name, driver.Team.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
        {
            // Arrange
            var team = new Team("Team", "Country", "OpenF1TeamId");

            // Act & Assert
            var ex = Record.Exception(() => new Driver(invalidName, "Netherlands", "VER", team.Id));
            Assert.IsType<ArgumentException>(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_InvalidCountry_ThrowsArgumentException(string invalidCountry)
        {
            // Arrange
            var team = new Team("Team", "Country", "OpenF1TeamId");

            // Act & Assert
            var ex = Record.Exception(() => new Driver("Name", invalidCountry, "VER", team.Id));
            Assert.IsType<ArgumentException>(ex);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidTeamId_ThrowsArgumentException(int invalidTeamId)
        {
            // Arrange
            var team = new Team("Team", "Country", "OpenF1TeamId");

            // Act & Assert
            var ex = Record.Exception(() => new Driver("Name", "Country", "VER", invalidTeamId));
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void GetFullName_ReturnsDriverName()
        {
            // Arrange
            var driver = new Driver("Charles Leclerc", "Monaco", "LEC", 1);

            // Act
            string fullName = driver.GetFullName();

            // Assert
            Assert.Equal("Charles Leclerc", fullName);
        }
    }
}
