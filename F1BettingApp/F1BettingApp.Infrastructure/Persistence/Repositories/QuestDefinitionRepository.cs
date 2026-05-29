using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for QuestDefinition entity operations.
    /// </summary>
    public class QuestDefinitionRepository : IQuestDefinitionRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<QuestDefinition> _dbSet;

        public QuestDefinitionRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<QuestDefinition>();
        }

        public async Task<IQueryable<QuestDefinition>> GetAllAsync(bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(q => q.IsActive == isActive.Value);
            }

            return query.OrderBy(q => q.Order);
        }

        public async Task<QuestDefinition?> GetByQuestIdAsync(string questId)
        {
            return await _dbSet.FirstOrDefaultAsync(q => q.QuestId == questId);
        }

        public async Task<QuestDefinition?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task CreateAsync(QuestDefinition quest)
        {
            await _dbSet.AddAsync(quest);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(QuestDefinition quest)
        {
            _dbSet.Update(quest);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var quest = await GetByIdAsync(id);
            if (quest != null)
            {
                _dbSet.Remove(quest);
                await SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
