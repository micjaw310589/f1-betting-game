using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities;

public class Team
{
    public Team()
    {
        Drivers = new List<Driver>();
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string OpenF1TeamId { get; set; } = string.Empty;
    public string Base { get; set; } = string.Empty;

    public ICollection<Driver> Drivers { get; set; }

    public IEnumerable<Driver> GetDrivers() => Drivers;
}