namespace F1BettingApp.Domain.Enums
{
    /// <summary>
    /// Defines the types of notifications that can be generated within the betting application.
    /// </summary>
    public enum NotificationType
    {
        BetPlaced,
        BetWon,
        BetLost,
        RaceResultProcessed,
        SystemMessage
    }
}