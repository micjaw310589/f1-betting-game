using F1BettingGame.Models.F1;

namespace F1BettingGame.Models.Results;

/// <summary>
/// Wynik kierowcy w kwalifikacjach.
/// Unikalny rekord na kombinację (race_id, driver_id).
/// Odpowiada tabeli: qualifying_results
/// </summary>
public class QualifyingResult
{
    public int QualiId { get; set; }

    public int RaceId { get; set; }

    public int DriverId { get; set; }

    public int TeamId { get; set; }

    public short? Position { get; set; }

    public TimeSpan? Q1Time { get; set; }

    public TimeSpan? Q2Time { get; set; }

    public TimeSpan? Q3Time { get; set; }

    public TimeSpan? BestTime { get; set; }

    /// <summary>
    /// Segment, w którym kierowca odpadł.
    /// null = dotarł do Q3, 1 = odpadł w Q1, 2 = odpadł w Q2.
    /// </summary>
    public short? EliminatedIn { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Team Team { get; set; } = null!;
}
