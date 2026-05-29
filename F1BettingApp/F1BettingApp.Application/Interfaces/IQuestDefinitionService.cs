using F1BettingApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for quest definition admin operations.
    /// </summary>
    public interface IQuestDefinitionService
    {
        /// <summary>
        /// Gets all quest definitions, optionally filtered by active status.
        /// </summary>
        /// <param name="isActive">Optional filter for active quests.</param>
        /// <returns>List of quest definitions.</returns>
        Task<List<QuestDto>> GetAllQuestDefinitionsAsync(bool? isActive = null);

        /// <summary>
        /// Creates a new quest definition.
        /// </summary>
        /// <param name="dto">The quest creation data.</param>
        /// <returns>The created quest DTO.</returns>
        Task<QuestDto> CreateQuestDefinitionAsync(CreateQuestDto dto);

        /// <summary>
        /// Updates an existing quest definition.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <param name="dto">The quest update data.</param>
        /// <returns>The updated quest DTO.</returns>
        Task<QuestDto> UpdateQuestDefinitionAsync(int id, UpdateQuestDto dto);

        /// <summary>
        /// Deletes a quest definition. Throws if there are active progress records.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteQuestDefinitionAsync(int id);

        /// <summary>
        /// Toggles a quest's active status.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <param name="isActive">The new active status.</param>
        /// <returns>The updated quest DTO.</returns>
        Task<QuestDto> ToggleQuestActiveAsync(int id, bool isActive);

        /// <summary>
        /// Resets all weekly quest progress records for all users to the current week.
        /// Sets IsClaimed = false and Progress = 0 for all active weekly quests.
        /// </summary>
        /// <returns>Number of records reset.</returns>
        Task<int> ResetWeeklyQuestsAsync();
    }
}
