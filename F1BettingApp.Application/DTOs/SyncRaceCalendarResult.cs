namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Result of a race calendar sync operation.
    /// </summary>
    public class SyncRaceCalendarResult
    {
        public int TotalCount { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
    }
}