// // Extension interface for Bet-specific queries
// // These methods are used by BettingService but not in the base IRepository<T> interface

// using F1BettingApp.Domain.Entities;

// namespace F1BettingApp.Infrastructure.Persistence.Repositories
// {
//     /// <summary>
//     /// Extension interface providing domain-specific query operations for bets.
//     /// </summary>
//     public interface IBetRepositoryExtensions : IRepository<Bet>
//     {
//         /// <summary>
//         /// Gets all bets placed by a specific user.
//         /// </summary>
//         Task<IList<Bet>> GetByUserIdAsync(int userId);

//         /// <summary>
//         /// Gets all bets for a specific race.
//         /// </summary>
//         Task<IList<Bet>> GetByRaceIdAsync(int raceId);

//         /// <summary>
//         /// Gets active (non-resolved) bets for a user on an upcoming race.
//         /// </summary>
//         Task<IList<Bet>> GetUserActiveBetsAsync(int userId, int raceId);

//         /// <summary>
//         /// Checks if a user has any active bets on a specific driver in a race.
//         /// </summary>
//         Task<bool> HasActiveBetOnDriverAsync(int userId, int raceId, int driverId);

//         /// <summary>
//         /// Gets the total amount wagered by a user on a specific driver.
//         /// </summary>
//         Task<decimal> GetUserTotalWagerOnDriverAsync(int userId, int raceId, int driverId);
//     }
// }