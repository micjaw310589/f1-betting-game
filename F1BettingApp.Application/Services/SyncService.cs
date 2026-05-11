using F1BettingApp.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service layer responsible for coordinating the data synchronization process.
    /// </summary>
    public class SyncService : ISyncService
    {
        private readonly IOpenF1ApiClient _openF1ApiClient;
        private readonly ISyncPersistenceService _syncPersistenceService;
        private readonly ILogger<SyncService> _logger;
        
        public SyncService(IOpenF1ApiClient openF1ApiClient, ISyncPersistenceService syncPersistenceService, ILogger<SyncService> logger)
        {
            _openF1ApiClient = openF1ApiClient;
            _syncPersistenceService = syncPersistenceService;
            _logger = logger;
        }

        public async Task SyncRaceCalendarAsync(int season)
        {
            try
            {
                var races = await _openF1ApiClient.GetRaceCalendarAsync(season);
                await _syncPersistenceService.SyncRaceCalendar(races, season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync race calendar.");
                throw; 
            }
        }

        public async Task SyncStandingsAsync(int season)
        {
            try
            {
                var standings = await _openF1ApiClient.GetStandingsAsync(season);
                await _syncPersistenceService.SyncStandings(standings, season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync standings.");
                throw;
            }
        }

        public async Task SyncMasterDataAsync(int season)
        {
            try
            {
                var (drivers, teams) = await _openF1ApiClient.GetDriverAndTeamInfoAsync(season);
                await _syncPersistenceService.SyncMasterData(drivers, teams, season);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync master data.");
                throw;
            }
        }

        public async Task SyncRaceResultsAsync(string raceId)
        {
            try
            {
                var results = await _openF1ApiClient.GetRaceResultsAsync(raceId);
                await _syncPersistenceService.SyncRaceResults(results, raceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync race results.");
                throw;
            }
        }
    }
}