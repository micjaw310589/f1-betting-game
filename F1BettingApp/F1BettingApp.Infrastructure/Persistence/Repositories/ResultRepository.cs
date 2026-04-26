using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class ResultRepository : Repository<Result>, IResultRepository
    {
        private readonly ILogger<ResultRepository> _logger;

        public ResultRepository(AppDbContext context, ILogger<ResultRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Result>> GetRaceResultsAsync(int raceId)
        {
            try
            {
                _logger.LogInformation($"Getting results for race: {raceId}");
                return await _dbSet
                    .Where(r => r.RaceId == raceId)
                    .Include(r => r.Driver)
                    .OrderBy(r => r.Position)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting results for race: {raceId}");
                throw;
            }
        }

        public async Task<IEnumerable<Result>> GetDriverResultsAsync(int driverId)
        {
            try
            {
                _logger.LogInformation($"Getting results for driver: {driverId}");
                return await _dbSet
                    .Where(r => r.DriverId == driverId)
                    .Include(r => r.Race)
                    .OrderByDescending(r => r.Race.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting results for driver: {driverId}");
                throw;
            }
        }
    }
}