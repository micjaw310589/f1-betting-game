using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Team entities
/// </summary>
public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(AppDbContext context) : base(context) { }

    public async Task<Team?> GetByOpenF1IdAsync(string openF1TeamId)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.OpenF1TeamId == openF1TeamId);
    }

    public async Task<Team?> GetWithDriversAsync(int teamId)
    {
        return await _dbSet.Include(t => t.Drivers).FirstOrDefaultAsync(t => t.Id == teamId);
    }
}