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
    public class BetRepository : Repository<Bet>, IBetRepository
    {
        private readonly ILogger<BetRepository> _logger;

        public BetRepository(AppDbContext context, ILogger<BetRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IQueryable<Bet>> GetUserBetsAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Getting bets for user: {userId}");
                return _dbSet
                    .Where(b => b.UserId == userId)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting bets for user: {userId}");
                throw;
            }
        }

        public async Task<IQueryable<Bet>> GetPendingBetsForRaceAsync(int raceId)
        {
            try
            {
                _logger.LogInformation($"Getting pending bets for race: {raceId}");
                return _dbSet
                    .Where(b => b.RaceId == raceId && b.Status == BetStatus.Pending)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pending bets for race: {raceId}");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetBetStatisticsAsync(int raceId)
        {
            try
            {
                _logger.LogInformation($"Getting bet statistics for race: {raceId}");
                var bets = await _dbSet
                    .Where(b => b.RaceId == raceId)
                    .ToListAsync();

                var statistics = new Dictionary<string, int>();
                var betTypes = bets.Select(b => b.BetType).Distinct();

                foreach (var betType in betTypes)
                {
                    statistics[betType.ToString()] = bets.Count(b => b.BetType == betType);
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting bet statistics for race: {raceId}");
                throw;
            }
        }
    }
}