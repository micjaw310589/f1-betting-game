namespace F1BettingApp.Domain.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Driver> Drivers { get; set; }
}