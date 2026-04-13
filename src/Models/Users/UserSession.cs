namespace F1BettingGame.Models.Users;

/// <summary>
/// Sesja użytkownika — przechowuje refresh token i dane o urządzeniu.
/// Odpowiada tabeli: user_sessions
/// </summary>
public class UserSession
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string RefreshTokenHash { get; set; } = string.Empty;

    /// <summary>Adres IP w notacji tekstowej (IPv4 / IPv6).</summary>
    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public User User { get; set; } = null!;
}
