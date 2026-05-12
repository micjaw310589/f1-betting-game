using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
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
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }
        }

        private async Task<T> ExecuteApiCallAsync<T>(string endpoint, int maxRetries = 3)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("API returned null");
                    }
                    else if ((int)response.StatusCode == 429) // Rate Limit
                    {
                        if (response.Headers.TryGetValues("Retry-After", out var values) && values.Any())
                        {
                            await Task.Delay(int.Parse(values.First()) * 1000 + 1000);
                        }
                        else
                        {
                            await Task.Delay(2000 * (attempt + 1));
                        }
                        continue;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"API call to {endpoint} failed: {response.StatusCode}. Details: {errorContent}");
                    }
                }
                catch (HttpRequestException) when (attempt < maxRetries - 1)
                {
                    await Task.Delay((int)Math.Pow(2, attempt) * 1000);
                }
            }
            throw new InvalidOperationException($"Failed to execute API call to {endpoint} after {maxRetries} retries.");
        }

        public async Task<IEnumerable<OpenF1Race>> GetRacesAsync()
        {
            var response = await ExecuteApiCallAsync<List<OpenF1RaceResponse>>("races");
            return response.Select(r => new OpenF1Race
            {
                Id = r.race_id?.ToString() ?? string.Empty,
                Name = r.name ?? r.circuit_name ?? "Unknown Race",
                Date = r.date_utc ?? DateTime.UtcNow,
                Circuit = r.circuit_name ?? "Unknown Circuit",
                Country = r.country_name ?? "Unknown",
                Season = r.year ?? DateTime.UtcNow.Year
            });
        }

        public async Task<OpenF1Race?> GetRaceByIdAsync(string raceId)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1RaceResponse>>($"races?race_id={raceId}");
            var race = response.FirstOrDefault();
            if (race == null) return null;

            return new OpenF1Race
            {
                Id = race.race_id?.ToString() ?? string.Empty,
                Name = race.name ?? race.circuit_name ?? "Unknown Race",
                Date = race.date_utc ?? DateTime.UtcNow,
                Circuit = race.circuit_name ?? "Unknown Circuit",
                Country = race.country_name ?? "Unknown",
                Season = race.year ?? DateTime.UtcNow.Year
            };
        }

        public async Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1DriverSessionResponse>>($"drivers?race_id={raceId}");
            return response.Select(s => new OpenF1DriverSessionData
            {
                RaceId = s.race_id ?? 0,
                DriverId = s.driver_id ?? 0,
                DriverName = s.driver_name ?? "Unknown Driver",
                TeamName = s.team_name ?? "Unknown Team",
                Date = s.date ?? DateTime.UtcNow
            });
        }

        public async Task<OpenF1Race?> GetLatestRaceAsync()
        {
            var races = await GetRacesAsync();
            return races.OrderByDescending(r => r.Date).FirstOrDefault();
        }

        // Implementation for TASK-02 Sync Methods
        
        public async Task<List<RaceDto>> GetRaceCalendarAsync(int season)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1RaceResponse>>($"races?year={season}");
            return response.Select(r => new RaceDto
            {
                RaceId = r.race_id?.ToString() ?? string.Empty,
                Name = r.name ?? r.circuit_name ?? "Unknown Race",
                Circuit = r.circuit_name ?? "Unknown Circuit",
                Date = r.date_utc ?? DateTime.UtcNow,
                Status = r.date_utc < DateTime.UtcNow ? "Finished" : "Scheduled",
                Season = r.year ?? season
            }).ToList();
        }

        public async Task<List<DriverStandingsDto>> GetStandingsAsync(int season)
        {
            // OpenF1 doesn't have a direct 'standings' endpoint in the same way as Ergast.
            // Typically you calculate from results or use another source.
            // For now, we fetch drivers from the season as a proxy or use a specific session if known.
            // Placeholder: Returning empty or simulated if endpoint not directly available.
            // Based on OpenF1 docs, we might need to aggregate from session_results.
            return new List<DriverStandingsDto>();
        }

        public async Task<(List<DriverDto> Drivers, List<TeamDto> Teams)> GetDriverAndTeamInfoAsync(int season)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1DriverSessionResponse>>($"drivers?year={season}");
            
            var drivers = response.GroupBy(d => d.driver_id).Select(g => {
                var first = g.First();
                return new DriverDto {
                    DriverId = first.driver_id?.ToString() ?? string.Empty,
                    Name = first.driver_name ?? "Unknown",
                    TeamId = first.team_name ?? "Unknown",
                    OpenF1DriverId = first.driver_id?.ToString() ?? string.Empty
                };
            }).ToList();

            var teams = response.GroupBy(d => d.team_name).Select(g => {
                var first = g.First();
                return new TeamDto {
                    TeamId = first.team_name ?? string.Empty,
                    Name = first.team_name ?? "Unknown Team",
                    OpenF1TeamId = first.team_name ?? string.Empty
                };
            }).ToList();

            return (drivers, teams);
        }

        public async Task<List<RaceResultDto>> GetRaceResultsAsync(string raceId)
        {
            // Fetch results for a specific race session
            // We'd need the session_key for the race.
            return new List<RaceResultDto>();
        }

        // Internal settings class
        public class OpenF1Settings
        {
            public string BaseUrl { get; set; } = "https://api.openf1.org/v1";
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
            public DateTime? date_utc { get; set; }
            public int? year { get; set; }
        }

        private class OpenF1DriverSessionResponse
        {
            public int? race_id { get; set; }
            public int? driver_id { get; set; }
            public string? driver_name { get; set; }
            public string? team_name { get; set; }
            public DateTime? date { get; set; }
        }
    }
}
