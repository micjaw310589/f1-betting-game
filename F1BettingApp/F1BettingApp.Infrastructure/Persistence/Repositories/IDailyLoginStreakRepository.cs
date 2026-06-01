using F1BettingApp.Domain.Entities;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository interface for DailyLoginStreak entity operations.
    /// </summary>
    public interface IDailyLoginStreakRepository
    {
        /// <summary>
        /// Gets the daily login streak for a specific user by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The DailyLoginStreak entity, or null if not found.</returns>
        Task<DailyLoginStreak?> GetByUserIdAsync(int userId);

        /// <summary>
        /// Checks whether a daily login streak record exists for the given user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>True if a streak record exists for the user.</returns>
        Task<bool> ExistsByUserIdAsync(int userId);

        /// <summary>
        /// Upserts a daily login streak record for a user.
        /// Creates a new record if none exists, or updates the existing one.
        /// </summary>
        /// <param name="streak">The streak entity to upsert.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpsertAsync(DailyLoginStreak streak);

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task SaveChangesAsync();
    }
}
