using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Betting;

/// <summary>
/// Historia zmian kursu zakładu.
/// Odpowiada tabeli: odds_history
/// </summary>
public class OddsHistory
{
    public long HistoryId { get; set; }

    public Guid OddsId { get; set; }

    public int RaceId { get; set; }

    public short BetTypeId { get; set; }

    public int? DriverId { get; set; }

    public int? TeamId { get; set; }

    public decimal OldValue { get; set; }

    public decimal NewValue { get; set; }

    public string? ChangeReason { get; set; }

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>FK do użytkownika (operatora), który zmienił kurs.</summary>
    public Guid ChangedBy { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Odd Odd { get; set; } = null!;

    public User ChangedByUser { get; set; } = null!;
}
