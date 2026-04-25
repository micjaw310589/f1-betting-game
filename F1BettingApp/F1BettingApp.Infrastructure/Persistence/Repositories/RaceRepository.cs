using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        public RaceRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Race> GetUpcomingRaces()
        {
            var now = DateTime.UtcNow;
            return _dbSet
                .Where(r => r.Date >= now && r.Status == RaceStatus.Scheduled)
                .OrderBy(r => r.Date);
        }

        public async Task<Race?> GetRaceWithResultsAsync(int raceId)
        {
            return await _dbSet
                .Include(r => r.Results)
                .Include(r => r.Bets)
                .FirstOrDefaultAsync(r => r.Id == raceId);
        }

        public IQueryable<Race> GetCurrentSeasonRaces()
        {
            var currentYear = DateTime.UtcNow.Year;
            return _dbSet
                .Where(r => r.Season == currentYear)
                .OrderBy(r => r.Date);
        }
    }
}
