using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories;

/// <summary>
/// Specific repository interface for Team entities
/// </summary>
public interface ITeamRepository : IRepository<Team>
{
    /// <summary>
    /// Gets a team by their OpenF1 API ID
    /// </summary>
    Task<Team?> GetByOpenF1IdAsync(string openF1TeamId);

    /// <summary>
    /// Gets a team with its drivers
    /// </summary>
    Task<Team?> GetWithDriversAsync(int teamId);
}