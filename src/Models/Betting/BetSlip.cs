using F1BettingGame.Models.Enums;
using F1BettingGame.Models.Finance;
using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Betting;

/// <summary>
/// Kupon zakładowy złożony przez użytkownika.
/// Odpowiada tabeli: bet_slips
/// </summary>
public class BetSlip
{
    public Guid SlipId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Typ kuponu: Single, Accumulator, System.</summary>
    public BetSlipType Type { get; set; }

    /// <summary>Status kuponu: Pending, Won, Lost, Void, Cashout, PartialWin.</summary>
    public BetSlipStatus Status { get; set; } = BetSlipStatus.Pending;

    public decimal Stake { get; set; }

    public decimal PotentialWin { get; set; }

    public decimal? ActualWin { get; set; }

    /// <summary>Łączny kurs (dla zakładów kombinowanych).</summary>
    public decimal? CombinedOdd { get; set; }

    /// <summary>Opcjonalne FK do bonusu użytkownika zastosowanego przy kuponie.</summary>
    public Guid? UserBonusId { get; set; }

    public decimal? CashoutValue { get; set; }

    public DateTimeOffset PlacedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? SettledAt { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public UserBonus? UserBonus { get; set; }

    public ICollection<BetSelection> Selections { get; set; } = [];

    public ICollection<CashoutRequest> CashoutRequests { get; set; } = [];
}
