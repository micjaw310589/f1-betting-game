using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Service interface for leaderboard operations and ranking calculations.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Gets the global leaderboard with top players.
        /// </summary>
        /// <param name="limit">Maximum number of entries to return.</param>
        /// <returns>The sorted list of leaderboard entries.</returns>
        Task<IEnumerable<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(int limit);

        /// <summary>
        /// Gets the top players by count.
        /// </summary>
        /// <param name="count">Number of top players to retrieve.</param>
        /// <returns>The list of top players.</returns>
        Task<IEnumerable<LeaderboardEntryDto>> GetTopPlayersAsync(int count);

        /// <summary>
        /// Gets the current user's ranking information.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>The user's ranking details.</returns>
        Task<UserRankingDto> GetUserRankingAsync(int userId);

        /// <summary>
        /// Gets historical leaderboard data for a specific season.
        /// </summary>
        /// <param name="season">The season identifier (e.g., "2024").</param>
        /// <returns>The historical leaderboard data.</returns>
        Task<IEnumerable<HistoricalLeaderboardDto>> GetHistoricalLeaderboardAsync(string? season = null);

        /// <summary>
        /// Gets the current user's rank change since last session.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>The rank change information.</returns>
        Task<int> GetRankChangeAsync(int userId);

        /// <summary>
        /// Gets points needed to reach the next rank.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <returns>The points needed for next rank.</returns>
        Task<long> GetPointsToNextRankAsync(int userId);
    }
}