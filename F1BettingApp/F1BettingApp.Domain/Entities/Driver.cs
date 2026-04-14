namespace F1BettingApp.Domain.Entities;

public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; }
    public ICollection<Bet> Bets { get; set; }
}