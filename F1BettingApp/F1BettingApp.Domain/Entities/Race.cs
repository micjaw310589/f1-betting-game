namespace F1BettingApp.Domain.Entities;

public class Race
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public RaceStatus Status { get; set; }
    public ICollection<Bet> Bets { get; set; }
}