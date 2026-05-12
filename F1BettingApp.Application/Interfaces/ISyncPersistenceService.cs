using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Handles the mapping and persistence of data retrieved from the OpenF1 API to local database entities.
    /// </summary>
    public interface ISyncPersistenceService
    {
        /// <summary>
        /// Persists race calendar entries.
        /// </summary>
        Task<SyncRaceCalendarResult> SyncRaceCalendar(List<RaceDto> races, int season);

        /// <summary>
        /// Persists championship standings.
        /// </summary>
        Task SyncStandings(List<DriverStandingsDto> standings, int season);

        /// <summary>
        /// Persists driver and team master data.
        /// </summary>
        Task SyncMasterData(List<DriverDto> drivers, List<TeamDto> teams, int season);

        /// <summary>
        /// Processes and persists race results, triggering bet updates.
        /// </summary>
        Task SyncRaceResults(List<RaceResultDto> results, string raceId);
    }
}
