using F1BettingGame.Models.F1;
using F1BettingGame.Models.Results;
using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Betting;

/// <summary>
/// Aktualny kurs zakładu dla danego wyścigu, typu zakładu i opcjonalnie kierowcy/zespołu.
/// Unikalny rekord na kombinację (race_id, bet_type_id, driver_id, team_id).
/// Odpowiada tabeli: odds
/// </summary>
public class Odd
{
    public Guid OddsId { get; set; }

    public int RaceId { get; set; }

    public short BetTypeId { get; set; }

    /// <summary>Opcjonalne FK do kierowcy — gdy zakład dotyczy konkretnego kierowcy.</summary>
    public int? DriverId { get; set; }

    /// <summary>Opcjonalne FK do zespołu — gdy zakład dotyczy konkretnego zespołu.</summary>
    public int? TeamId { get; set; }

    public decimal OddValue { get; set; }

    /// <summary>Marża bukmachera w procentach.</summary>
    public decimal? MarginPct { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ValidTo { get; set; }

    /// <summary>FK do użytkownika (operatora), który wystawił kurs.</summary>
    public Guid CreatedBy { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Race Race { get; set; } = null!;

    public BetType BetType { get; set; } = null!;

    public Driver? Driver { get; set; }

    public Team? Team { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public ICollection<OddsHistory> History { get; set; } = [];

    public ICollection<BetSelection> BetSelections { get; set; } = [];
}
