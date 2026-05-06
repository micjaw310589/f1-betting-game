// Extension implementation for Race-specific queries
// These methods are used by BettingService but not in the base IRepository<T> interface

using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Extension implementation providing domain-specific query operations for races.
    /// </summary>
    public class RaceRepositoryExtensions : Repository<Race>, IRaceRepositoryExtensions
    {
        private readonly AppDbContext _context;

        public RaceRepositoryExtensions(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Race>> GetUpcomingRacesAsync()
        {
            return await _dbSet
                .Where(r => r.Status == RaceStatus.Scheduled || r.Status == RaceStatus.InProgress)
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task<Race> GetRaceWithOddsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Bets)
                .FirstOrDefaultAsync(r => r.Id == id) ?? throw new KeyNotFoundException($"Race with id {id} not found.");
        }

        public bool CanPlaceBets(Race race)
        {
            return race.Status == RaceStatus.Scheduled || race.Status == RaceStatus.InProgress;
        }

        public async Task<decimal?> GetTotalAmountWageredAsync(int raceId)
        {
            var bets = await _context.Set<Bet>()
                .Where(b => b.RaceId == raceId)
                .ToListAsync();
            
            return bets.Any() ? bets.Sum(b => b.Amount) : (decimal?)null;
        }

        public async Task<int?> GetTotalBetsCountAsync(int raceId)
        {
            return await _context.Set<Bet>().CountAsync(b => b.RaceId == raceId);
        }

        public async Task<IEnumerable<Race>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idsList = ids.ToList();
            return await _dbSet.Where(r => idsList.Contains(r.Id)).ToListAsync();
        }
    }
}