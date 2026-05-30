using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public interface IOpenF1ApiClient
    {
        Task<IEnumerable<OpenF1Race>> GetRacesAsync();
        Task<OpenF1Race?> GetRaceByIdAsync(string raceId);
        Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId);
        Task<OpenF1Race?> GetLatestRaceAsync();
    }


    public class OpenF1Race
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }
        public string Circuit { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Season { get; set; }
    }

    public class OpenF1DriverSessionData
    {
        public int RaceId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }
    }

    public class OpenF1Settings
    {
        // Optional path to the Python executable (fallback: "python")
        public string? PythonPath { get; set; }

        // Path to the openf1 CLI script (fallback: ./openf1/openf1_cli.py)
        public string? CliPath { get; set; }

        // Working directory to run the CLI from (fallback: current directory)
        public string? WorkingDirectory { get; set; }

        // Optional: base URL for the OpenF1 API used by the CLI (kept for configurability)
        public string BaseUrl { get; set; } = "https://openf1.org/api";

        // CLI request timeout in seconds
        public int TimeoutSeconds { get; set; } = 10;
    }
}
