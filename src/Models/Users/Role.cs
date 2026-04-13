using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Users;

/// <summary>
/// Rola użytkownika w systemie (np. Admin, User, Operator).
/// Odpowiada tabeli: roles
/// </summary>
public class Role
{
    public short RoleId { get; set; }

    /// <summary>Unikalna nazwa roli.</summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public ICollection<User> Users { get; set; } = [];
}
