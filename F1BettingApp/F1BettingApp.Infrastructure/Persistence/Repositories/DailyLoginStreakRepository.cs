using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for DailyLoginStreak entity operations.
    /// </summary>
    public class DailyLoginStreakRepository : IDailyLoginStreakRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<DailyLoginStreak> _dbSet;

        public DailyLoginStreakRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<DailyLoginStreak>();
        }

        public async Task<DailyLoginStreak?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _dbSet.AnyAsync(s => s.UserId == userId);
        }

        public async Task UpsertAsync(DailyLoginStreak streak)
        {
            var existing = await _dbSet.FirstOrDefaultAsync(s => s.UserId == streak.UserId);

            if (existing != null)
            {
                existing.CurrentStreak = streak.CurrentStreak;
                existing.LastLoginDate = streak.LastLoginDate;
                existing.ClaimedToday = streak.ClaimedToday;
                existing.UpdatedAt = streak.UpdatedAt;
            }
            else
            {
                await _dbSet.AddAsync(streak);
            }

            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
