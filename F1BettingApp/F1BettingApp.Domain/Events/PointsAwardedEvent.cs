using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Domain.Events
{
    /// <summary>
    /// Domain event raised when points are awarded to a user
    /// </summary>
    public class PointsAwardedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the user ID who received points
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Gets the amount of points awarded
        /// </summary>
        public int Points { get; }

        /// <summary>
        /// Gets the reason for the points award
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the timestamp when points were awarded
        /// </summary>
        public DateTime EventTimestamp { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointsAwardedEvent"/> class
        /// </summary>
        /// <param name="userId">The user ID who received points</param>
        /// <param name="points">The amount of points awarded</param>
        /// <param name="reason">The reason for the points award</param>
        public PointsAwardedEvent(int userId, int points, string reason)
        {
            UserId = userId;
            Points = points;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            EventTimestamp = DateTime.UtcNow;
        }
    }
}