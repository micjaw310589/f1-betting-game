using F1BettingGame.Models.Enums;
using F1BettingGame.Models.Users;

namespace F1BettingGame.Models.Notifications;

/// <summary>
/// Powiadomienie systemowe dla użytkownika.
/// Odpowiada tabeli: notifications
/// </summary>
public class Notification
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Typ powiadomienia: BetSettled, RaceReminder, OddsChange, Bonus, System.</summary>
    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public bool IsRead { get; set; } = false;

    /// <summary>Opcjonalne UUID powiązanego obiektu (np. BetSlip, Race).</summary>
    public Guid? ReferenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Relacje nawigacyjne ──────────────────────────────────────────────────
    public User User { get; set; } = null!;
}
