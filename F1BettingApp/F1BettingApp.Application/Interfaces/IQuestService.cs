using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for weekly quest operations.
    /// Handles quest progress tracking, completion evaluation, and point awards.
    /// </summary>
    public interface IQuestService
    {
        /// <summary>
        /// Gets all active quest definitions with the user's current progress.
        /// For recurring quests, uses the current ISO week. For one-time quests, aggregates lifetime progress.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>List of quest DTOs with progress information.</returns>
        Task<QuestResponseDto> GetActiveQuestsAsync(int userId);

        /// <summary>
        /// Gets the quest board progress for a single quest for a single user.
        /// Returns null if the user is not authenticated or quest not found.
        /// </summary>
        /// <param name="questId">The quest identifier.</param>
        /// <param name="userId">The user ID (nullable for unauthenticated requests).</param>
        /// <returns>QuestBoardDto with progress, or null if not found.</returns>
        Task<QuestBoardDto?> GetQuestBoardProgressAsync(string questId, int? userId);

        /// <summary>
        /// Evaluates all active quests for the user, awards points for newly completed ones.
        /// Called at weekly reset and at specific action triggers.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task CheckAndCompleteQuestsAsync(int userId);

        /// <summary>
        /// Increments progress for a quest. Awards points immediately if target is reached.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="questId">The quest identifier.</param>
        /// <param name="amount">Amount to increment progress by.</param>
        /// <param name="additionalContext">Optional context for special tracking (e.g., date for consistent_bettor).</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateQuestProgressAsync(int userId, string questId, int amount, string? additionalContext = null);

        /// <summary>
        /// Gets all active quest definitions by category.
        /// </summary>
        /// <param name="category">The quest category (e.g., "Betting", "Engagement", "Achievement").</param>
        /// <returns>List of active quest definitions in the specified category.</returns>
        Task<List<QuestDto>> GetAllActiveByCategoryAsync(string category);

        /// <summary>
        /// Updates progress for active quests in a category that are triggered by a specific event type.
        /// This allows category-based flows such as bet placement, race views, and login activity to trigger quests.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="category">The quest category to evaluate.</param>
        /// <param name="eventType">The trigger event type (e.g. "BetPlaced", "Login", "RaceViewed", "BetWon").</param>
        /// <param name="amount">The amount to increment progress by.</param>
        /// <param name="additionalContext">Optional context such as race ID or date for special quests.</param>
        Task UpdateQuestProgressByCategoryEventAsync(int userId, string category, string eventType, int amount = 1, string? additionalContext = null);

        /// <summary>
        /// Gets a single quest definition by its QuestId.
        /// </summary>
        /// <param name="questId">The quest identifier.</param>
        /// <returns>The quest definition DTO, or null if not found.</returns>
        Task<QuestDto?> GetQuestDefinitionAsync(string questId);

        /// <summary>
        /// Checks if a given date falls on a race weekend (Friday, Saturday, or Sunday).
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>True if the date is a race weekend day.</returns>
        bool IsRaceWeekendDay(System.DateTime date);
    }
}
