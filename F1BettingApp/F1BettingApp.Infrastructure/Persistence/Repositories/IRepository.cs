using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IQueryable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();

        /// <summary>
        /// Adds an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        Task AddAsyncNoBatch(T entity);

        /// <summary>
        /// Updates an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        Task UpdateAsyncNoBatch(T entity);

        /// <summary>
        /// Deletes an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        Task DeleteAsyncNoBatch(int id);
    }
}
