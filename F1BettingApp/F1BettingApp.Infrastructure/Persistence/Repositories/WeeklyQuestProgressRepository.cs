using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for WeeklyQuestProgress entity operations.
    /// </summary>
    public class WeeklyQuestProgressRepository : IWeeklyQuestProgressRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<WeeklyQuestProgress> _dbSet;

        public WeeklyQuestProgressRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<WeeklyQuestProgress>();
        }

        public async Task<IQueryable<WeeklyQuestProgress>> GetAllAsync(int userId, int? weekNumber = null, int? year = null)
        {
            var query = _dbSet.AsQueryable().Where(p => p.UserId == userId);

            if (weekNumber.HasValue)
            {
                query = query.Where(p => p.WeekNumber == weekNumber.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(p => p.Year == year.Value);
            }

            return query;
        }

        public async Task<WeeklyQuestProgress?> GetAsync(int userId, string questId, int weekNumber, int year)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.QuestId == questId &&
                    p.WeekNumber == weekNumber &&
                    p.Year == year);
        }

        public async Task<IQueryable<WeeklyQuestProgress>> GetActiveAsync(int userId)
        {
            return _dbSet
                .Where(p => p.UserId == userId && !p.IsClaimed)
                .OrderBy(p => p.UpdatedAt);
        }

        public async Task UpsertAsync(WeeklyQuestProgress progress)
        {
            var existing = await _dbSet
                .FirstOrDefaultAsync(p =>
                    p.UserId == progress.UserId &&
                    p.QuestId == progress.QuestId &&
                    p.WeekNumber == progress.WeekNumber &&
                    p.Year == progress.Year);

            if (existing != null)
            {
                existing.Progress = progress.Progress;
                existing.Target = progress.Target;
                existing.IsCompleted = progress.IsCompleted;
                existing.PointsAwarded = progress.PointsAwarded;
                existing.IsClaimed = progress.IsClaimed;
                existing.UpdatedAt = progress.UpdatedAt;
            }
            else
            {
                await _dbSet.AddAsync(progress);
            }

            await SaveChangesAsync();
        }

        public async Task ResetWeekAsync(int userId, int weekNumber, int year)
        {
            // Only reset INCOMPLETE progress records; preserve completed and claimed quests
            var records = await _dbSet
                .Where(p => p.UserId == userId && p.WeekNumber == weekNumber && p.Year == year && !p.IsCompleted)
                .ToListAsync();

            foreach (var record in records)
            {
                record.Progress = 0;
                record.IsCompleted = false;
                record.PointsAwarded = 0;
                record.IsClaimed = false;
                record.UpdatedAt = DateTime.UtcNow;
            }

            await SaveChangesAsync();
        }

        public async Task<IQueryable<WeeklyQuestProgress>> GetAllLifetimeAsync(int userId)
        {
            // Return all records for one-time quests (week 0, year 0)
            return _dbSet
                .Where(p => p.UserId == userId && p.WeekNumber == 0 && p.Year == 0);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetActiveProgressCountByQuestIdAsync(string questId)
        {
            return await _dbSet
                .Where(p => p.QuestId == questId && !p.IsClaimed)
                .CountAsync();
        }

        public async Task<int> ResetAllWeeksAsync(int weekNumber, int year)
        {
            // Only reset INCOMPLETE progress records; preserve completed and claimed quests
            var records = await _dbSet
                .Where(p => p.WeekNumber == weekNumber && p.Year == year && !p.IsCompleted)
                .ToListAsync();

            foreach (var record in records)
            {
                record.Progress = 0;
                record.IsCompleted = false;
                record.PointsAwarded = 0;
                record.IsClaimed = false;
                record.UpdatedAt = DateTime.UtcNow;
            }

            await SaveChangesAsync();
            return records.Count;
        }

        public async Task<int> GetCompletedCountByQuestIdAsync(string questId)
        {
            return await _dbSet
                .Where(p => p.QuestId == questId && p.IsClaimed)
                .CountAsync();
        }
    }
}
