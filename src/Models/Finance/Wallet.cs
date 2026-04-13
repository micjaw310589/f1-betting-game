using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Finance;

/// <summary>
/// Portfel użytkownika — saldo i waluta.
/// Odpowiada tabeli: wallets
/// </summary>
public class Wallet
{
    public Guid WalletId { get; set; }

    /// <summary>Klucz obcy do użytkownika (relacja 1:1).</summary>
    public Guid UserId { get; set; }

    /// <summary>Aktualne saldo konta.</summary>
    public decimal Balance { get; set; } = 0.00m;

    /// <summary>Trzyliterowy kod waluty ISO 4217, np. PLN, EUR.</summary>
    public string Currency { get; set; } = "PLN";

    public DateTimeOffset? UpdatedAt { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = [];
}
