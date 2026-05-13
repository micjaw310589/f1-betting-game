namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Result of a sync operation.
    /// </summary>
    public class SyncResultDto
    {
        public bool Success { get; set; }
        public int RacesProcessed { get; set; }
        public int RacesCreated { get; set; }
        public int RacesUpdated { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SyncedAt { get; set; }
    }
}