namespace F1BettingApp.Domain.ValueObjects;

public class BetStatistics
{
    public int UserId { get; set; }
    public int TotalBets { get; set; }
    public int WonBets { get; set; }
    public int LostBets { get; set; }
    public int PendingBets { get; set; }
    public decimal TotalStaked { get; set; }
    public decimal TotalPotentialWinnings { get; set; }
    public decimal TotalWinnings { get; set; }
}