using F1BettingApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository for PointHistory entity operations.
    /// </summary>
    public interface IPointHistoryRepository
    {
        /// <summary>
        /// Adds a new point history entry.
        /// </summary>
        Task AddAsync(PointHistory entity);

        /// <summary>
        /// Gets paginated point history for a user, newest first.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Items per page.</param>
        /// <returns>Enumerable of point history entries for the page.</returns>
        Task<IEnumerable<PointHistory>> GetByUserIdAsync(int userId, int page, int pageSize);

        /// <summary>
        /// Gets the total count of point history entries for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>Total count.</returns>
        Task<int> GetCountByUserIdAsync(int userId);

        /// <summary>
        /// Gets the weekly summary (total earned and spent) for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="weekNumber">ISO week number.</param>
        /// <param name="year">Year.</param>
        /// <returns>A tuple of (totalEarned, totalSpent).</returns>
        Task<(int TotalEarned, int TotalSpent)> GetWeeklySummaryAsync(int userId, int weekNumber, int year);
    }
}
