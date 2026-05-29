using F1BettingApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository interface for WeeklyQuestProgress entity operations.
    /// </summary>
    public interface IWeeklyQuestProgressRepository
    {
        /// <summary>
        /// Gets all quest progress records for a user, optionally filtered by week/year.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="weekNumber">Optional ISO week number filter.</param>
        /// <param name="year">Optional year filter.</param>
        /// <returns>All matching quest progress records.</returns>
        Task<IQueryable<WeeklyQuestProgress>> GetAllAsync(int userId, int? weekNumber = null, int? year = null);

        /// <summary>
        /// Gets a specific quest progress record for a user in a given week.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="questId">The quest identifier.</param>
        /// <param name="weekNumber">The ISO week number.</param>
        /// <param name="year">The year.</param>
        /// <returns>The quest progress record, or null if not found.</returns>
        Task<WeeklyQuestProgress?> GetAsync(int userId, string questId, int weekNumber, int year);

        /// <summary>
        /// Gets all active (not claimed) quest progress records for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>All active quest progress records.</returns>
        Task<IQueryable<WeeklyQuestProgress>> GetActiveAsync(int userId);

        /// <summary>
        /// Creates or updates a quest progress record (upsert).
        /// </summary>
        /// <param name="progress">The quest progress record to upsert.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpsertAsync(WeeklyQuestProgress progress);

        /// <summary>
        /// Resets all weekly quest progress records for a user for the given week/year.
        /// Used during weekly reset.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="weekNumber">The ISO week number to reset.</param>
        /// <param name="year">The year.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task ResetWeekAsync(int userId, int weekNumber, int year);

        /// <summary>
        /// Gets all unique quest progress records for a user across all weeks.
        /// Used for one-time quest tracking.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>All quest progress records for one-time quests.</returns>
        Task<IQueryable<WeeklyQuestProgress>> GetAllLifetimeAsync(int userId);

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task SaveChangesAsync();

        /// <summary>
        /// Gets the count of active (not claimed) quest progress records for a given quest ID across all users.
        /// Used to check if a quest can be safely deleted.
        /// </summary>
        /// <param name="questId">The quest identifier.</param>
        /// <returns>Count of active progress records.</returns>
        Task<int> GetActiveProgressCountByQuestIdAsync(string questId);

        /// <summary>
        /// Resets all weekly quest progress records for all users for the given week/year.
        /// Sets IsClaimed = false, Progress = 0, and PointsAwarded = 0.
        /// </summary>
        /// <param name="weekNumber">The ISO week number to reset.</param>
        /// <param name="year">The year.</param>
        /// <returns>Number of records reset.</returns>
        Task<int> ResetAllWeeksAsync(int weekNumber, int year);
    }
}
