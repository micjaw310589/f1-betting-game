using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.OpenF1;
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

        public async Task<SyncResultDto> SyncAllAsync(int season)
        {
            var result = new SyncResultDto
            {
                Success = false,
                SyncedAt = DateTime.UtcNow
            };

            try
            {
                // 1. Sync Race Calendar
                var races = await _openF1ApiClient.GetRaceCalendarAsync(season);
                var syncResult = await _syncPersistenceService.SyncRaceCalendar(races, season);
                result.RacesProcessed = syncResult.TotalCount;
                result.RacesCreated = syncResult.CreatedCount;
                result.RacesUpdated = syncResult.UpdatedCount;

                // 2. Sync Standings
                try
                {
                    var standings = await _openF1ApiClient.GetStandingsAsync(season);
                    await _syncPersistenceService.SyncStandings(standings, season);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync standings (non-fatal for full sync).");
                }

                // 3. Sync Master Data
                try
                {
                    var (drivers, teams) = await _openF1ApiClient.GetDriverAndTeamInfoAsync(season);
                    await _syncPersistenceService.SyncMasterData(drivers, teams, season);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync master data (non-fatal for full sync).");
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to perform full sync.");
            }

            return result;
        }
    }
}