using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IQueryable<T>> GetAllAsync()
        {
            return _dbSet.AsQueryable();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.FindAsync(id) != null;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        public async Task AddAsyncNoBatch(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Updates an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        public async Task UpdateAsyncNoBatch(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask; // Just to make it async for consistency
        }

        /// <summary>
        /// Deletes an entity without immediately saving changes. Use SaveChangesAsync() to persist.
        /// Useful for batch operations where you want to save once at the end.
        /// </summary>
        public async Task DeleteAsyncNoBatch(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }
    }
}

