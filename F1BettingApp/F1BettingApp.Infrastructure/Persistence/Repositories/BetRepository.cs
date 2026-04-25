using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Domain.ValueObjects;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class BetRepository : Repository<Bet>, IBetRepository
    {
        public BetRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Bet> GetUserBets(int userId, BetStatus? status)
        {
            var query = (IQueryable<Bet>)_dbSet.Where(b => b.UserId == userId).Include(b => b.Race).Include(b => b.Driver);

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            return query;
        }

        public IQueryable<Bet> GetPendingBetsForRace(int raceId)
        {
            return (IQueryable<Bet>)_dbSet
                .Where(b => b.RaceId == raceId && b.Status == BetStatus.Pending)
                .Include(b => b.User);
        }

        public async Task<BetStatistics> GetBetStatisticsAsync(int userId)
        {
            var bets = await _dbSet
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var statistics = new BetStatistics
            {
                UserId = userId,
                TotalBets = bets.Count,
                WonBets = bets.Count(b => b.Status == BetStatus.Won),
                LostBets = bets.Count(b => b.Status == BetStatus.Lost),
                PendingBets = bets.Count(b => b.Status == BetStatus.Pending),
                TotalStaked = bets.Sum(b => b.Amount),
                TotalPotentialWinnings = bets.Sum(b => b.PotentialWinnings),
                TotalWinnings = bets.Where(b => b.Status == BetStatus.Won).Sum(b => b.PotentialWinnings)
            };

            return statistics;
        }
    }
}
