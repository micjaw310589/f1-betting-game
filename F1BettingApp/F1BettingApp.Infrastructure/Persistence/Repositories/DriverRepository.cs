using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Driver entities
/// </summary>
public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(AppDbContext context) : base(context) { }

    public async Task<Driver?> GetByOpenF1IdAsync(string openF1DriverId)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.OpenF1DriverId == openF1DriverId);
    }

    public async Task<IQueryable<Driver>> GetByTeamIdAsync(int teamId)
    {
        return _dbSet.Where(d => d.TeamId == teamId).AsQueryable();
    }

    public async Task<IQueryable<Driver>> GetAllWithTeamAsync()
    {
        return _dbSet.Include(d => d.Team).AsQueryable();
    }
}