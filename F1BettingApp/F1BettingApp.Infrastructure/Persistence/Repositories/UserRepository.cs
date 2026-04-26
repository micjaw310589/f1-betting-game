using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation($"Getting user by email: {email}");
                return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by email: {email}");
                throw;
            }
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation($"Getting user by username: {username}");
                return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by username: {username}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetLeaderboardAsync(int limit)
        {
            try
            {
                _logger.LogInformation($"Getting leaderboard with limit: {limit}");
                return await _dbSet
                    .OrderByDescending(u => u.Points)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting leaderboard with limit: {limit}");
                throw;
            }
        }
    }
}