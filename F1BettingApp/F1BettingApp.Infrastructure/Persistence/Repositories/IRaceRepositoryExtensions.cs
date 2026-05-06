// Extension interface for Race-specific queries
// These methods are used by BettingService but not in the base IRepository<T> interface

using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Extension interface providing domain-specific query operations for races.
    /// </summary>
    public interface IRaceRepositoryExtensions : IRepository<Race>
    {
        /// <summary>
        /// Gets all upcoming races that can accept bets.
        /// </summary>
        Task<IEnumerable<Race>> GetUpcomingRacesAsync();

        /// <summary>
        /// Gets a race with its associated odds for each driver.
        /// </summary>
        Task<Race> GetRaceWithOddsAsync(int id);

        /// <summary>
        /// Checks if bets can be placed on a specific race.
        /// </summary>
        bool CanPlaceBets(Race race);

        /// <summary>
        /// Gets the total amount wagered on a race (nullable).
        /// </summary>
        Task<decimal?> GetTotalAmountWageredAsync(int raceId);

        /// <summary>
        /// Gets the total number of bets placed on a race (nullable).
        /// </summary>
        Task<int?> GetTotalBetsCountAsync(int raceId);

        /// <summary>
        /// Gets races by their IDs.
        /// </summary>
        Task<IEnumerable<Race>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
