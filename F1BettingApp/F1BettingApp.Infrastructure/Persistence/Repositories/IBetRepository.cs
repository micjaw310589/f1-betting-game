using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.ValueObjects;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IBetRepository : IRepository<Bet>
    {
        IQueryable<Bet> GetUserBets(int userId, BetStatus? status);
        IQueryable<Bet> GetPendingBetsForRace(int raceId);
        Task<BetStatistics> GetBetStatisticsAsync(int userId);
    }
}
