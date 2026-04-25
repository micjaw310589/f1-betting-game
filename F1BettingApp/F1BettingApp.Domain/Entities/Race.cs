using F1BettingApp.Domain.Enums;
using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

public class Race
{
    public Race()
    {
        Bets = new List<Bet>();
        Results = new List<Result>();
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public RaceStatus Status { get; set; }
    public string Circuit { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string OpenF1RaceId { get; set; } = string.Empty;
    public int Season { get; set; }

    public ICollection<Bet> Bets { get; set; }
    public ICollection<Result> Results { get; set; }

    public bool CanPlaceBets() => Status == RaceStatus.Scheduled;

    public bool IsRaceFinished() => Status == RaceStatus.Finished || Status == RaceStatus.ResultsProcessed;
}