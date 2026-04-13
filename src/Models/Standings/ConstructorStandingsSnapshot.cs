using F1BettingGame.Models.F1;
using F1BettingGame.Models.Results;

namespace F1BettingGame.Models.Standings;

/// <summary>
/// Migawka klasyfikacji konstruktorów po konkretnym wyścigu.
/// Unikalny rekord na kombinację (race_id, team_id).
/// Odpowiada tabeli: constructor_standings_snapshots
/// </summary>
public class ConstructorStandingsSnapshot
{
    public int SnapshotId { get; set; }

    public int RaceId { get; set; }

    public int TeamId { get; set; }

    public short SeasonId { get; set; }

    public short Position { get; set; }

    public decimal Points { get; set; }

    public short Wins { get; set; } = 0;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public Team Team { get; set; } = null!;

    public Season Season { get; set; } = null!;
}
