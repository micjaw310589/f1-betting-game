using F1BettingGame.Models.Betting;
using F1BettingGame.Models.Results;
using F1BettingGame.Models.Standings;

namespace F1BettingGame.Models.F1;

/// <summary>
/// Zespół (konstruktor) Formuły 1.
/// Odpowiada tabeli: teams
/// </summary>
public class Team
{
    public int TeamId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? FullName { get; set; }

    /// <summary>Trzyliterowy kod skrócony, np. RBR, MER, FER.</summary>
    public string? ShortCode { get; set; }

    public string? Nationality { get; set; }

    public string? BaseCity { get; set; }

    public short? FoundedYear { get; set; }

    public string? EngineSupplier { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public ICollection<DriverTeamSeason> DriverTeamSeasons { get; set; } = [];

    public ICollection<Car> Cars { get; set; } = [];

    public ICollection<RaceResult> RaceResults { get; set; } = [];

    public ICollection<QualifyingResult> QualifyingResults { get; set; } = [];

    public ICollection<SprintResult> SprintResults { get; set; } = [];

    public ICollection<PracticeResult> PracticeResults { get; set; } = [];

    public ICollection<ConstructorStandingsSnapshot> ConstructorStandingsSnapshots { get; set; } = [];

    public ICollection<Odd> Odds { get; set; } = [];
}
