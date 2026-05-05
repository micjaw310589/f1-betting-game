using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Specific repository interface for Driver entities
/// </summary>
public interface IDriverRepository : IRepository<Driver>
{
    /// <summary>
    /// Gets a driver by their OpenF1 API ID
    /// </summary>
    Task<Driver?> GetByOpenF1IdAsync(string openF1DriverId);

    /// <summary>
    /// Gets all drivers for a specific team
    /// </summary>
    Task<IQueryable<Driver>> GetByTeamIdAsync(int teamId);

    /// <summary>
    /// Gets all drivers with their team information
    /// </summary>
    Task<IQueryable<Driver>> GetAllWithTeamAsync();
}