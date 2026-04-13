using F1BettingGame.Models.Betting;
using F1BettingGame.Models.Results;
using F1BettingGame.Models.Standings;

namespace F1BettingGame.Models.F1;

/// <summary>
/// Kierowca Formuły 1.
/// Odpowiada tabeli: drivers
/// </summary>
public class Driver
{
    public int DriverId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Nationality { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public short? DriverNumber { get; set; }

    /// <summary>Trzyliterowy skrót nazwiska, np. VER, HAM, LEC.</summary>
    public string? Abbreviation { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>FK do sezonu debiutu kierowcy w F1.</summary>
    public short? DebutSeasonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Season? DebutSeason { get; set; }

    public ICollection<DriverTeamSeason> DriverTeamSeasons { get; set; } = [];

    public ICollection<CircuitRecord> CircuitRecords { get; set; } = [];

    public ICollection<RaceResult> RaceResults { get; set; } = [];

    public ICollection<QualifyingResult> QualifyingResults { get; set; } = [];

    public ICollection<SprintResult> SprintResults { get; set; } = [];

    public ICollection<PracticeResult> PracticeResults { get; set; } = [];

    public ICollection<DriverStandingsSnapshot> DriverStandingsSnapshots { get; set; } = [];

    public ICollection<Odd> Odds { get; set; } = [];
}
