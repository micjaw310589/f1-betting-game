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

        public async Task<IQueryable<Result>> GetRaceResultsAsync(int raceId)
        {
            try
            {
                _logger.LogInformation($"Getting results for race: {raceId}");
                return _dbSet
                    .Where(r => r.RaceId == raceId)
                    .Include(r => r.Driver)
                    .OrderBy(r => r.Position)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting results for race: {raceId}");
                throw;
            }
        }

        public async Task<IQueryable<Result>> GetDriverResultsAsync(int driverId)
        {
            try
            {
                _logger.LogInformation($"Getting results for driver: {driverId}");
                return _dbSet
                    .Where(r => r.DriverId == driverId)
                    .Include(r => r.Race)
                    .OrderByDescending(r => r.Race.Date)
                    .AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting results for driver: {driverId}");
                throw;
            }
        }
    }
}