using F1BettingGame.Models.Enums;

namespace F1BettingGame.Models.Finance;

/// <summary>
/// Transakcja finansowa powiązana z portfelem użytkownika.
/// Odpowiada tabeli: transactions
/// </summary>
public class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid WalletId { get; set; }

    /// <summary>Typ transakcji: Deposit, Withdrawal, BetStake, WinPayout, Refund, Bonus, Cashout.</summary>
    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    /// <summary>Opcjonalne UUID powiązanego obiektu (np. BetSlip, CashoutRequest).</summary>
    public Guid? ReferenceId { get; set; }

    public string? Description { get; set; }

    /// <summary>Status transakcji: Pending, Completed, Failed, Reversed.</summary>
    public TransactionStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Wallet Wallet { get; set; } = null!;
}
