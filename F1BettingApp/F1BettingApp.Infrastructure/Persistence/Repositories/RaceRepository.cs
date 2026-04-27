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
    public class RaceRepository : Repository<Race>, IRaceRepository, IRaceRepositoryExtensions
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
                return await _dbSet
                    .Where(r => r.Status == RaceStatus.Scheduled)
                    .OrderBy(r => r.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming races");
                throw;
            }
        }

        public async Task<Race> GetRaceWithOddsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Bets)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting race with odds for race: {id}");
                throw;
            }
        }

        public bool CanPlaceBets(Race race)
        {
            return race.Status == RaceStatus.Scheduled && !race.Bets.Any(b => b.Status == BetStatus.Completed);
        }

        public decimal? GetTotalAmountWageredAsync(int raceId)
        {
            try
            {
                var bets = _dbSet.Where(b => b.RaceId == raceId).ToList();
                return bets.Sum(b => b.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total amount wagered for race: {raceId}");
                throw;
            }
        }

        public int? GetTotalBetsCountAsync(int raceId)
        {
            try
            {
                return _dbSet.Count(b => b.RaceId == raceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total bets count for race: {raceId}");
                throw;
            }
        }

        public async Task<IEnumerable<Race>> GetByIdsAsync(IEnumerable<int> ids)
        {
            try
            {
                return await _dbSet.Where(r => ids.Contains(r.Id)).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting races by IDs: {string.Join(", ", ids)}");
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