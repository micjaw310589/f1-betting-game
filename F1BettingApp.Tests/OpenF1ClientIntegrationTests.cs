using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Infrastructure.OpenF1;
using Microsoft.Extensions.Logging;

namespace F1BettingApp.Tests
{
    /// <summary>
    /// Integration tests for OpenF1 API client against the real API.
    /// Run with: dotnet test --filter "FullyQualifiedName~OpenF1ClientIntegrationTests"
    /// </summary>
    public class OpenF1ClientIntegrationTests
    {
        private readonly OpenF1Client _client;
        private readonly ILogger<OpenF1ClientIntegrationTests> _logger;

        public OpenF1ClientIntegrationTests()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.openf1.org/v1") };
            _client = new OpenF1Client(httpClient);
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<OpenF1ClientIntegrationTests>();
        }

        [Fact]
        public async Task GetRaceCalendarAsync_ShouldReturnRaces()
        {
            // Arrange
            int currentSeason = DateTime.Now.Year;

            // Act
            var races = await _client.GetRaceCalendarAsync(currentSeason);

            // Assert
            Assert.NotNull(races);
            Assert.NotEmpty(races);
            Console.WriteLine($"Fetched {races.Count} races for season {currentSeason}");
            
            foreach (var race in races.Take(5))
            {
                Console.WriteLine($"  - {race.Name} on {race.Date:yyyy-MM-dd} ({race.Status})");
            }
        }

        [Fact]
        public async Task GetStandingsAsync_ShouldReturnStandings()
        {
            // Arrange
            int currentSeason = DateTime.Now.Year;

            // Act
            var standings = await _client.GetStandingsAsync(currentSeason);

            // Assert
            Assert.NotNull(standings);
            Assert.NotEmpty(standings);
            Console.WriteLine($"Fetched {standings.Count} driver standings for season {currentSeason}");
            
            foreach (var standing in standings.Take(5))
            {
                Console.WriteLine($"  {standing.Position}. {standing.Name} - {standing.Points} points");
            }
        }

        [Fact]
        public async Task GetDriverAndTeamInfoAsync_ShouldReturnDriversAndTeams()
        {
            // Arrange
            int currentSeason = DateTime.Now.Year;

            // Act
            var (drivers, teams) = await _client.GetDriverAndTeamInfoAsync(currentSeason);

            // Assert
            Assert.NotNull(drivers);
            Assert.NotNull(teams);
            Assert.NotEmpty(drivers);
            Assert.NotEmpty(teams);
            Console.WriteLine($"Fetched {drivers.Count} drivers and {teams.Count} teams for season {currentSeason}");
            
            foreach (var driver in drivers.Take(5))
            {
                Console.WriteLine($"  Driver: {driver.Name} (Team: {driver.TeamId})");
            }
            
            foreach (var team in teams.Take(5))
            {
                Console.WriteLine($"  Team: {team.Name}");
            }
        }

        [Fact]
        public async Task GetRaceResultsAsync_ShouldReturnResults()
        {
            // Arrange - Get the most recent completed race
            var races = await _client.GetRaceCalendarAsync(DateTime.Now.Year);
            var completedRace = races?.FirstOrDefault(r => r.Status == "Finished");
            
            if (completedRace == null)
            {
                Assert.True(true, "No finished races found for testing");
                return;
            }

            // Act
            var results = await _client.GetRaceResultsAsync(completedRace.RaceId);

            // Assert
            Assert.NotNull(results);
            Console.WriteLine($"Fetched {results.Count} results for race {completedRace.Name}");
            
            foreach (var result in results.Take(5))
            {
                Console.WriteLine($"  Position {result.Position}: Driver {result.DriverId} - {result.Points} points");
            }
        }

        [Fact]
        public async Task GetRaceDetailsAsync_ShouldReturnRaceDetails()
        {
            // Arrange - Get a race
            var races = await _client.GetRaceCalendarAsync(DateTime.Now.Year);
            var race = races?.FirstOrDefault();
            
            if (race == null)
            {
                Assert.True(true, "No races found for testing");
                return;
            }

            // Act
            var raceDetails = await _client.GetRaceDetailsAsync(race.RaceId);

            // Assert
            Assert.NotNull(raceDetails);
            Assert.Equal(race.RaceId, raceDetails.RaceId);
            Console.WriteLine($"Race details: {raceDetails.Name} at {raceDetails.Circuit} on {raceDetails.Date:yyyy-MM-dd}");
        }
    }
}