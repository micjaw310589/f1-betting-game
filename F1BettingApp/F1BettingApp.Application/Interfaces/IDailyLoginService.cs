using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for daily login streak operations.
    /// </summary>
    public interface IDailyLoginService
    {
        /// <summary>
        /// Processes a daily login for the given user.
        /// Updates the streak, awards points, and publishes a domain event.
        /// Called after successful authentication.
        /// </summary>
        /// <param name="userId">The ID of the user logging in.</param>
        /// <returns>The number of points awarded for this login (0 if already claimed today).</returns>
        Task<int> ProcessDailyLoginAsync(int userId);

        /// <summary>
        /// Gets the current streak information for display in the UI.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A DTO with streak details, or null if no streak record exists.</returns>
        Task<DailyStreakInfoDto?> GetStreakInfoAsync(int userId);
    }
}
