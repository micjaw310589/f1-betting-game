using F1BettingGame.Models.Results;
using F1BettingGame.Models.Standings;

namespace F1BettingGame.Models.F1;

/// <summary>
/// Sezon Formuły 1.
/// Odpowiada tabeli: seasons
/// </summary>
public class Season
{
    public short SeasonId { get; set; }

    public short Year { get; set; }

    public bool IsActive { get; set; } = false;

    /// <summary>FK do kierowcy — mistrz sezonu (uzupełniany po zakończeniu).</summary>
    public int? ChampionDriverId { get; set; }

    /// <summary>FK do zespołu — mistrz konstruktorów (uzupełniany po zakończeniu).</summary>
    public int? ChampionTeamId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Driver? ChampionDriver { get; set; }

    public Team? ChampionTeam { get; set; }

    public ICollection<Race> Races { get; set; } = [];

    public ICollection<DriverTeamSeason> DriverTeamSeasons { get; set; } = [];

    public ICollection<Car> Cars { get; set; } = [];

    public ICollection<CircuitRecord> CircuitRecords { get; set; } = [];

    public ICollection<DriverStandingsSnapshot> DriverStandingsSnapshots { get; set; } = [];

    public ICollection<ConstructorStandingsSnapshot> ConstructorStandingsSnapshots { get; set; } = [];
}
