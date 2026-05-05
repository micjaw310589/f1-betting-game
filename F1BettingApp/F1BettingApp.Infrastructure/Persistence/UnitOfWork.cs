using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.Persistence
{
    /// <summary>
    /// Implementation of the Unit of Work pattern
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _currentTransaction;

        /// <summary>
        /// Gets the bet repository
        /// </summary>
        public IRepository<Bet> BetRepository { get; }

        /// <summary>
        /// Gets the user repository
        /// </summary>
        public IRepository<User> UserRepository { get; }

        /// <summary>
        /// Gets the race repository
        /// </summary>
        public IRepository<Race> RaceRepository { get; }

        /// <summary>
        /// Gets the result repository
        /// </summary>
        public IRepository<Result> ResultRepository { get; }

        /// <summary>
        /// Gets the notification repository
        /// </summary>
        public IRepository<Notification> NotificationRepository { get; }

        /// <summary>
        /// Gets the leaderboard history repository
        /// </summary>
        public IRepository<LeaderboardHistory> LeaderboardHistoryRepository { get; }

        /// <summary>
        /// Gets the driver repository
        /// </summary>
        public IDriverRepository DriverRepository { get; }

        /// <summary>
        /// Gets the team repository
        /// </summary>
        public ITeamRepository TeamRepository { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="betRepository">The bet repository</param>
        /// <param name="userRepository">The user repository</param>
        /// <param name="raceRepository">The race repository</param>
        /// <param name="resultRepository">The result repository</param>
        /// <param name="notificationRepository">The notification repository</param>
        /// <param name="leaderboardHistoryRepository">The leaderboard history repository</param>
        public UnitOfWork(
            AppDbContext context,
            IRepository<Bet> betRepository,
            IRepository<User> userRepository,
            IRepository<Race> raceRepository,
            IRepository<Result> resultRepository,
            IRepository<Notification> notificationRepository,
            IRepository<LeaderboardHistory> leaderboardHistoryRepository,
            IDriverRepository driverRepository,
            ITeamRepository teamRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            BetRepository = betRepository ?? throw new ArgumentNullException(nameof(betRepository));
            UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            RaceRepository = raceRepository ?? throw new ArgumentNullException(nameof(raceRepository));
            ResultRepository = resultRepository ?? throw new ArgumentNullException(nameof(resultRepository));
            NotificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            LeaderboardHistoryRepository = leaderboardHistoryRepository ?? throw new ArgumentNullException(nameof(leaderboardHistoryRepository));
            DriverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            TeamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        }

        /// <summary>
        /// Commits all changes made in this unit of work
        /// </summary>
        /// <returns>Task representing the number of affected records</returns>
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Rolls back all changes made in this unit of work
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task RollbackAsync()
        {
            // Reset all tracked entities to their original values
            var changedEntries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction is currently active.");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction is currently active.");
            }

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Disposes the unit of work and any active transactions
        /// </summary>
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}