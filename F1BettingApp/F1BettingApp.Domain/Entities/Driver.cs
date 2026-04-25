using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

public class Driver
{
    public Driver()
    {
        Bets = new List<Bet>();
        Results = new List<Result>();
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Country { get; set; } = string.Empty;
    public string OpenF1DriverId { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<Bet> Bets { get; set; }
    public ICollection<Result> Results { get; set; }

    public string GetFullName() => Name;
}