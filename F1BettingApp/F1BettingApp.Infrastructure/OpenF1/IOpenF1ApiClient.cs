using System;
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
        
        // Added for TASK-02 synchronization
        Task<List<RaceDto>> GetRaceCalendarAsync(int season);
        Task<List<DriverStandingsDto>> GetStandingsAsync(int season);
        Task<(List<DriverDto> Drivers, List<TeamDto> Teams)> GetDriverAndTeamInfoAsync(int season);
        Task<List<RaceResultDto>> GetRaceResultsAsync(string raceId);
    }

    public class OpenF1Race
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
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
        public DateTime Date { get; set; }
    }

    // DTOs for API responses
    public class RaceDto
    {
        public string RaceId { get; set; }
        public string Name { get; set; }
        public string Circuit { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int Season { get; set; }
    }

    public class DriverStandingsDto
    {
        public string DriverId { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
    }

    public class DriverDto
    {
        public string DriverId { get; set; }
        public string Name { get; set; }
        public string TeamId { get; set; }
        public string OpenF1DriverId { get; set; }
    }

    public class TeamDto
    {
        public string TeamId { get; set; }
        public string Name { get; set; }
        public string OpenF1TeamId { get; set; }
    }
    
    public class RaceResultDto
    {
        public string DriverId { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }
        public string OpenF1ResultId { get; set; }
    }
}
