using F1BettingGame.Models.Enums;
using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Finance;

/// <summary>
/// Bonus przyznany konkretnemu użytkownikowi w ramach kampanii.
/// Odpowiada tabeli: user_bonuses
/// </summary>
public class UserBonus
{
    public Guid UserBonusId { get; set; }

    public Guid UserId { get; set; }

    public Guid BonusId { get; set; }

    public DateTimeOffset AwardedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Status bonusu: Active, Used, Expired, Cancelled.</summary>
    public UserBonusStatus Status { get; set; }

    /// <summary>Pozostała kwota do wykorzystania (jeśli dotyczy).</summary>
    public decimal? RemainingAmount { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public BonusCampaign BonusCampaign { get; set; } = null!;
}
