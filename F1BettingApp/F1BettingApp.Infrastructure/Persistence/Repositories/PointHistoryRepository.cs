using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for PointHistory entity operations.
    /// </summary>
    public class PointHistoryRepository : IPointHistoryRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<PointHistory> _dbSet;

        public PointHistoryRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<PointHistory>();
        }

        /// <inheritdoc />
        public async Task AddAsync(PointHistory entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PointHistory>> GetByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _dbSet.CountAsync(ph => ph.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<(int TotalEarned, int TotalSpent)> GetWeeklySummaryAsync(int userId, int weekNumber, int year)
        {
            // Calculate the start and end of the ISO week
            // We use a simplified approach: find all entries for the user in the given week
            // The week boundaries are computed from the ISO week number
            var (startDate, endDate) = GetIsoWeekBounds(weekNumber, year);

            var summary = await _dbSet
                .Where(ph => ph.UserId == userId
                          && ph.CreatedAt >= startDate
                          && ph.CreatedAt < endDate)
                .GroupBy(ph => 1)
                .Select(g => new
                {
                    TotalEarned = g.Sum(ph => ph.Points > 0 ? ph.Points : 0),
                    TotalSpent = Math.Abs(g.Sum(ph => ph.Points < 0 ? ph.Points : 0))
                })
                .FirstOrDefaultAsync();

            return (summary?.TotalEarned ?? 0, summary?.TotalSpent ?? 0);
        }

        private static (DateTime StartDate, DateTime EndDate) GetIsoWeekBounds(int weekNumber, int year)
        {
            // ISO week 1 is the week containing the first Thursday of the year
            // The Monday of ISO week 1 is found by going to Jan 4 and finding the preceding Monday
            var jan4 = new DateTime(year, 1, 4);
            var dayOfWeek = jan4.DayOfWeek;
            var mondayOfWeek1 = jan4.AddDays(-(dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));

            var startDate = mondayOfWeek1.AddDays((weekNumber - 1) * 7);
            var endDate = startDate.AddDays(7);

            return (startDate, endDate);
        }
    }
}
