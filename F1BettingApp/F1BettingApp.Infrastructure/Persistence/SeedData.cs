using BCrypt.Net;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with initial data (teams, drivers, races, etc.)
/// </summary>
public static class SeedData
{
    public static async Task Initialize(AppDbContext context)
    {
        // Seed admin user (ensure id=1 is available)
        var existingUserWithId1 = context.Users.FirstOrDefault(u => u.Id == 1);
        if (existingUserWithId1 != null)
        {
            context.Users.Remove(existingUserWithId1);
            await context.SaveChangesAsync();
        }
        var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123456");
        var adminUser = new User("admin", "admin@f1bet.com", adminPassword, isActive: true, isAdmin: true)
        {
            Id = 1,
            Points = 10000
        };
        await context.Users.AddAsync(adminUser);
        await context.SaveChangesAsync();

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
                new Team("Audi", "Germany", "audi"),
                new Team("Haas F1 Team", "United States", "haas"),
                new Team("Cadillac", "United States", "cadillac")
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
                ("Isack Hadjar", "France", "red-bull-racing"),
                // Ferrari
                ("Charles Leclerc", "Monaco", "ferrari"),
                ("Lewis Hamilton", "United Kingdom", "ferrari"),
                // Mercedes
                ("Andrea Kimi Antonelli", "Italy", "mercedes"),
                ("George Russell", "United Kingdom", "mercedes"),
                // McLaren
                ("Lando Norris", "United Kingdom", "mclaren"),
                ("Oscar Piastri", "Australia", "mclaren"),
                // Aston Martin
                ("Fernando Alonso", "Spain", "aston-martin"),
                ("Lance Stroll", "Canada", "aston-martin"),
                // Alpine
                ("Pierre Gasly", "France", "alpine"),
                ("Franco Colapinto", "Argentina", "alpine"),
                // Williams
                ("Alexander Albon", "Thailand", "williams"),
                ("Carlos Sainz", "Spain", "williams"),
                // RB
                ("Arvid Lindblad", "United Kingdom", "rb"),
                ("Liam Lawson", "New Zealand", "rb"),
                // Audi
                ("Nico Hulkenberg", "Germany", "audi"),
                ("Gabriel Bortoletto", "Brazil", "audi"),
                // Haas
                ("Esteban Ocon", "France", "haas"),
                ("Oliver Bearman", "United Kingdom", "haas"),
                // Cadillac
                ("Valtteri Bottas", "Finland", "cadillac"),
                ("Sergio Perez", "Mexico", "cadillac")
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

        // Seed 2026 F1 calendar races if not already seeded
        if (!context.Races.Any())
        {
            var races2026 = new[]
            {
                new { Name = "Australian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 3, 15), DateTimeKind.Utc), Circuit = "Albert Park Grand Prix Circuit", Country = "Australia", OpenF1RaceId = "2026-aus" },
                new { Name = "Chinese Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 3, 22), DateTimeKind.Utc), Circuit = "Shanghai International Circuit", Country = "China", OpenF1RaceId = "2026-chi" },
                new { Name = "Japanese Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 4, 5), DateTimeKind.Utc), Circuit = "Suzuka International Racing Course", Country = "Japan", OpenF1RaceId = "2026-jpn" },
                new { Name = "Bahrain Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 4, 19), DateTimeKind.Utc), Circuit = "Bahrain International Circuit", Country = "Bahrain", OpenF1RaceId = "2026-bhr" },
                new { Name = "Saudi Arabian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 4, 26), DateTimeKind.Utc), Circuit = "Jeddah Corniche Circuit", Country = "Saudi Arabia", OpenF1RaceId = "2026-sau" },
                new { Name = "Miami Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 5, 10), DateTimeKind.Utc), Circuit = "Miami International Autodrome", Country = "United States", OpenF1RaceId = "2026-mia" },
                new { Name = "Emilia Romagna Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 5, 24), DateTimeKind.Utc), Circuit = "Autodromo Enzo e Dino Ferrari", Country = "Italy", OpenF1RaceId = "2026-emi" },
                new { Name = "Monaco Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 5, 31), DateTimeKind.Utc), Circuit = "Circuit de Monaco", Country = "Monaco", OpenF1RaceId = "2026-mco" },
                new { Name = "American Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 6, 7), DateTimeKind.Utc), Circuit = "Circuit of the Americas", Country = "United States", OpenF1RaceId = "2026-usa" },
                new { Name = "Madrid Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 6, 14), DateTimeKind.Utc), Circuit = "Circuit Madrid-Aragon", Country = "Spain", OpenF1RaceId = "2026-mad" },
                new { Name = "Canadian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 6, 21), DateTimeKind.Utc), Circuit = "Circuit Gilles Villeneuve", Country = "Canada", OpenF1RaceId = "2026-can" },
                new { Name = "French Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 6, 28), DateTimeKind.Utc), Circuit = "Circuit Paul Ricard", Country = "France", OpenF1RaceId = "2026-fra" },
                new { Name = "British Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 7, 12), DateTimeKind.Utc), Circuit = "Silverstone Circuit", Country = "United Kingdom", OpenF1RaceId = "2026-gbr" },
                new { Name = "Austrian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 7, 26), DateTimeKind.Utc), Circuit = "Red Bull Ring", Country = "Austria", OpenF1RaceId = "2026-aut" },
                new { Name = "Belgian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 8, 23), DateTimeKind.Utc), Circuit = "Circuit de Spa-Francorchamps", Country = "Belgium", OpenF1RaceId = "2026-bel" },
                new { Name = "Dutch Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 8, 30), DateTimeKind.Utc), Circuit = "Circuit Zandvoort", Country = "Netherlands", OpenF1RaceId = "2026-ned" },
                new { Name = "Italian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 9, 6), DateTimeKind.Utc), Circuit = "Autodromo Nazionale di Monza", Country = "Italy", OpenF1RaceId = "2026-ita" },
                new { Name = "Singapore Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 9, 20), DateTimeKind.Utc), Circuit = "Marina Bay Street Circuit", Country = "Singapore", OpenF1RaceId = "2026-sin" },
                new { Name = "United States Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 10, 4), DateTimeKind.Utc), Circuit = "Circuit of the Americas", Country = "United States", OpenF1RaceId = "2026-usg" },
                new { Name = "Mexican Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 10, 25), DateTimeKind.Utc), Circuit = "Autodromo Hermanos Rodriguez", Country = "Mexico", OpenF1RaceId = "2026-mex" },
                new { Name = "Brazilian Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 11, 1), DateTimeKind.Utc), Circuit = "Autodromo Jose Carlos Pace", Country = "Brazil", OpenF1RaceId = "2026-bra" },
                new { Name = "Las Vegas Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 11, 13), DateTimeKind.Utc), Circuit = "Las Vegas Strip Circuit", Country = "United States", OpenF1RaceId = "2026-las" },
                new { Name = "Qatar Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 11, 20), DateTimeKind.Utc), Circuit = "Lusail International Circuit", Country = "Qatar", OpenF1RaceId = "2026-qat" },
                new { Name = "Abu Dhabi Grand Prix", Date = DateTime.SpecifyKind(new DateTime(2026, 11, 29), DateTimeKind.Utc), Circuit = "Yas Marina Circuit", Country = "Abu Dhabi", OpenF1RaceId = "2026-abi" }
            };

            foreach (var race in races2026)
            {
                var f1Race = new Race(
                    race.Name,
                    race.Date,
                    race.Circuit,
                    race.Country,
                    race.OpenF1RaceId,
                    2026);
                await context.Races.AddAsync(f1Race);
            }

            await context.SaveChangesAsync();
        }
    }
}
