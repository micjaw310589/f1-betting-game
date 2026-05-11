using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    public interface IOpenF1ApiClient
    {
        /// <summary>
        /// Retrieves the list of upcoming and past race calendars.
        /// </summary>
        /// <param name="season">The current F1 season year.</param>
        /// <returns>A list of race data objects.</returns>
        Task<List<RaceDto>> GetRaceCalendarAsync(int season);

        /// <summary>
        /// Retrieves detailed data for a specific race.
        /// </summary>
        /// <param name="raceId">The unique ID of the race.</param>
        /// <returns>Race detail object.</returns>
        Task<RaceDto> GetRaceDetailsAsync(string raceId);

        /// <summary>
        /// Retrieves the current championship standings.
        /// </summary>
        /// <param name="season">The current F1 season year.</param>
        /// <returns>A list of driver standings.</returns>
        Task<List<DriverStandingsDto>> GetStandingsAsync(int season);

        /// <summary>
        /// Retrieves general driver and team information.
        /// </summary>
        /// <param name="season">The current F1 season year.</param>
        /// <returns>A tuple containing lists of drivers and teams.</returns>
        Task<(List<DriverDto> Drivers, List<TeamDto> Teams)> GetDriverAndTeamInfoAsync(int season);

        /// <summary>
        /// Retrieves detailed race results for a completed race.
        /// </summary>
        /// <param name="raceId">The unique ID of the race.</param>
        /// <returns>List of result entries.</returns>
        Task<List<RaceResultDto>> GetRaceResultsAsync(string raceId);
    }

    // DTOs for API responses (simplified for task structure)
    public class RaceDto
    {
        public string RaceId { get; set; }
        public string Name { get; set; }
        public string Circuit { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // e.g., Scheduled, Finished
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