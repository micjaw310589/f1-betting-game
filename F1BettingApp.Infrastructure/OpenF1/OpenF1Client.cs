using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using F1BettingApp.Application.Interfaces;
using System.Linq;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client : IOpenF1ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://api.openf1.org/v1";

        public OpenF1Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
            // Initialize client with base configuration, e.g., default headers
        }

        // Simple retry mechanism (can be enhanced with exponential backoff)
        private async Task<T> ExecuteApiCallAsync<T>(string endpoint, HttpMethod method, object parameters = null, int maxRetries = 3)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    HttpResponseMessage response = null;
                    string fullUrl = endpoint;

                    if (parameters != null)
                    {
                        // Handle query parameters for GET requests
                        if (method == HttpMethod.Get)
                        {
                            fullUrl += $"?{string.Join("&", parameters.GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(parameters)}"))}";
                        }
                        // Handle body parameters for POST requests (not used in this task, but good practice)
                        else
                        {
                            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(parameters), System.Text.Encoding.UTF8, "application/json");
                            var request = new HttpRequestMessage(method, endpoint) { Content = content };
                            response = await _httpClient.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                var json = await response.Content.ReadAsStringAsync();
                                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                            }
                        }
                    }
                    else
                    {
                        // Handle simple GET requests
                        response = await _httpClient.SendAsync(new HttpRequestMessage(method, fullUrl));
                    }


                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                    }
                    else if ((int)response.StatusCode == 429) // Rate Limit
                    {
                        // Read retry-after header if present
                        if (response.Headers.TryGetValues("Retry-After", out var values) && values.Any())
                        {
                            Console.WriteLine($"Rate limit hit. Retrying after {values.First()} seconds.");
                            await Task.Delay(int.Parse(values.First()) * 1000 + 2000); // Wait longer than required
                        }
                        else
                        {
                            // Fallback wait
                            await Task.Delay(2000 * (attempt + 1));
                        }
                        continue; // Retry the loop
                    }
                    else if ((int)response.StatusCode == 404)
                    {
                        throw new HttpRequestException($"Resource not found at {endpoint}.");
                    }
                    else
                    {
                        // Other failures
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"API call failed: {response.ReasonPhrase}. Details: {errorContent}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < maxRetries - 1)
                    {
                        Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}. Retrying in {Math.Pow(2, attempt) * 1000}ms...");
                        await Task.Delay((int)Math.Pow(2, attempt) * 1000); // Exponential backoff
                    }
                    else
                    {
                        throw; // Re-throw if all attempts fail
                    }
                }
            }
            throw new InvalidOperationException("Failed to execute API call after all retries.");
        }

        public async Task<List<RaceDto>> GetRaceCalendarAsync(int season)
        {
            // In a real implementation, we'd construct the full URL and parameters
            // For now, we simulate the call structure.
            // Assume we call /v1/calendar for the given season
            Console.WriteLine($"[OpenF1Client] Fetching race calendar for season {season}...");
            
            // Placeholder for actual API call simulation
            await Task.Delay(50); 
            return new List<RaceDto>
            {
                new RaceDto { RaceId = "race-1", Name = "Bahrain GP", Circuit = "Bahrain", Date = new DateTime(2024, 3, 2), Status = "Finished", Season = season },
                new RaceDto { RaceId = "race-2", Name = "Emilia Romagna GP", Circuit = "Imola", Date = new DateTime(2024, 4, 21), Status = "Finished", Season = season },
                new RaceDto { RaceId = "race-3", Name = "Monaco GP", Circuit = "Monte Carlo", Date = new DateTime(2024, 6, 1), Status = "Scheduled", Season = season }
            };
        }

        public async Task<RaceDto> GetRaceDetailsAsync(string raceId)
        {
            Console.WriteLine($"[OpenF1Client] Fetching race details for ID: {raceId}...");
            await Task.Delay(50);
            // Simulation
            return new RaceDto { RaceId = raceId, Name = "Sample Race", Circuit = "Sample Track", Date = DateTime.Now, Status = "Finished", Season = 2024 };
        }

        public async Task<List<DriverStandingsDto>> GetStandingsAsync(int season)
        {
            Console.WriteLine($"[OpenF1Client] Fetching standings for season {season}...");
            await Task.Delay(50);
            // Simulation
            return new List<DriverStandingsDto>
            {
                new DriverStandingsDto { DriverId = "d1", Name = "Verstappen", Points = 250, Position = 1 },
                new DriverStandingsDto { DriverId = "d2", Name = "Perez", Points = 180, Position = 2 }
            };
        }

        public async Task<(List<DriverDto> Drivers, List<TeamDto> Teams)> GetDriverAndTeamInfoAsync(int season)
        {
            Console.WriteLine($"[OpenF1Client] Fetching driver and team info for season {season}...");
            await Task.Delay(50);
            // Simulation
            var drivers = new List<DriverDto>
            {
                new DriverDto { DriverId = "d1", Name = "Verstappen", TeamId = "t1", OpenF1DriverId = "openf1d1" },
                new DriverDto { DriverId = "d2", Name = "Perez", TeamId = "t2", OpenF1DriverId = "openf1d2" }
            };
            var teams = new List<TeamDto>
            {
                new TeamDto { TeamId = "t1", Name = "Red Bull Racing", OpenF1TeamId = "openf1t1" },
                new TeamDto { TeamId = "t2", Name = "Nissan", OpenF1TeamId = "openf1t2" }
            };
            return (drivers, teams);
        }

        public async Task<List<RaceResultDto>> GetRaceResultsAsync(string raceId)
        {
            Console.WriteLine($"[OpenF1Client] Fetching race results for ID: {raceId}...");
            await Task.Delay(50);
            // Simulation
            return new List<RaceResultDto>
            {
                new RaceResultDto { DriverId = "d1", Position = 1, Points = 25, OpenF1ResultId = "res1" },
                new RaceResultDto { DriverId = "d2", Position = 2, Points = 18, OpenF1ResultId = "res2" }
            };
        }
    }
}