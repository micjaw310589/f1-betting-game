using F1BettingGame.Models.Enums;

namespace F1BettingGame.Models.F1;

/// <summary>
/// Przypisanie kierowcy do zespołu w danym sezonie.
/// Unikalny rekord na kombinację (driver_id, season_id).
/// Odpowiada tabeli: driver_team_seasons
/// </summary>
public class DriverTeamSeason
{
    public int DtsId { get; set; }

    public int DriverId { get; set; }

    public int TeamId { get; set; }

    public short SeasonId { get; set; }

    public short? CarNumber { get; set; }

    /// <summary>Rola: RaceDriver, Reserve, Test. Domyślnie RaceDriver.</summary>
    public DriverTeamRole Role { get; set; } = DriverTeamRole.RaceDriver;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Driver Driver { get; set; } = null!;

    public Team Team { get; set; } = null!;

    public Season Season { get; set; } = null!;
}
