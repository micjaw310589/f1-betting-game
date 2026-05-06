// Extension implementation for Bet-specific queries
// These methods are used by BettingService but not in the base IRepository<T> interface

using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Extension implementation providing domain-specific query operations for bets.
    /// </summary>
    public class BetRepositoryExtensions : Repository<Bet>, IBetRepositoryExtensions
    {
        private readonly AppDbContext _context;

        public BetRepositoryExtensions(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IList<Bet>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(b => b.UserId == userId).ToListAsync();
        }

        public async Task<IList<Bet>> GetByRaceIdAsync(int raceId)
        {
            return await _dbSet.Where(b => b.RaceId == raceId).ToListAsync();
        }

        public async Task<IList<Bet>> GetUserActiveBetsAsync(int userId, int raceId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId && b.RaceId == raceId && b.Status == BetStatus.Pending)
                .ToListAsync();
        }

        public async Task<bool> HasActiveBetOnDriverAsync(int userId, int raceId, int driverId)
        {
            return await _dbSet
                .AnyAsync(b => b.UserId == userId && b.RaceId == raceId && b.DriverId == driverId && b.Status == BetStatus.Pending);
        }

        public async Task<decimal> GetUserTotalWagerOnDriverAsync(int userId, int raceId, int driverId)
        {
            var total = await _dbSet
                .Where(b => b.UserId == userId && b.RaceId == raceId && b.DriverId == driverId && b.Status == BetStatus.Pending)
                .Select(b => b.Amount)
                .ToListAsync();
            
            return total.Any() ? total.Sum() : 0m;
        }
    }
}