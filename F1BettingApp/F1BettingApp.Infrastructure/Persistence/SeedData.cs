using F1BettingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with initial data (teams, drivers, etc.)
/// </summary>
public static class SeedData
{
    public static async Task Initialize(AppDbContext context)
    {
        // Seed teams if not already seeded
        if (!context.Teams.Any())
        {
            var teams = new[]
            {
                new Team("Red Bull Racing", "Austria", "red-bull-racing"),
                new Team("Ferrari", "Italy", "ferrari"),
                new Team("Mercedes", "Germany", "mercedes"),
                new Team("McLaren", "United Kingdom", "mclaren"),
                new Team("Aston Martin", "United Kingdom", "aston-martin"),
                new Team("Alpine", "France", "alpine"),
                new Team("Williams", "United Kingdom", "williams"),
                new Team("RB", "Italy", "rb"),
                new Team("Kick Sauber", "Switzerland", "kick-sauber"),
                new Team("Haas F1 Team", "United States", "haas")
            };

            foreach (var team in teams)
            {
                await context.Teams.AddAsync(team);
            }

            await context.SaveChangesAsync();
        }

        // Seed drivers if not already seeded
        if (!context.Drivers.Any())
        {
            // Get teams for driver assignment
            var teams = await context.Teams.ToListAsync();
            var teamByName = teams.ToDictionary(t => t.Name);

            var drivers = new[]
            {
                // Red Bull Racing
                ("Max Verstappen", "Netherlands", "red-bull-racing"),
                ("Sergio Perez", "Mexico", "red-bull-racing"),
                // Ferrari
                ("Charles Leclerc", "Monaco", "ferrari"),
                ("Carlos Sainz", "Spain", "ferrari"),
                // Mercedes
                ("Lewis Hamilton", "United Kingdom", "mercedes"),
                ("George Russell", "United Kingdom", "mercedes"),
                // McLaren
                ("Lando Norris", "United Kingdom", "mclaren"),
                ("Oscar Piastri", "Australia", "mclaren"),
                // Aston Martin
                ("Fernando Alonso", "Spain", "aston-martin"),
                ("Lance Stroll", "Canada", "aston-martin"),
                // Alpine
                ("Pierre Gasly", "France", "alpine"),
                ("Esteban Ocon", "France", "alpine"),
                // Williams
                ("Alexander Albon", "Thailand", "williams"),
                ("Franco Colapinto", "Argentina", "williams"),
                // RB
                ("Yuki Tsunoda", "Japan", "rb"),
                ("Liam Lawson", "New Zealand", "rb"),
                // Kick Sauber
                ("Valtteri Bottas", "Finland", "kick-sauber"),
                ("Zhou Guanyu", "China", "kick-sauber"),
                // Haas
                ("Kevin Magnussen", "Denmark", "haas"),
                ("Nico Hulkenberg", "Germany", "haas")
            };

            foreach (var (name, country, teamKey) in drivers)
            {
                var team = teamByName.Values.FirstOrDefault(t => t.OpenF1TeamId == teamKey);
                if (team != null)
                {
                    var driverId = $"{teamKey}-{name.ToLower().Replace(" ", "-")}";
                    var driver = new Driver(name, country, driverId, team.Id);
                    await context.Drivers.AddAsync(driver);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}