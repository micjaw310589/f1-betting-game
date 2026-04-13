using F1BettingGame.Models.F1;
using F1BettingGame.Models.Results;

namespace F1BettingGame.Models.Standings;

/// <summary>
/// Migawka klasyfikacji kierowców po konkretnym wyścigu.
/// Unikalny rekord na kombinację (race_id, driver_id).
/// Odpowiada tabeli: driver_standings_snapshots
/// </summary>
public class DriverStandingsSnapshot
{
    public int SnapshotId { get; set; }

    public int RaceId { get; set; }

    public int DriverId { get; set; }

    public short SeasonId { get; set; }

    public short Position { get; set; }

    public decimal Points { get; set; }

    public short Wins { get; set; } = 0;

    public short Podiums { get; set; } = 0;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Driver Driver { get; set; } = null!;

    public Season Season { get; set; } = null!;
}
