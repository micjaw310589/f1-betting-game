using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using F1BettingApp.Infrastructure.OpenF1;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace F1BettingApp.Tests
{
    /// <summary>
    /// Integration tests for OpenF1 API client against the real API.
    /// Run with: dotnet test F1BettingApp/F1BettingApp.Tests/F1BettingApp.Tests.csproj --filter "FullyQualifiedName~OpenF1ClientIntegrationTests"
    /// These tests verify the OpenF1 API is accessible and returns valid data.
    /// </summary>
    public class OpenF1ClientIntegrationTests
    {
        private readonly HttpClient _httpClient;
        private readonly OpenF1Client _client;
        private readonly ITestOutputHelper _output;

        public OpenF1ClientIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.openf1.org/v1") };
            
            var options = Options.Create(new OpenF1Client.OpenF1Settings 
            { 
                BaseUrl = "https://api.openf1.org/v1",
                TimeoutSeconds = 30,
                RetryCount = 3,
                RetryDelaySeconds = 5
            });
            _client = new OpenF1Client(null, options);
        }

        [Fact]
        public async Task GetRacesAsync_ShouldReturnRaces()
        {
            // Act
            var races = await _client.GetRacesAsync();

            // Assert
            Assert.NotNull(races);
            var raceList = races.ToList();
            Assert.NotEmpty(raceList);
            
            _output.WriteLine($"Fetched {raceList.Count} races from OpenF1 API");
            foreach (var race in raceList.Take(5))
            {
                _output.WriteLine($"  - {race.Name} on {race.Date:yyyy-MM-dd} at {race.Circuit} ({race.Country})");
            }
        }

        [Fact]
        public async Task GetRaceByIdAsync_ShouldReturnRace()
        {
            // Arrange - get a race ID first
            var races = await _client.GetRacesAsync();
            var raceList = races.ToList();
            var sampleRace = raceList.FirstOrDefault();
            
            if (sampleRace == null)
            {
                _output.WriteLine("No races found for testing");
                return;
            }

            // Act
            var foundRace = await _client.GetRaceByIdAsync(sampleRace.Id);

            // Assert
            Assert.NotNull(foundRace);
            _output.WriteLine($"Found race: {foundRace.Name} at {foundRace.Circuit}");
        }

        [Fact]
        public async Task GetDriversAsync_ShouldReturnDrivers()
        {
            // Arrange - get a race ID first
            var races = await _client.GetRacesAsync();
            var raceList = races.ToList();
            var sampleRace = raceList.FirstOrDefault();
            
            if (sampleRace == null)
            {
                _output.WriteLine("No races found for testing");
                return;
            }

            // Act
            var drivers = await _client.GetDriversAsync(sampleRace.Id);
            var driverList = drivers.ToList();

            // Assert
            _output.WriteLine($"Fetched {driverList.Count} driver session entries for race {sampleRace.Name}");
            foreach (var driver in driverList.Take(5))
            {
                _output.WriteLine($"  - {driver.DriverName} ({driver.TeamName})");
            }
        }

        [Fact]
        public async Task GetLatestRaceAsync_ShouldReturnLatestRace()
        {
            // Act
            var latestRace = await _client.GetLatestRaceAsync();

            // Assert
            Assert.NotNull(latestRace);
            _output.WriteLine($"Latest race: {latestRace.Name} on {latestRace.Date:yyyy-MM-dd} at {latestRace.Circuit}");
        }

        [Fact]
        public async Task RawApiCall_ShouldReturnValidJson()
        {
            // Act - test the raw API endpoint directly
            var response = await _httpClient.GetAsync("races?season=2024");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            
            // Assert
            Assert.NotEmpty(json);
            Assert.Contains("[", json);
            Assert.Contains("]", json);
            
            // Verify we can deserialize
            var races = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>(json);
            Assert.NotNull(races);
            Assert.NotEmpty(races);
            
            _output.WriteLine($"Raw API returned {races.Count} race entries for 2024 season");
        }
    }
}