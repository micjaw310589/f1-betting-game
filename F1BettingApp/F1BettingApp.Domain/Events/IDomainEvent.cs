namespace F1BettingApp.Domain.Events
{
    /// <summary>
    /// Marker interface for domain events
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the timestamp when the event occurred
        /// </summary>
        DateTime EventTimestamp { get; }
    }
}