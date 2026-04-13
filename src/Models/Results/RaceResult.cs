using F1BettingGame.Models.Enums;
using F1BettingGame.Models.F1;

namespace F1BettingGame.Models.Results;

/// <summary>
/// Wynik kierowcy w wyścigu.
/// Unikalny rekord na kombinację (race_id, driver_id).
/// Odpowiada tabeli: race_results
/// </summary>
public class RaceResult
{
    public int ResultId { get; set; }

    public int RaceId { get; set; }

    public int DriverId { get; set; }

    public int TeamId { get; set; }

    public short? FinishPosition { get; set; }

    public short? GridPosition { get; set; }

    /// <summary>Punkty zdobyte w wyścigu (z uwzględnieniem bonusu za najszybsze okrążenie).</summary>
    public decimal Points { get; set; } = 0;

    public short? LapsCompleted { get; set; }

    /// <summary>Łączny czas wyścigu (INTERVAL → TimeSpan).</summary>
    public TimeSpan? TotalRaceTime { get; set; }

    /// <summary>Strata do lidera.</summary>
    public TimeSpan? GapToLeader { get; set; }

    public TimeSpan? FastestLapTime { get; set; }

    public short? FastestLapNumber { get; set; }

    public bool IsFastestLap { get; set; } = false;

    /// <summary>Powód nieukończenia: Accident, Mechanical, Dnf, Dns, Dsq.</summary>
    public DnfReason? DnfReason { get; set; }

    /// <summary>Status rekordu wynikowego (np. FINISHED, DNF, DNS, DSQ).</summary>
    public string Status { get; set; } = string.Empty;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Team Team { get; set; } = null!;
}
