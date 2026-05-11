namespace F1BettingApp.Application.DTOs;

/// <summary>
/// Result of a sync operation.
/// </summary>
public class SyncResultDto
{
    /// <summary>
    /// Whether the sync was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of races processed during the sync.
    /// </summary>
    public int RacesProcessed { get; set; }

    /// <summary>
    /// Number of races created (new).
    /// </summary>
    public int RacesCreated { get; set; }

    /// <summary>
    /// Number of races updated.
    /// </summary>
    public int RacesUpdated { get; set; }

    /// <summary>
    /// Error message if the sync failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when the sync was performed.
    /// </summary>
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}
