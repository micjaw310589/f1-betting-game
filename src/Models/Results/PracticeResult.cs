using F1BettingGame.Models.F1;

namespace F1BettingGame.Models.Results;

/// <summary>
/// Wynik kierowcy w sesji treningowej (FP1, FP2, FP3).
/// Unikalny rekord na kombinację (race_id, session_number, driver_id).
/// Odpowiada tabeli: practice_results
/// </summary>
public class PracticeResult
{
    public int PracticeResultId { get; set; }

    public int RaceId { get; set; }

    /// <summary>Numer sesji treningowej: 1 = FP1, 2 = FP2, 3 = FP3.</summary>
    public short SessionNumber { get; set; }

    public int DriverId { get; set; }

    public int TeamId { get; set; }

    public short? Position { get; set; }

    public TimeSpan? BestLapTime { get; set; }

    public short? LapsCompleted { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Team Team { get; set; } = null!;
}
