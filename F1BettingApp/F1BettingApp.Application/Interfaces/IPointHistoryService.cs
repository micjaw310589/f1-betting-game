using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Service for recording and querying point history (earnings and spending).
    /// </summary>
    public interface IPointHistoryService
    {
        /// <summary>
        /// Records a point change (earning or spending) in the history.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="points">Positive for earned, negative for spent.</param>
        /// <param name="category">Category (e.g. "DailyLogin", "Quest", "BetWin", "BetLoss", "BetPlacement", "BetCancellation", "AdminAdjustment").</param>
        /// <param name="description">Human-readable description.</param>
        /// <param name="source">Source ("System", "Admin", "Bet").</param>
        /// <param name="referenceId">Optional reference ID (e.g. bet id, quest id).</param>
        Task RecordPointChangeAsync(int userId, int points, string category, string description, string source, int? referenceId = null);

        /// <summary>
        /// Gets paginated point history for a user, newest first.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Items per page.</param>
        /// <returns>Paginated point history response.</returns>
        Task<PointHistoryResponseDto> GetUserPointHistoryAsync(int userId, int page, int pageSize);

        /// <summary>
        /// Gets the weekly point summary (total earned and spent) for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="weekNumber">ISO week number.</param>
        /// <param name="year">Year.</param>
        /// <returns>Weekly point summary.</returns>
        Task<WeeklyPointSummaryDto> GetWeeklyPointSummaryAsync(int userId, int weekNumber, int year);
    }
}
