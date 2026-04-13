using F1BettingGame.Models.F1;

namespace F1BettingGame.Models.Results;

/// <summary>
/// Wynik kierowcy w wyścigu sprinterskim.
/// Unikalny rekord na kombinację (race_id, driver_id).
/// Odpowiada tabeli: sprint_results
/// </summary>
public class SprintResult
{
    public int SprintResultId { get; set; }

    public int RaceId { get; set; }

    public int DriverId { get; set; }

    public int TeamId { get; set; }

    public short? FinishPosition { get; set; }

    public short? GridPosition { get; set; }

    public decimal Points { get; set; } = 0;

    /// <summary>Status wynikowy (np. FINISHED, DNF).</summary>
    public string Status { get; set; } = string.Empty;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Team Team { get; set; } = null!;
}
