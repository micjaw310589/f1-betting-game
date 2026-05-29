using F1BettingApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository interface for QuestDefinition entity operations.
    /// </summary>
    public interface IQuestDefinitionRepository
    {
        /// <summary>
        /// Gets all quest definitions, optionally filtered by active status.
        /// </summary>
        /// <param name="isActive">Optional filter for active quests.</param>
        /// <returns>All matching quest definitions ordered by display order.</returns>
        Task<IQueryable<QuestDefinition>> GetAllAsync(bool? isActive = null);

        /// <summary>
        /// Gets a quest definition by its unique QuestId.
        /// </summary>
        /// <param name="questId">The unique quest identifier.</param>
        /// <returns>The quest definition, or null if not found.</returns>
        Task<QuestDefinition?> GetByQuestIdAsync(string questId);

        /// <summary>
        /// Gets a quest definition by its numeric ID.
        /// </summary>
        /// <param name="id">The numeric ID.</param>
        /// <returns>The quest definition, or null if not found.</returns>
        Task<QuestDefinition?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new quest definition.
        /// </summary>
        /// <param name="quest">The quest definition to create.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task CreateAsync(QuestDefinition quest);

        /// <summary>
        /// Updates an existing quest definition.
        /// </summary>
        /// <param name="quest">The quest definition to update.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateAsync(QuestDefinition quest);

        /// <summary>
        /// Deletes a quest definition by its numeric ID.
        /// Does not affect existing WeeklyQuestProgress records.
        /// </summary>
        /// <param name="id">The numeric ID of the quest to delete.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Saves all pending changes to the database.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task SaveChangesAsync();
    }
}
