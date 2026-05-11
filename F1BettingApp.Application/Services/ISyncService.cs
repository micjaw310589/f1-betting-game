using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Contract for services responsible for handling the synchronization and persistence of OpenF1 data.
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Synchronizes the core race calendar data.
        /// </summary>
        Task SyncRaceCalendarAsync(int season);

        /// <summary>
        /// Synchronizes current and historical championship standings.
        /// </summary>
        Task SyncStandingsAsync(int season);

        /// <summary>
        /// Synchronizes driver and team master data.
        /// </summary>
        Task SyncMasterDataAsync(int season);

        /// <summary>
        /// Processes race results against user bets and updates leaderboards.
        /// </summary>
        Task SyncRaceResultsAsync(string raceId);
    }
}