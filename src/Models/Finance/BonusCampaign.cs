using F1BettingGame.Models.Enums;

namespace F1BettingGame.Models.Finance;

/// <summary>
/// Kampania bonusowa dostępna w systemie.
/// Odpowiada tabeli: bonus_campaigns
/// </summary>
public class BonusCampaign
{
    public Guid BonusId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Typ bonusu: Welcome, Deposit, FreeBet, Cashback.</summary>
    public BonusType Type { get; set; }

    /// <summary>Wartość bonusu (kwota lub procent).</summary>
    public decimal Value { get; set; }

    /// <summary>Minimalna kwota depozytu wymagana do aktywacji bonusu.</summary>
    public decimal? MinDeposit { get; set; }

    /// <summary>Mnożnik obrotu (wagering requirement).</summary>
    public short? WageringMultiplier { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public ICollection<UserBonus> UserBonuses { get; set; } = [];
}
