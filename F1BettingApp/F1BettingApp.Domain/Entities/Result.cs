namespace F1BettingApp.Domain.Entities;

public class Result
{
    public int Id { get; set; }
    public int RaceId { get; set; }
    public Race? Race { get; set; }
    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
    public int Position { get; set; }
    public decimal Points { get; set; }
    public bool FastestLap { get; set; }
    public TimeSpan PitStopTime { get; set; }
    public int Season { get; set; }

    public bool IsPodiumFinish() => Position >= 1 && Position <= 3;
    public bool IsPointsFinish() => Position >= 1 && Position <= 10;
}