using F1BettingGame.Models.Betting;
using F1BettingGame.Models.Enums;
using F1BettingGame.Models.F1;
using F1BettingGame.Models.Standings;

namespace F1BettingGame.Models.Results;

/// <summary>
/// Wyścig (weekend GP) w danym sezonie.
/// Unikalny rekord na kombinację (season_id, round_number).
/// Odpowiada tabeli: races
/// </summary>
public class Race
{
    public int RaceId { get; set; }

    public short SeasonId { get; set; }

    public short CircuitId { get; set; }

    public short RoundNumber { get; set; }

    public string OfficialName { get; set; } = string.Empty;

    public DateTimeOffset? RaceDatetime { get; set; }

    public DateTimeOffset? QualifyingDatetime { get; set; }

    public DateTimeOffset? SprintDatetime { get; set; }

    public DateTimeOffset? Fp1Datetime { get; set; }

    public DateTimeOffset? Fp2Datetime { get; set; }

    public DateTimeOffset? Fp3Datetime { get; set; }

    /// <summary>Status wyścigu: Scheduled, Completed, Cancelled, Postponed.</summary>
    public RaceStatus Status { get; set; } = RaceStatus.Scheduled;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Season Season { get; set; } = null!;

    public Circuit Circuit { get; set; } = null!;

    public ICollection<RaceResult> RaceResults { get; set; } = [];

    public ICollection<QualifyingResult> QualifyingResults { get; set; } = [];

    public ICollection<SprintResult> SprintResults { get; set; } = [];

    public ICollection<PracticeResult> PracticeResults { get; set; } = [];

    public ICollection<DriverStandingsSnapshot> DriverStandingsSnapshots { get; set; } = [];

    public ICollection<ConstructorStandingsSnapshot> ConstructorStandingsSnapshots { get; set; } = [];

    public ICollection<Odd> Odds { get; set; } = [];
}
