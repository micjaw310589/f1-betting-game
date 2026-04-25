namespace F1BettingApp.Domain.Entities;

public class LeaderboardHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int RaceId { get; set; }
    public Race? Race { get; set; }
    public int Season { get; set; }
    public int TotalPoints { get; set; }
    public int Rank { get; set; }
    public DateTime CreatedAt { get; set; }
}