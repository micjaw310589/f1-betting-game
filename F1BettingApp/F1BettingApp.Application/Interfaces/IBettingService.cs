using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for betting-related operations
    /// </summary>
    public interface IBettingService
    {
        /// <summary>
        /// Places a bet on a specific driver in a race
        /// </summary>
        /// <param name="userId">The ID of the user placing the bet</param>
        /// <param name="raceId">The ID of the race</param>
        /// <param name="driverId">The ID of the driver being bet on</param>
        /// <param name="amount">The amount to bet</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task PlaceBetAsync(int userId, int raceId, int driverId, decimal amount);

        /// <summary>
        /// Cancels an existing bet
        /// </summary>
        /// <param name="betId">The ID of the bet to cancel</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CancelBetAsync(int betId);

        /// <summary>
        /// Gets all bets for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Collection of bet DTOs</returns>
        Task<IEnumerable<BetDto>> GetUserBetsAsync(int userId);

        /// <summary>
        /// Gets a specific bet by ID
        /// </summary>
        /// <param name="betId">The ID of the bet</param>
        /// <returns>The bet DTO or null if not found</returns>
        Task<BetDto?> GetBetByIdAsync(int betId);

        /// <summary>
        /// Processes race results and updates bet statuses
        /// </summary>
        /// <param name="raceId">The ID of the race to process</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ProcessRaceResultsAsync(int raceId);
    }
}