using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence
{
    /// <summary>
    /// Interface for the Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the bet repository
        /// </summary>
        IRepository<Bet> BetRepository { get; }

        /// <summary>
        /// Gets the user repository
        /// </summary>
        IRepository<User> UserRepository { get; }

        /// <summary>
        /// Gets the race repository
        /// </summary>
        IRepository<Race> RaceRepository { get; }

        /// <summary>
        /// Gets the result repository
        /// </summary>
        IRepository<Result> ResultRepository { get; }

        /// <summary>
        /// Gets the notification repository
        /// </summary>
        IRepository<Notification> NotificationRepository { get; }

        /// <summary>
        /// Gets the leaderboard history repository
        /// </summary>
        IRepository<LeaderboardHistory> LeaderboardHistoryRepository { get; }

        /// <summary>
        /// Commits all changes made in this unit of work
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task<int> CommitAsync();

        /// <summary>
        /// Rolls back all changes made in this unit of work
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RollbackAsync();

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RollbackTransactionAsync();
    }
}