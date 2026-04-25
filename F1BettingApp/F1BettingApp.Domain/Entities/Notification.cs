using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void MarkAsRead()
    {
        IsRead = true;
    }
}