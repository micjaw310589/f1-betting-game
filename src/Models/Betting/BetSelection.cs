using F1BettingGame.Models.Enums;

namespace F1BettingGame.Models.Betting;

/// <summary>
/// Pojedyncze zdarzenie (selekcja) w ramach kuponu zakładowego.
/// Odpowiada tabeli: bet_selections
/// </summary>
public class BetSelection
{
    public Guid SelectionId { get; set; }

    public Guid SlipId { get; set; }

    public Guid OddsId { get; set; }

    /// <summary>Kurs w momencie składania zakładu (frozen odd).</summary>
    public decimal OddValueAtBet { get; set; }

    /// <summary>Status selekcji: Pending, Won, Lost, Void, Push.</summary>
    public BetSelectionStatus Status { get; set; } = BetSelectionStatus.Pending;

    public DateTimeOffset? SettledAt { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public BetSlip BetSlip { get; set; } = null!;

    public Odd Odd { get; set; } = null!;
}
