using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IBetRepository : IRepository<Bet>
    {
        Task<IEnumerable<Bet>> GetUserBetsAsync(int userId);
        Task<IEnumerable<Bet>> GetPendingBetsForRaceAsync(int raceId);
        Task<Dictionary<string, int>> GetBetStatisticsAsync(int raceId);
    }
}