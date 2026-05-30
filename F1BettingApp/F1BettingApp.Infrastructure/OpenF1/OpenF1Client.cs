using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public class OpenF1Client : IOpenF1ApiClient
    {
        private readonly OpenF1Settings _settings;

        public OpenF1Client(IOptions<OpenF1Settings> options)
        {
            _settings = options.Value ?? new OpenF1Settings();
        }


        public async Task<IEnumerable<OpenF1Race>> GetRacesAsync()
        {
            var json = await RunCliAndGetJsonAsync("races", null);
            var items = ExtractList(json);
            return items.Select(ParseRace).ToList();
        }

        public async Task<OpenF1Race?> GetRaceByIdAsync(string raceId)
        {
            var param = $"race_id={raceId}";
            var json = await RunCliAndGetJsonAsync("races", param);
            var items = ExtractList(json);
            return items.Select(ParseRace).FirstOrDefault();
        }

        public async Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId)
        {
            var param = $"race_id={raceId}";
            var json = await RunCliAndGetJsonAsync("drivers", param);
            var items = ExtractList(json);
            return items.Select(ParseDriverSession).ToList();
        }

        public async Task<OpenF1Race?> GetLatestRaceAsync()
        {
            var races = await GetRacesAsync();
            return races.OrderByDescending(r => r.Date).FirstOrDefault();
        }

        protected virtual async Task<JsonElement> RunCliAndGetJsonAsync(string endpoint, string? param)
        {
            var pythonExe = _settings.PythonPath ?? "python";
            var cliPath = _settings.CliPath ?? Path.Combine(Directory.GetCurrentDirectory(), "openf1", "openf1_cli.py");
            var args = new List<string> { WrapArg(cliPath), endpoint, "--format", "json" };
            if (!string.IsNullOrWhiteSpace(param))
            {
                args.Add("--params");
                args.Add(param!);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _settings.WorkingDirectory ?? Directory.GetCurrentDirectory()
            };

            using var proc = new Process { StartInfo = startInfo };
            try
            {
                proc.Start();

                var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                var stderrTask = proc.StandardError.ReadToEndAsync();

                await Task.WhenAll(stdoutTask, stderrTask);

                proc.WaitForExit();

                var stdout = stdoutTask.Result;
                var stderr = stderrTask.Result;

                if (proc.ExitCode != 0)
                {
                    throw new InvalidOperationException($"openf1 cli failed (code {proc.ExitCode}): {stderr}");
                }

                using var doc = JsonDocument.Parse(stdout);
                return doc.RootElement.Clone();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to run openf1 CLI: " + ex.Message, ex);
            }
        }

        private static string WrapArg(string s)
        {
            if (s.Contains(" ")) return $"\"{s}\"";
            return s;
        }

        private static IEnumerable<JsonElement> ExtractList(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.EnumerateArray().ToList();
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (var candidate in new[] { "data", "items", "results", "drivers", "races", "sessions" })
                {
                    if (root.TryGetProperty(candidate, out var prop) && prop.ValueKind == JsonValueKind.Array)
                        return prop.EnumerateArray().ToList();
                }

                // fallback: first property that is an array
                foreach (var p in root.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.Array) return p.Value.EnumerateArray().ToList();
                }
            }

            // wrap single object/value
            return new[] { root };
        }

        private static OpenF1Race ParseRace(JsonElement el)
        {
            string id = GetStringFromElement(el, "race_id") ?? GetStringFromElement(el, "id") ?? string.Empty;
            string name = GetStringFromElement(el, "name") ?? GetStringFromElement(el, "circuit_name") ?? "Unknown Race";
            DateTime date = GetDateFromElement(el, "date_utc") ?? GetDateFromElement(el, "date") ?? DateTime.UtcNow;
            string circuit = GetStringFromElement(el, "circuit_name") ?? "Unknown Circuit";
            string country = GetStringFromElement(el, "country_name") ?? GetStringFromElement(el, "country") ?? "Unknown";
            int season = GetIntFromElement(el, "year") ?? date.Year;

            return new OpenF1Race
            {
                Id = id,
                Name = name,
                Date = date,
                Circuit = circuit,
                Country = country,
                Season = season
            };
        }

        private static OpenF1DriverSessionData ParseDriverSession(JsonElement el)
        {
            int raceId = GetIntFromElement(el, "race_id") ?? 0;
            int driverId = GetIntFromElement(el, "driver_id") ?? 0;
            string driverName = GetStringFromElement(el, "driver_name") ?? "Unknown Driver";
            string teamName = GetStringFromElement(el, "team_name") ?? GetStringFromElement(el, "constructor_name") ?? "Unknown Team";
            DateTime date = GetDateFromElement(el, "date") ?? DateTime.UtcNow;

            return new OpenF1DriverSessionData
            {
                RaceId = raceId,
                DriverId = driverId,
                DriverName = driverName,
                TeamName = teamName,
                Date = date
            };
        }

        private static string? GetStringFromElement(JsonElement el, string propName)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(propName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
                if (prop.ValueKind == JsonValueKind.Number) return prop.GetRawText();
            }
            return null;
        }

        private static int? GetIntFromElement(JsonElement el, string propName)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(propName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var v)) return v;
                if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var vs)) return vs;
            }
            return null;
        }

        private static DateTime? GetDateFromElement(JsonElement el, string propName)
        {
            var s = GetStringFromElement(el, propName);
            if (string.IsNullOrEmpty(s)) return null;
            if (DateTime.TryParse(s, out var dt)) return dt;
            return null;
        }
    }
}