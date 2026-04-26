using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        private readonly ILogger<RaceRepository> _logger;

        public RaceRepository(AppDbContext context, ILogger<RaceRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IQueryable<Race>> GetUpcomingRacesAsync()
        {
            try
            {
                _logger.LogInformation("Getting upcoming races");
                return _dbSet
                    .Where(r => r.Status == RaceStatus.Scheduled)
                    .OrderBy(r => r.Date)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming races");
                throw;
            }
        }

        public async Task<Race> GetRaceWithResultsAsync(int raceId)
        {
            try
            {
                _logger.LogInformation($"Getting race with results for race: {raceId}");
                return await _dbSet
                    .Include(r => r.Bets)
                    .FirstOrDefaultAsync(r => r.Id == raceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting race with results for race: {raceId}");
                throw;
            }
        }

        public async Task<IQueryable<Race>> GetCurrentSeasonRacesAsync(int seasonId)
        {
            try
            {
                _logger.LogInformation($"Getting races for season: {seasonId}");
                return _dbSet
                    .Where(r => r.Season == seasonId)
                    .OrderBy(r => r.Date)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting races for season: {seasonId}");
                throw;
            }
        }
    }
}