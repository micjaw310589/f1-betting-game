using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Domain.Events
{
    /// <summary>
    /// Domain event raised when a bet is placed
    /// </summary>
    public class BetPlacedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the bet that was placed
        /// </summary>
        public Bet Bet { get; }

        /// <summary>
        /// Gets the user ID who placed the bet
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Gets the race ID the bet was placed on
        /// </summary>
        public int RaceId { get; }

        /// <summary>
        /// Gets the amount of the bet
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Gets the timestamp when the bet was placed
        /// </summary>
        public DateTime EventTimestamp { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetPlacedEvent"/> class
        /// </summary>
        /// <param name="bet">The bet that was placed</param>
        /// <param name="userId">The user ID who placed the bet</param>
        /// <param name="raceId">The race ID the bet was placed on</param>
        /// <param name="amount">The amount of the bet</param>
        public BetPlacedEvent(Bet bet, int userId, int raceId, decimal amount)
        {
            Bet = bet ?? throw new ArgumentNullException(nameof(bet));
            UserId = userId;
            RaceId = raceId;
            Amount = amount;
            EventTimestamp = DateTime.UtcNow;
        }
    }
}
