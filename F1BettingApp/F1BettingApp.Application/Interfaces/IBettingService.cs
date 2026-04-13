using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    public interface IBettingService
    {
        Task PlaceBetAsync(int userId, int raceId, int driverId, decimal amount);
        Task CancelBetAsync(int betId);
        Task<IEnumerable<BetDto>> GetUserBetsAsync(int userId);
    }
}