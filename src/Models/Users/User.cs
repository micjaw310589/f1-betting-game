using F1BettingGame.Models.Betting;
using F1BettingGame.Models.Finance;
using F1BettingGame.Models.Notifications;

namespace F1BettingGame.Models.Users;

/// <summary>
/// Użytkownik systemu.
/// Odpowiada tabeli: users
/// </summary>
public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public short RoleId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Skrót hasła (bcrypt / Argon2).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public Role Role { get; set; } = null!;

    public Wallet? Wallet { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = [];

    public ICollection<UserBonus> UserBonuses { get; set; } = [];

    public ICollection<Notification> Notifications { get; set; } = [];

    public ICollection<BetSlip> BetSlips { get; set; } = [];

    public ICollection<CashoutRequest> CashoutRequests { get; set; } = [];
}
