using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client : IOpenF1ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenF1Settings _settings;
        private readonly JsonSerializerOptions _jsonOptions;

        public OpenF1Client(IHttpClientFactory httpClientFactory, IOptions<OpenF1Settings> options)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient("OpenF1");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // 1. Pobieranie wszystkich wyścigów (głównych sesji "Race")
        public async Task<IEnumerable<OpenF1Race>> GetRacesAsync()
        {
            var response = await _httpClient.GetAsync("v1/sessions?session_name=Race");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var sessions = JsonSerializer.Deserialize<List<OpenF1SessionResponse>>(json, _jsonOptions);
            
            if (sessions == null) return [];

            var now = DateTime.UtcNow;

            return sessions.Select(s => new OpenF1Race
            {
                Id = s.SessionKey.ToString(),
                Name = $"{s.Location} Grand Prix",
                Date = s.DateStart ?? DateTime.UtcNow,
                Circuit = s.CircuitShortName ?? "Unknown Circuit",
                Country = s.CountryName ?? "Unknown Country",
                Season = s.Year ?? DateTime.UtcNow.Year,
                Location = s.Location ?? string.Empty,
                IsUpcoming = s.DateStart > now
            });
        }

        // 2. Pobieranie konkretnego wyścigu po jego Id (session_key)
        public async Task<OpenF1Race?> GetRaceByIdAsync(string raceId)
        {
            var response = await _httpClient.GetAsync($"v1/sessions?session_key={raceId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var sessions = JsonSerializer.Deserialize<List<OpenF1SessionResponse>>(json, _jsonOptions);
            var s = sessions?.FirstOrDefault();

            if (s == null) return null;

            var now = DateTime.UtcNow;
            return new OpenF1Race
            {
                Id = s.SessionKey.ToString(),
                Name = $"{s.Location} Grand Prix",
                Date = s.DateStart ?? DateTime.UtcNow,
                Circuit = s.CircuitShortName ?? "Unknown Circuit",
                Country = s.CountryName ?? "Unknown Country",
                Season = s.Year ?? DateTime.UtcNow.Year,
                Location = s.Location ?? string.Empty,
                IsUpcoming = s.DateStart > now
            };
        }

        // 3. Pobieranie kierowców dla danej sesji wyścigowej wraz z pełnymi danymi zespołów
        public async Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId)
        {
            var response = await _httpClient.GetAsync($"v1/drivers?session_key={raceId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var drivers = JsonSerializer.Deserialize<List<OpenF1DriverResponse>>(json, _jsonOptions);

            if (drivers == null) return [];

            return drivers.Select(d => new OpenF1DriverSessionData
            {
                RaceId = int.TryParse(raceId, out var rId) ? rId : d.SessionKey,
                DriverId = d.DriverNumber,
                DriverName = d.FullName ?? "Unknown Driver",
                TeamName = d.TeamName ?? "Unknown Team",
                Date = DateTime.UtcNow,
                NameAcronym = d.NameAcronym ?? string.Empty,
                TeamColour = d.TeamColour != null ? $"#{d.TeamColour}" : "#FFFFFF", // Formatowanie koloru do HEX HTML
                DriverNumber = d.DriverNumber
            });
        }

        // 4. Pobieranie ostatniego/najnowszego wyścigu (Kompatybilność)
        public async Task<OpenF1Race?> GetLatestRaceAsync()
        {
            var races = await GetRacesAsync();
            return races.OrderByDescending(r => r.Date).FirstOrDefault();
        }

        // 5. NOWE: Pobieranie nadchodzących wydarzeń (wyścigi z przyszłości)
        public async Task<IEnumerable<OpenF1Race>> GetUpcomingRacesAsync()
        {
            var races = await GetRacesAsync();
            return races.Where(r => r.IsUpcoming).OrderBy(r => r.Date);
        }

        // 6. NOWE: Pobieranie minionych wydarzeń (wyścigi historyczne)
        public async Task<IEnumerable<OpenF1Race>> GetPastRacesAsync()
        {
            var races = await GetRacesAsync();
            return races.Where(r => !r.IsUpcoming).OrderByDescending(r => r.Date);
        }

        // 7. NOWE: Pobieranie pełnych informacji o unikalnych zespołach z danego wyścigu
        public async Task<IEnumerable<OpenF1TeamData>> GetTeamsAsync(string raceId)
        {
            var drivers = await GetDriversAsync(raceId);
            return drivers
                .Where(d => !string.IsNullOrEmpty(d.TeamName))
                .Select(d => new OpenF1TeamData
                {
                    TeamName = d.TeamName,
                    TeamColour = d.TeamColour
                })
                .GroupBy(t => t.TeamName)
                .Select(g => g.First()); // Eliminacja duplikatów zespołów
        }

        // Klasa ustawień (Nienaruszona)
        public class OpenF1Settings
        {
            public string BaseUrl { get; set; } = "https://api.openf1.org";
            public int TimeoutSeconds { get; set; } = 30;
            public int RetryCount { get; set; } = 3;
            public int RetryDelaySeconds { get; set; } = 5;
        }

public async Task<IEnumerable<OpenF1ChampionshipDriverData>> GetDriverChampionshipStandingsAsync(string sessionKey)
        {
            var response = await _httpClient.GetAsync($"v1/championship_drivers?session_key={sessionKey}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var standings = JsonSerializer.Deserialize<List<OpenF1ChampionshipDriverResponse>>(json, _jsonOptions);

            if (standings == null) return [];

            return standings.Select(s => new OpenF1ChampionshipDriverData
            {
                DriverNumber = s.DriverNumber,
                MeetingKey = s.MeetingKey,
                PointsCurrent = s.PointsCurrent,
                PointsStart = s.PointsStart,
                PositionCurrent = s.PositionCurrent,
                PositionStart = s.PositionStart,
                SessionKey = s.SessionKey
            });
        }

// Dodaj do prywatnych klas na dole OpenF1Client:
private class OpenF1ChampionshipDriverResponse
        {
            [JsonPropertyName("driver_number")] public int DriverNumber { get; set; }
            [JsonPropertyName("meeting_key")] public int MeetingKey { get; set; }
            [JsonPropertyName("points_current")] public double? PointsCurrent { get; set; } // double?
            [JsonPropertyName("points_start")] public double? PointsStart { get; set; }     // double?
            [JsonPropertyName("position_current")] public int? PositionCurrent { get; set; } // int?
            [JsonPropertyName("position_start")] public int? PositionStart { get; set; }     // int?
            [JsonPropertyName("session_key")] public int SessionKey { get; set; }
        }

        // DTOs dopasowane precyzyjnie pod strukturę obiektów JSON z v1 OpenF1 API
        private class OpenF1SessionResponse
        {
            [JsonPropertyName("session_key")] public int SessionKey { get; set; }
            [JsonPropertyName("meeting_key")] public int MeetingKey { get; set; }
            [JsonPropertyName("session_name")] public string? SessionName { get; set; }
            [JsonPropertyName("circuit_short_name")] public string? CircuitShortName { get; set; }
            [JsonPropertyName("country_name")] public string? CountryName { get; set; }
            [JsonPropertyName("location")] public string? Location { get; set; }
            [JsonPropertyName("date_start")] public DateTime? DateStart { get; set; }
            [JsonPropertyName("year")] public int? Year { get; set; }
        }

        private class OpenF1DriverResponse
        {
            [JsonPropertyName("driver_number")] public int DriverNumber { get; set; }
            [JsonPropertyName("full_name")] public string? FullName { get; set; }
            [JsonPropertyName("name_acronym")] public string? NameAcronym { get; set; }
            [JsonPropertyName("team_name")] public string? TeamName { get; set; }
            [JsonPropertyName("team_colour")] public string? TeamColour { get; set; }
            [JsonPropertyName("session_key")] public int SessionKey { get; set; }
        }
    }
}