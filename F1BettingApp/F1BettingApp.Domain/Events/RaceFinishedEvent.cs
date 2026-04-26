using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Domain.Events
{
    /// <summary>
    /// Domain event raised when a race is finished
    /// </summary>
    public class RaceFinishedEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the race that was finished
        /// </summary>
        public Race Race { get; }

        /// <summary>
        /// Gets the race ID
        /// </summary>
        public int RaceId { get; }

        /// <summary>
        /// Gets the timestamp when the race was finished
        /// </summary>
        public DateTime EventTimestamp { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RaceFinishedEvent"/> class
        /// </summary>
        /// <param name="race">The race that was finished</param>
        public RaceFinishedEvent(Race race)
        {
            Race = race ?? throw new ArgumentNullException(nameof(race));
            RaceId = race.Id;
            EventTimestamp = DateTime.UtcNow;
        }
    }
}