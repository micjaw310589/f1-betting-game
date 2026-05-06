using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client : IOpenF1ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenF1Settings _settings;

        public OpenF1Client(IHttpClientFactory httpClientFactory, IOptions<OpenF1Settings> options)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient("OpenF1");
        }

        public async Task<IEnumerable<OpenF1Race>> GetRacesAsync()
        {
            var response = await _httpClient.GetAsync("races");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var races = JsonSerializer.Deserialize<List<OpenF1RaceResponse>>(json);
            
            if (races == null) return [];

            return races.Select(r => new OpenF1Race
            {
                Id = r.race_id?.ToString() ?? string.Empty,
                Name = r.name ?? r.circuit_name ?? "Unknown Race",
                Date = r.date_utc ?? System.DateTime.UtcNow,
                Circuit = r.circuit_name ?? "Unknown Circuit",
                Country = r.country_name ?? "Unknown",
                Season = r.year ?? System.DateTime.UtcNow.Year
            });
        }

        public async Task<OpenF1Race?> GetRaceByIdAsync(string raceId)
        {
            var response = await _httpClient.GetAsync($"races?race_id={raceId}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var races = JsonSerializer.Deserialize<List<OpenF1RaceResponse>>(json);
            return races?.Select(r => new OpenF1Race
            {
                Id = r.race_id?.ToString() ?? string.Empty,
                Name = r.name ?? r.circuit_name ?? "Unknown Race",
                Date = r.date_utc ?? System.DateTime.UtcNow,
                Circuit = r.circuit_name ?? "Unknown Circuit",
                Country = r.country_name ?? "Unknown",
                Season = r.year ?? System.DateTime.UtcNow.Year
            }).FirstOrDefault();
        }

        public async Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId)
        {
            var response = await _httpClient.GetAsync($"drivers?race_id={raceId}");
            if (!response.IsSuccessStatusCode) return [];

            var json = await response.Content.ReadAsStringAsync();
            var sessions = JsonSerializer.Deserialize<List<OpenF1DriverSessionResponse>>(json);
            return sessions?.Select(s => new OpenF1DriverSessionData
            {
                RaceId = s.race_id ?? 0,
                DriverId = s.driver_id ?? 0,
                DriverName = s.driver_name ?? "Unknown Driver",
                TeamName = s.team_name ?? "Unknown Team",
                Date = s.date ?? System.DateTime.UtcNow
            }) ?? [];
        }

        public async Task<OpenF1Race?> GetLatestRaceAsync()
        {
            var races = await GetRacesAsync();
            return races.OrderByDescending(r => r.Date).FirstOrDefault();
        }

        // Internal settings class
        public class OpenF1Settings
        {
            public string BaseUrl { get; set; } = "https://api.openf1.org";
            public int TimeoutSeconds { get; set; } = 30;
            public int RetryCount { get; set; } = 3;
            public int RetryDelaySeconds { get; set; } = 5;
        }

        // Internal DTO for deserializing OpenF1 API responses
        private class OpenF1RaceResponse
        {
            public int? race_id { get; set; }
            public string? name { get; set; }
            public string? circuit_name { get; set; }
            public string? country_name { get; set; }
            public System.DateTime? date_utc { get; set; }
            public int? year { get; set; }
        }

        private class OpenF1DriverSessionResponse
        {
            public int? race_id { get; set; }
            public int? driver_id { get; set; }
            public string? driver_name { get; set; }
            public string? team_name { get; set; }
            public System.DateTime? date { get; set; }
        }
    }
}