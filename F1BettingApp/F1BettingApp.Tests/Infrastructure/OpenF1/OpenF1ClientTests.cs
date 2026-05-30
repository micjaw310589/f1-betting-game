using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using F1BettingApp.Infrastructure.OpenF1;
using Microsoft.Extensions.Options;
using Xunit;

namespace F1BettingApp.Tests.Infrastructure.OpenF1
{
    public class OpenF1ClientTests
    {
        private class TestOpenF1Client : OpenF1Client
        {
            private readonly JsonElement _response;

            public TestOpenF1Client(OpenF1Settings settings, JsonElement response)
                : base(Options.Create(settings))
            {
                _response = response;
            }

            protected override Task<JsonElement> RunCliAndGetJsonAsync(string endpoint, string? param)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task GetRacesAsync_ParsesArrayResponse()
        {
            var json = JsonDocument.Parse(@"[
                { ""race_id"": 1, ""name"": ""Test GP"", ""date_utc"": ""2025-06-01T12:00:00Z"", ""circuit_name"": ""Test Circuit"", ""country_name"": ""Testland"", ""year"": 2025 }
            ]").RootElement;

            var client = new TestOpenF1Client(new OpenF1Settings(), json);
            var races = (await client.GetRacesAsync()).ToList();

            Assert.Single(races);
            var race = races[0];
            Assert.Equal("1", race.Id);
            Assert.Equal("Test GP", race.Name);
            Assert.Equal("Test Circuit", race.Circuit);
            Assert.Equal("Testland", race.Country);
            Assert.Equal(2025, race.Season);
        }

        [Fact]
        public async Task GetRaceByIdAsync_ReturnsFirstMatch()
        {
            var json = JsonDocument.Parse(@"{ ""races"": [
                { ""race_id"": 7, ""name"": ""Seven GP"", ""date_utc"": ""2024-07-07T10:00:00Z"", ""year"": 2024 }
            ]}").RootElement;

            var client = new TestOpenF1Client(new OpenF1Settings(), json);
            var race = await client.GetRaceByIdAsync("7");

            Assert.NotNull(race);
            Assert.Equal("7", race!.Id);
            Assert.Equal("Seven GP", race.Name);
        }

        [Fact]
        public async Task GetDriversAsync_ParsesDriverSessions()
        {
            var json = JsonDocument.Parse(@"[
                { ""race_id"": 2, ""driver_id"": 42, ""driver_name"": ""Jane Doe"", ""team_name"": ""Fast Team"", ""date"": ""2025-08-08T09:00:00Z"" }
            ]").RootElement;

            var client = new TestOpenF1Client(new OpenF1Settings(), json);
            var sessions = (await client.GetDriversAsync("2")).ToList();

            Assert.Single(sessions);
            var s = sessions[0];
            Assert.Equal(2, s.RaceId);
            Assert.Equal(42, s.DriverId);
            Assert.Equal("Jane Doe", s.DriverName);
            Assert.Equal("Fast Team", s.TeamName);
        }
    }
}