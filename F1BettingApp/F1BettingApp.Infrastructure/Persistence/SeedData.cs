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
        var adminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123456");
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
        if (adminUser == null)
        {
            adminUser = new User("admin", "admin@f1bet.com", adminPassword, isActive: true, isAdmin: true)
            {
                Id = 1,
                Points = 10000
            };
            await context.Users.AddAsync(adminUser);
        }
        else
        {
            // Update existing admin user
            adminUser.UserName = "admin";
            adminUser.Email = "admin@f1bet.com";
            adminUser.PasswordHash = adminPassword;
            adminUser.IsActive = true;
            adminUser.IsAdmin = true;
            adminUser.Points = 10000;
            context.Users.Update(adminUser);
        }
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

        // ====================================================================
        // SEED MOCK DATA FOR DRIVER CHAMPIONSHIP STANDINGS (Wielosezonowy: 2026, 2025, 2024)
        // ====================================================================
        if (!context.DriverChampionships.Any())
        {
            var drivers = await context.Drivers.ToListAsync();
            var races2026 = await context.Races.Where(r => r.Season == 2026).OrderBy(r => r.Date).ToListAsync();

            if (drivers.Any())
            {
                var driverDict = drivers.ToDictionary(d => d.Name);

                // --- 1. SEZON 2026 (Wyścigi z oficjalnego kalendarza) ---
                var gpAustralia = races2026.FirstOrDefault(r => r.OpenF1RaceId == "2026-aus");
                var gpChina = races2026.FirstOrDefault(r => r.OpenF1RaceId == "2026-chi");
                var gpJapan = races2026.FirstOrDefault(r => r.OpenF1RaceId == "2026-jpn");

                var mockResults2026 = new List<(string DriverName, (int Pos, int Pts) Aus, (int Pos, int Pts) Chi, (int Pos, int Pts) Jpn)>
                {
                    ("Max Verstappen", (1, 25), (1, 25), (2, 18)),     // 68 pkt
                    ("Charles Leclerc", (2, 18), (3, 15), (1, 25)),    // 58 pkt
                    ("Lando Norris", (3, 15), (2, 18), (4, 12)),       // 45 pkt
                    ("Lewis Hamilton", (4, 12), (4, 12), (3, 15)),     // 39 pkt
                    ("George Russell", (5, 10), (5, 10), (5, 10))      // 30 pkt
                };

                foreach (var resultData in mockResults2026)
                {
                    if (driverDict.TryGetValue(resultData.DriverName, out var driver))
                    {
                        var championshipEntry = new F1BettingGame.Domain.Entities.DriverChampionship
                        {
                            DriverId = driver.Id,
                            Season = 2026,
                            Points = resultData.Aus.Pts + resultData.Chi.Pts + resultData.Jpn.Pts,
                            Position = 0, 
                            LastUpdated = DateTime.UtcNow.AddHours(-2),
                            RaceResults = new List<F1BettingGame.Domain.Entities.DriverChampionshipRace>()
                        };
                        await context.DriverChampionships.AddAsync(championshipEntry);
                        await context.SaveChangesAsync(); 

                        if (gpAustralia != null)
                        {
                            await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace
                            {
                                DriverChampionshipId = championshipEntry.Id,
                                RaceId = gpAustralia.Id,
                                PointsEarned = resultData.Aus.Pts,
                                Position = resultData.Aus.Pos
                            });
                        }
                        if (gpChina != null)
                        {
                            await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace
                            {
                                DriverChampionshipId = championshipEntry.Id,
                                RaceId = gpChina.Id,
                                PointsEarned = resultData.Chi.Pts,
                                Position = resultData.Chi.Pos
                            });
                        }
                        if (gpJapan != null)
                        {
                            await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace
                            {
                                DriverChampionshipId = championshipEntry.Id,
                                RaceId = gpJapan.Id,
                                PointsEarned = resultData.Jpn.Pts,
                                Position = resultData.Jpn.Pos
                            });
                        }
                    }
                }

                // --- 2. SEZON 2025 (Dane Historyczne) ---
                // Tworzymy wirtualne wyścigi archiwalne dla 2025, żeby encje powiązane nie strzeliły errorem
                var bhr2025 = new Race("Bahrain Grand Prix", new DateTime(2025, 3, 2, 0, 0, 0, DateTimeKind.Utc), "Sakhir", "Bahrain", "2025-bhr", 2025);
                var mco2025 = new Race("Monaco Grand Prix", new DateTime(2025, 5, 25, 0, 0, 0, DateTimeKind.Utc), "Monte Carlo", "Monaco", "2025-mco", 2025);
                await context.Races.AddRangeAsync(bhr2025, mco2025);
                await context.SaveChangesAsync();

                var mockResults2025 = new List<(string DriverName, (int Pos, int Pts) Bhr, (int Pos, int Pts) Mco)>
                {
                    ("Lando Norris", (1, 25), (1, 25)),       // 50 pkt - Lando mistrzem 2025!
                    ("Max Verstappen", (2, 18), (2, 18)),     // 36 pkt
                    ("Charles Leclerc", (3, 15), (4, 12)),    // 27 pkt
                    ("Oscar Piastri", (4, 12), (3, 15))       // 27 pkt
                };

                foreach (var resultData in mockResults2025)
                {
                    if (driverDict.TryGetValue(resultData.DriverName, out var driver))
                    {
                        var championshipEntry = new F1BettingGame.Domain.Entities.DriverChampionship
                        {
                            DriverId = driver.Id,
                            Season = 2025,
                            Points = resultData.Bhr.Pts + resultData.Mco.Pts,
                            Position = 0,
                            LastUpdated = DateTime.UtcNow.AddDays(-180),
                            RaceResults = new List<F1BettingGame.Domain.Entities.DriverChampionshipRace>()
                        };
                        await context.DriverChampionships.AddAsync(championshipEntry);
                        await context.SaveChangesAsync();

                        await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace 
                            { DriverChampionshipId = championshipEntry.Id, RaceId = bhr2025.Id, Position = resultData.Bhr.Pos, PointsEarned = resultData.Bhr.Pts });
                        await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace 
                            { DriverChampionshipId = championshipEntry.Id, RaceId = mco2025.Id, Position = resultData.Mco.Pos, PointsEarned = resultData.Mco.Pts });
                    }
                }

                // --- 3. SEZON 2024 (Dane Historyczne) ---
                var dabi2024 = new Race("Abu Dhabi Grand Prix", new DateTime(2024, 11, 26, 0, 0, 0, DateTimeKind.Utc), "Yas Marina", "Abu Dhabi", "2024-abi", 2024);
                await context.Races.AddAsync(dabi2024);
                await context.SaveChangesAsync();

                var mockResults2024 = new List<(string DriverName, (int Pos, int Pts) Abi)>
                {
                    ("Max Verstappen", (1, 25)),     // 25 pkt - Dominacja Maxa
                    ("Lewis Hamilton", (2, 18)),     // 18 pkt
                    ("George Russell", (3, 15))      // 15 pkt
                };

                foreach (var resultData in mockResults2024)
                {
                    if (driverDict.TryGetValue(resultData.DriverName, out var driver))
                    {
                        var championshipEntry = new F1BettingGame.Domain.Entities.DriverChampionship
                        {
                            DriverId = driver.Id,
                            Season = 2024,
                            Points = resultData.Abi.Pts,
                            Position = 0,
                            LastUpdated = DateTime.UtcNow.AddDays(-500),
                            RaceResults = new List<F1BettingGame.Domain.Entities.DriverChampionshipRace>()
                        };
                        await context.DriverChampionships.AddAsync(championshipEntry);
                        await context.SaveChangesAsync();

                        await context.DriverChampionshipRaces.AddAsync(new F1BettingGame.Domain.Entities.DriverChampionshipRace 
                            { DriverChampionshipId = championshipEntry.Id, RaceId = dabi2024.Id, Position = resultData.Abi.Pos, PointsEarned = resultData.Abi.Pts });
                    }
                }

                await context.SaveChangesAsync();

                // --- 4. AUTOMATYCZNE PRZELICZENIE POZYCJI DLA WSZYSTKICH SEZONÓW ---
                var allSeasons = new[] { 2024, 2025, 2026 };
                foreach (var currentSeason in allSeasons)
                {
                    var standings = await context.DriverChampionships
                        .Where(dc => dc.Season == currentSeason)
                        .OrderByDescending(dc => dc.Points)
                        .ToListAsync();

                    int currentPosition = 1;
                    foreach (var entry in standings)
                    {
                        entry.Position = currentPosition++;
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
