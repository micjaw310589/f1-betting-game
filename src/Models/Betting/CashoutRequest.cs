using F1BettingGame.Models.Enums;
using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Betting;

/// <summary>
/// Żądanie wcześniejszego wypłacenia środków z kuponu (cashout).
/// Odpowiada tabeli: cashout_requests
/// </summary>
public class CashoutRequest
{
    public Guid CashoutId { get; set; }

    public Guid SlipId { get; set; }

    public Guid UserId { get; set; }

    public decimal CashoutValue { get; set; }

    /// <summary>Status żądania: Requested, Approved, Rejected.</summary>
    public CashoutRequestStatus Status { get; set; }

    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public BetSlip BetSlip { get; set; } = null!;

    public User User { get; set; } = null!;
}
