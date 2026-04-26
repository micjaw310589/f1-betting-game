using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for leaderboard-related operations
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Updates the leaderboard based on current user points
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateLeaderboardAsync();

        /// <summary>
        /// Gets the current leaderboard
        /// </summary>
        /// <param name="limit">Maximum number of entries to return</param>
        /// <returns>Collection of user points DTOs representing the leaderboard</returns>
        Task<IEnumerable<UserPointsDto>> GetCurrentLeaderboardAsync(int limit = 10);

        /// <summary>
        /// Gets the season leaderboard
        /// </summary>
        /// <param name="season">The season year</param>
        /// <param name="limit">Maximum number of entries to return</param>
        /// <returns>Collection of user points DTOs representing the season leaderboard</returns>
        Task<IEnumerable<UserPointsDto>> GetSeasonLeaderboardAsync(int season, int limit = 10);
    }
}