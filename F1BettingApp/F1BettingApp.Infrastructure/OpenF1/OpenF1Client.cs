using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;
using F1BettingApp.Domain.OpenF1;

namespace F1BettingApp.Infrastructure.OpenF1
{
    /// <summary>
    /// HTTP client implementation for the OpenF1 API.
    /// Implements the IOpenF1ApiClient interface from the Domain layer.
    /// </summary>
    public class OpenF1Client : IOpenF1ApiClient
    {
        private readonly HttpClient _httpClient;

        public OpenF1Client(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("OpenF1");
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.openf1.org/v1");
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
                        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                            ?? throw new InvalidOperationException("API returned null");
                    }
                    else if ((int)response.StatusCode == (int)HttpStatusCode.TooManyRequests)
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
                        throw new HttpRequestException(
                            $"API call to {endpoint} failed: {response.StatusCode}. Details: {errorContent}");
                    }
                }
                catch (HttpRequestException) when (attempt < maxRetries - 1)
                {
                    await Task.Delay((int)Math.Pow(2, attempt) * 1000);
                }
            }
            throw new InvalidOperationException($"Failed to execute API call to {endpoint} after {maxRetries} retries.");
        }

        /// <inheritdoc />
        public async Task<List<RaceDto>> GetRaceCalendarAsync(int season)
{
    var response = await ExecuteApiCallAsync<List<OpenF1RaceResponse>>($"races?year={season}");
    var result = new List<RaceDto>();

    foreach (var r in response)
    {
        // 1. Pomijamy rekordy bez race_id
        if (r.race_id == null)
        {
            Console.WriteLine($"[OpenF1] Ignoring race with null race_id: {r.name}");
            continue;
        }

        // 2. Pomijamy rekordy bez daty (OpenF1 czasem zwraca takie śmieci)
        if (!r.date_utc.HasValue)
        {
            Console.WriteLine($"[OpenF1] Ignoring race without date: {r.name} (race_id={r.race_id})");
            continue;
        }

        // 3. Ustalanie statusu w sposób bezpieczny
        if (!r.date_utc.HasValue)
        {
            Console.WriteLine($"[OpenF1] Ignoring race without date: {r.name}");
            continue;
        }

        string status = r.date_utc.Value < DateTime.UtcNow
            ? "Finished"
            : "Scheduled";


        // 4. Mapowanie do DTO
        result.Add(new RaceDto
        {
            RaceId = r.race_id.Value.ToString(),
            Name = r.name ?? r.circuit_name ?? "Unknown Race",
            Circuit = r.circuit_name ?? "Unknown Circuit",
            Date = r.date_utc.Value,
            Status = status,
            Season = r.year ?? season
        });
    }

    return result;
}


        /// <inheritdoc />
        public async Task<RaceDto> GetRaceDetailsAsync(string raceId)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1RaceResponse>>($"races?race_id={raceId}");
            var race = response.FirstOrDefault();
            if (race == null) return null;
            

            return new RaceDto
            {
                RaceId = race.race_id?.ToString() ?? string.Empty,
                Name = race.name ?? race.circuit_name ?? "Unknown Race",
                Circuit = race.circuit_name ?? "Unknown Circuit",
                Date = race.date_utc ?? DateTime.UtcNow,
                Status = race.date_utc < DateTime.UtcNow ? "Finished" : "Scheduled",
                Season = race.year ?? DateTime.UtcNow.Year
            };
        }

        /// <inheritdoc />
        public async Task<List<DriverStandingsDto>> GetStandingsAsync(int season)
        {
            // OpenF1 doesn't have a direct 'standings' endpoint.
            // Standings must be aggregated from session results over the season.
            return new List<DriverStandingsDto>();
        }

        /// <inheritdoc />
        public async Task<(List<DriverDto> Drivers, List<TeamDto> Teams)> GetDriverAndTeamInfoAsync(int season)
        {
            var response = await ExecuteApiCallAsync<List<OpenF1DriverSessionResponse>>($"drivers?year={season}");
            
            var drivers = response
                .GroupBy(d => d.driver_id)
                .Select(g => {
                    var first = g.First();
                    return new DriverDto {
                        DriverId = first.driver_id?.ToString() ?? string.Empty,
                        Name = first.driver_name ?? "Unknown",
                        TeamId = first.team_name ?? "Unknown",
                        OpenF1DriverId = first.driver_id?.ToString() ?? string.Empty
                    };
                })
                .ToList();

            var teams = response
                .GroupBy(d => d.team_name)
                .Select(g => {
                    var first = g.First();
                    return new TeamDto {
                        TeamId = first.team_name ?? string.Empty,
                        Name = first.team_name ?? "Unknown Team",
                        OpenF1TeamId = first.team_name ?? string.Empty
                    };
                })
                .ToList();

            return (drivers, teams);
        }

        /// <inheritdoc />
        public async Task<List<RaceResultDto>> GetRaceResultsAsync(string raceId)
        {
            return new List<RaceResultDto>();
        }

        // == Internal DTOs for deserializing OpenF1 API responses ==

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