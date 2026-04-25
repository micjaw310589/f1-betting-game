using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public IQueryable<User> GetLeaderboard(int limit, int season)
        {
            if (limit <= 0)
            {
                limit = 10;
            }

            var leaderboardUsers = _context.LeaderboardHistories
                .Where(l => l.Season == season)
                .OrderBy(l => l.Rank)
                .Include(l => l.User)
                .Take(limit)
                .Select(l => l.User!)
                .Where(u => u != null);

            if (leaderboardUsers.Any())
            {
                return leaderboardUsers;
            }

            return _dbSet
                .OrderByDescending(u => u.Points)
                .Take(limit);
        }
    }
}
