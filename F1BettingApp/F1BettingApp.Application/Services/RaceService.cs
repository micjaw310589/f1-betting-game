using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using F1BettingApp.Infrastructure.OpenF1;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using F1BettingGame.Domain.Entities;
using System.Runtime.InteropServices;

namespace F1BettingApp.Application.Services
{
    public class RaceService : IRaceService
    {
        private readonly IRaceRepositoryExtensions _raceRepository;
        private readonly IRepository<Result> _resultRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IOpenF1ApiClient _openF1ApiClient;
        private readonly AppDbContext _dbContext;

        public RaceService(
            IRaceRepositoryExtensions raceRepository,
            IRepository<Result> resultRepository,
            IRepository<Driver> driverRepository,
            IOpenF1ApiClient openF1ApiClient,
            AppDbContext dbContext)
        {
            _raceRepository = raceRepository;
            _resultRepository = resultRepository;
            _driverRepository = driverRepository;
            _openF1ApiClient = openF1ApiClient;
            _dbContext = dbContext;
        }

        public async Task<RaceDto> GetRaceByIdAsync(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return null;

            return new RaceDto
            {
                Id = race.Id,
                Name = race.Name,
                Circuit = race.Circuit,
                Country = race.Country,
                RaceDate = race.Date,
                Status = race.Status,
                Season = race.Season,
                Flag = string.Empty, // TODO: Implement flag logic
                Odds = new Dictionary<int, decimal>()
            };
        }

        public async Task<IEnumerable<RaceDto>> GetAllRacesAsync()
        {
            var races = await _raceRepository.GetAllAsync();
            return races.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                RaceDate = r.Date,
                Status = r.Status,
                Country = r.Country,
                Circuit = r.Circuit,
                Season = r.Season,
                Flag = string.Empty, // TODO: Implement flag logic
                Odds = new Dictionary<int, decimal>()
            }).OrderBy(r => r.RaceDate);
        }

        public async Task<IEnumerable<RaceDto>> GetUpcomingRacesAsync()
        {
            var races = await _raceRepository.GetAllAsync();
            var upcoming = races.Where(r => r.Status == RaceStatus.Scheduled);
            return upcoming.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                RaceDate = r.Date,
                Status = r.Status,
                Country = r.Country,
                Circuit = r.Circuit,
                Season = r.Season,
                Flag = string.Empty, // TODO: Implement flag logic
                Odds = new Dictionary<int, decimal>()
            });
        }

        public async Task<SyncResultDto> SyncRaceDataFromOpenF1Async()
        {
            var result = new SyncResultDto
            {
                Success = false,
                SyncedAt = DateTime.UtcNow
            };

            try
            {
                var openF1Races = await _openF1ApiClient.GetRacesAsync();
                var allRaces = await _raceRepository.GetAllAsync();

                int created = 0;
                int updated = 0;

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        foreach (var openF1Race in openF1Races)
                        {
                            var existingRace = allRaces
                                .FirstOrDefault(r => r.OpenF1RaceId == openF1Race.Id);

                            if (existingRace == null)
                            {
                                var race = new Race(
                                    openF1Race.Name,
                                    openF1Race.Date,
                                    openF1Race.Circuit,
                                    openF1Race.Country,
                                    openF1Race.Id,
                                    openF1Race.Season
                                );
 
                                // Infer status from race date (OpenF1 doesn't provide explicit status)
                                var now = DateTime.UtcNow;
                                race.Status = openF1Race.Date > now ? RaceStatus.Scheduled : RaceStatus.Finished;
 
                                await _raceRepository.AddAsync(race);
                                created++;
                            }
                            else if (!existingRace.IsManuallyOverridden)
                            {
                                // Only update if not manually overridden
                                existingRace.Name = openF1Race.Name;
                                existingRace.Date = openF1Race.Date;
                                existingRace.Circuit = openF1Race.Circuit;
                                existingRace.Country = openF1Race.Country;
                                existingRace.Season = openF1Race.Season;
 
                                // Recalculate status based on updated date unless manually overridden
                                var now = DateTime.UtcNow;
                                existingRace.Status = openF1Race.Date > now ? RaceStatus.Scheduled : RaceStatus.Finished;
 
                                await _raceRepository.UpdateAsync(existingRace);
                                updated++;
                            }
                            // If IsManuallyOverridden, skip this race to preserve admin changes
                        }

                        await _raceRepository.SaveChangesAsync();
                        transaction.Complete();
                    }
                    catch
                    {
                        transaction.Dispose();
                        throw;
                    }
                }

                result.Success = true;
                result.RacesProcessed = created + updated;
                result.RacesCreated = created;
                result.RacesUpdated = updated;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                throw new InvalidOperationException("Failed to sync race data from OpenF1 API", ex);
            }

            return result;
        }

        public async Task<IEnumerable<RaceDto>> GetUpcomingRacesWithOddsAsync()
        {
            var races = await _raceRepository.GetAllAsync();
            var upcomingRaces = races.Where(r => r.Status == RaceStatus.Scheduled);

            var racesWithOdds = upcomingRaces.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                RaceDate = r.Date,
                Status = r.Status,
                Country = r.Country,
                Circuit = r.Circuit,
                Season = r.Season,
                Flag = string.Empty, // TODO: Implement flag logic
                Odds = new Dictionary<int, decimal>()
            });

            return racesWithOdds;
        }

        public async Task UpdateRaceStatusAsync(int raceId, RaceStatus newStatus)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null) throw new InvalidOperationException("Race not found");

            race.Status = newStatus;
            await _raceRepository.UpdateAsync(race);
            await _raceRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<RaceDto>> GetRacesByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<RaceDto>();

            var races = await _raceRepository.GetByIdsAsync(ids.ToList());
            return races.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                Circuit = r.Circuit,
                RaceDate = r.Date,
                Status = r.Status,
                Country = r.Country,
                Odds = new Dictionary<int, decimal>()
            });
        }

        public async Task<IEnumerable<Result>> GetResultsAsync(int raceId)
        {
            var results = await _resultRepository.GetAllAsync();
            return results.Where(r => r.RaceId == raceId).ToList();
        }

        public async Task OverrideRaceResultAsync(int raceId, OverrideRaceResultDto dto)
        {
            // Validate that at least one position is provided
            if (dto.Positions == null || !dto.Positions.Any())
            {
                throw new ArgumentException("At least one position entry is required.");
            }

            // Validate that all driver IDs are positive and positions are valid
            foreach (var positionEntry in dto.Positions)
            {
                if (positionEntry.DriverId <= 0)
                {
                    throw new ArgumentException($"Invalid driver ID: {positionEntry.DriverId}. Must be greater than 0.");
                }
                if (positionEntry.Position < 1)
                {
                    throw new ArgumentException($"Position must be at least 1.");
                }
            }

            // Validate that no driver appears more than once
            var driverIds = dto.Positions.Select(p => p.DriverId).ToList();
            var duplicates = driverIds.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Any())
            {
                var duplicateList = string.Join(", ", duplicates);
                throw new ArgumentException($"The following drivers are assigned to multiple positions: {duplicateList}. Each driver can only occupy one position.");
            }

            if (dto.FastestLapDriverId.HasValue && dto.FastestLapDriverId.Value <= 0)
            {
                throw new ArgumentException($"Invalid fastest lap driver ID: {dto.FastestLapDriverId.Value}. Must be greater than 0.");
            }

            // Get the race
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");
            }

            // Use DbContext directly for batch operations to avoid state management issues
            // Delete all existing results for this race
            var existingResults = _dbContext.Results.Where(r => r.RaceId == raceId).ToList();
            _dbContext.Results.RemoveRange(existingResults);
            
            // Save changes to flush deletions before inserting new results
            // This prevents unique constraint violations on (RaceId, DriverId)
            await _dbContext.SaveChangesAsync();

            // Create and add new results
            var newResults = new List<Result>();
            foreach (var positionEntry in dto.Positions.OrderBy(p => p.Position))
            {
                var points = CalculatePointsForPosition(positionEntry.Position);
                var result = new Result(
                    raceId,
                    positionEntry.DriverId,
                    positionEntry.Position,
                    points,
                    default,
                    TimeSpan.Zero,
                    null
                );
                // Clear fastest lap for all results initially
                result.FastestLap = null;
                _dbContext.Results.Add(result);
                newResults.Add(result);
            }

            // Set fastest lap if provided
            if (dto.FastestLapDriverId.HasValue)
            {
                var fastestLapResult = newResults.FirstOrDefault(r => r.DriverId == dto.FastestLapDriverId.Value);
                if (fastestLapResult != null)
                {
                    fastestLapResult.FastestLap = TimeSpan.Zero;
                }
            }

            // Update race entity
            race.IsManuallyOverridden = true;
            if (race.Status != RaceStatus.Finished && race.Status != RaceStatus.ResultsProcessed)
            {
                race.Status = RaceStatus.Finished;
            }

            // Save all changes in a single transaction
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RaceResultDto> GetRaceResultDtoAsync(int raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");
            }

            var results = await GetResultsAsync(raceId);
            var allDrivers = await _driverRepository.GetAllAsync();

            // Build a driver lookup
            var driverLookup = allDrivers.ToDictionary(d => d.Id, d => d);

            // Find winner (position 1)
            var winnerResult = results.FirstOrDefault(r => r.Position == 1);
            var winnerDriver = winnerResult != null && driverLookup.ContainsKey(winnerResult.DriverId)
                ? driverLookup[winnerResult.DriverId]
                : null;

            // Find fastest lap driver
            var fastestLapDriver = results.FirstOrDefault(r => r.FastestLap.HasValue);
            var fastestLapDriverEntity = fastestLapDriver != null && driverLookup.ContainsKey(fastestLapDriver.DriverId)
                ? driverLookup[fastestLapDriver.DriverId]
                : null;

            return new RaceResultDto
            {
                RaceId = race.Id,
                RaceName = race.Name,
                Circuit = race.Circuit,
                Country = race.Country,
                RaceDate = race.Date,
                WinnerDriverId = winnerDriver?.Id ?? 0,
                WinnerDriverName = winnerDriver?.Name ?? "TBD",
                WinnerTeamId = winnerDriver?.TeamId ?? 0,
                WinnerTeamName = winnerDriver?.Team?.Name ?? "TBD",
                FastestLapDriverId = fastestLapDriverEntity?.Id ?? 0,
                FastestLapDriverName = fastestLapDriverEntity?.Name ?? "TBD",
                // Remaining fields remain at default values
                WinningMargin = 0,
                PolePositionDriverId = 0,
                PolePositionDriverName = "TBD",
                SafetyCar = 0,
                VirtualSafetyCar = 0,
                RedFlag = 0,
                YellowFlag = 0,
                BlackFlag = 0,
                BlueFlag = 0,
                BlackAndWhiteFlag = 0,
                ChequeredFlag = 0,
                RaceDistance = 0,
                RaceDistanceUnit = 0,
                Laps = 0,
                LapsCompleted = 0,
                LapsToFinish = 0,
                RaceControlMessage = 0,
                RaceControlMessageText = "",
                TimeAttack = "",
                TimeAttackResult = "",
                TimeAttackComment = "",
                TimeAttackStatus = "",
                TimeAttackLaps = "",
                Positions = results
                    .OrderBy(r => r.Position)
                    .Select(r => new PositionDto
                    {
                        Position = r.Position,
                        DriverId = r.DriverId,
                        DriverName = driverLookup.ContainsKey(r.DriverId) ? driverLookup[r.DriverId].Name : "Unknown",
                        TeamId = driverLookup.ContainsKey(r.DriverId) ? driverLookup[r.DriverId].TeamId : 0,
                        TeamName = driverLookup.ContainsKey(r.DriverId) ? driverLookup[r.DriverId].Team?.Name ?? "TBD" : "TBD",
                        Points = CalculatePointsForPosition(r.Position),
                        FastestLap = r.FastestLap,
                        PitStopTime = r.PitStopTime
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// Calculates F1 points based on finishing position.
        /// Standard F1 scoring: 1st=25, 2nd=18, 3rd=15, 4th=12, 5th=10, 6th=8, 7th=6, 8th=4, 9th=2, 10th=1
        /// </summary>
        private static int CalculatePointsForPosition(int position)
        {
            return position switch
            {
                1 => 25,
                2 => 18,
                3 => 15,
                4 => 12,
                5 => 10,
                6 => 8,
                7 => 6,
                8 => 4,
                9 => 2,
                10 => 1,
                _ => 0
            };
        }

        public async Task UpdateRaceMetadataAsync(int raceId, UpdateRaceMetadataDto dto)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");
            }

            // Apply updates
            if (!string.IsNullOrWhiteSpace(dto.Name))
                race.Name = dto.Name;
            if (dto.Date.HasValue)
            {
                // Convert to UTC for PostgreSQL timestamp with time zone compatibility
                var dt = dto.Date.Value;
                if (dt.Kind == DateTimeKind.Unspecified)
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
                else if (dt.Kind == DateTimeKind.Local)
                {
                    dt = dt.ToUniversalTime();
                }
                race.Date = dt;
            }
            if (!string.IsNullOrWhiteSpace(dto.Circuit))
                race.Circuit = dto.Circuit;
            if (!string.IsNullOrWhiteSpace(dto.Country))
                race.Country = dto.Country;
            if (dto.Status.HasValue)
                race.Status = dto.Status.Value;

            // Mark as manually overridden to prevent future sync from reverting
            race.IsManuallyOverridden = true;

            await _raceRepository.UpdateAsync(race);
            await _raceRepository.SaveChangesAsync();
        }

        public async Task<RaceDto> CreateRaceAsync(CreateRaceDto dto)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Race name is required.");
            if (string.IsNullOrWhiteSpace(dto.Circuit))
                throw new ArgumentException("Circuit name is required.");
            if (string.IsNullOrWhiteSpace(dto.Country))
                throw new ArgumentException("Country is required.");
            if (dto.Season <= 0)
                throw new ArgumentException("Season must be positive.");

            // Generate a unique OpenF1RaceId (use a timestamp-based ID for manual races)
            string openF1RaceId = $"manual-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Convert to UTC if provided, otherwise default to UTC now
            DateTime raceDate = dto.Date.HasValue
                ? DateTime.SpecifyKind(dto.Date.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var race = new Race(
                dto.Name,
                raceDate,
                dto.Circuit,
                dto.Country,
                openF1RaceId,
                dto.Season
            );

            await _raceRepository.AddAsync(race);
            await _raceRepository.SaveChangesAsync();

            return new RaceDto
            {
                Id = race.Id,
                Name = race.Name,
                Circuit = race.Circuit,
                Country = race.Country,
                RaceDate = race.Date,
                Status = race.Status,
                Season = race.Season,
                Flag = race.Country,
                Odds = new Dictionary<int, decimal>()
            };
        }

        public async Task DeleteRaceAsync(int raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");

            // Check if there are any bets on this race
            var raceBets = await _dbContext.Bets.Where(b => b.RaceId == raceId).ToListAsync();
            if (raceBets.Any())
                throw new InvalidOperationException($"Cannot delete race '{race.Name}' because it has {raceBets.Count} bet(s) placed on it.");

            // Delete associated results
            var results = await _dbContext.Results.Where(r => r.RaceId == raceId).ToListAsync();
            _dbContext.Results.RemoveRange(results);

            // Delete the race
            await _raceRepository.DeleteAsync(raceId);
            await _raceRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<DriverDto>> GetAllDriversAsync()
        {
            var driversList = await _dbContext.Drivers
                .Include(d => d.Team)
                .ToListAsync();
            return driversList.Select(d => new DriverDto
            {
                Id = d.Id,
                Name = d.Name,
                Abbreviation = d.OpenF1DriverId,
                TeamId = d.TeamId,
                TeamName = d.Team?.Name ?? string.Empty
            });
        }

        public async Task<IEnumerable<DriverWithOddsDto>> GetDriversWithOddsForRaceAsync(int raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null) return Enumerable.Empty<DriverWithOddsDto>();

            // 1. Pobieramy kierowców do pamięci (.ToListAsync() lub .ToList())
            var drivers = await _driverRepository.GetAllAsync();
            var driversList = drivers.ToList(); 

            // 2. Mapujemy w pamięci (już po pobraniu z bazy), wtedy C# bez problemu obsłuży GetOddsForDriver
            return driversList.Select(d => new DriverWithOddsDto
            {
                DriverId = d.Id,
                DriverName = d.Name,
                Odds = GetOddsForDriver(race, d.Id) // Teraz to zadziała bezpiecznie
            });
        }

        private decimal GetOddsForDriver(Race race, int driverId)
        {
            try
            {
                return race.OddsForDriver(driverId);
            }
            catch (NotImplementedException)
            {
                return 1.25m;
            }
        }

/// <summary>
        /// Pobiera uproszczoną klasyfikację generalną kierowców dla danego sezonu bezpośrednio z bazy.
        /// </summary>
        public async Task<IEnumerable<DriverChampionshipDto>> GetDriverChampionshipStandingsAsync(int season)
        {
            return await _dbContext.DriverChampionships
                .Include(dc => dc.Driver)
                    .ThenInclude(d => d.Team)
                .Where(dc => dc.Season == season)
                .OrderBy(dc => dc.Position)
                .Select(dc => new DriverChampionshipDto
                {
                    DriverId = dc.DriverId,
                    DriverName = dc.Driver.Name,
                    DriverCountry = dc.Driver.Country,
                    TeamName = dc.Driver.Team != null ? dc.Driver.Team.Name : "No Team",
                    Season = dc.Season,
                    TotalPoints = dc.Points, // Zmapowane pod points_current z bazy
                    Position = dc.Position,
                    LastUpdated = dc.LastUpdated,
                    RaceResults = new List<DriverChampionshipRaceDto>() // Usunięte ciężkie ładowanie relacji
                })
                .ToListAsync();
        }

        /// <summary>
        /// Pobiera uproszczone szczegóły klasyfikacji konkretnego kierowcy.
        /// </summary>
        public async Task<DriverChampionshipDto?> GetDriverChampionshipDetailsAsync(int driverId, int season)
        {
            return await _dbContext.DriverChampionships
                .Include(dc => dc.Driver)
                    .ThenInclude(d => d.Team)
                .Where(dc => dc.DriverId == driverId && dc.Season == season)
                .Select(dc => new DriverChampionshipDto
                {
                    DriverId = dc.DriverId,
                    DriverName = dc.Driver.Name,
                    DriverCountry = dc.Driver.Country,
                    TeamName = dc.Driver.Team != null ? dc.Driver.Team.Name : "No Team",
                    Season = dc.Season,
                    TotalPoints = dc.Points,
                    Position = dc.Position,
                    LastUpdated = dc.LastUpdated,
                    RaceResults = new List<DriverChampionshipRaceDto>()
                })
                .FirstOrDefaultAsync();
        }

  public async Task SyncChampionshipFromOpenF1Async(int season)
        {
            // Przekazujemy "latest" jako klucz sesji zgodnie z dokumentacją OpenF1
            string sessionKey = "latest";

            // 1. Pobieramy aktualną klasyfikację punktową (zwraca same numery startowe)
            var apiStandings = await _openF1ApiClient.GetDriverChampionshipStandingsAsync(sessionKey);
            if (apiStandings == null || !apiStandings.Any()) return;

            // 2. Pobieramy mapowanie numerów na imiona i nazwiska kierowców z tej samej najnowszej sesji
            var openF1Drivers = await _openF1ApiClient.GetDriversAsync(sessionKey);
            var openF1DriverLookup = openF1Drivers
                .GroupBy(d => d.DriverNumber)
                .ToDictionary(g => g.Key, g => g.First().DriverName);

            // 3. Pobieramy wszystkich kierowców z Twojej bazy lokalnej
            var dbDrivers = await _dbContext.Drivers.ToListAsync();
            
            foreach (var apiDriverData in apiStandings)
            {
                // Sprawdzamy, jak ten numer z F1 nazywa się tekstowo (np. 1 -> "Max Verstappen")
                if (!openF1DriverLookup.TryGetValue(apiDriverData.DriverNumber, out var officialFullName))
                {
                    continue; 
                }

                // Szukamy kierowcy w Twojej bazie porównując tekstowo jego Imię i Nazwisko
                var driver = dbDrivers.FirstOrDefault(d => d.Name.Equals(officialFullName, StringComparison.OrdinalIgnoreCase));
                
                // ZABEZPIECZENIE: Jeśli nie znalazło idealnie, sprawdzamy czy nazwisko z bazy zawiera się w tym z API
                if (driver == null)
                {
                    driver = dbDrivers.FirstOrDefault(d => officialFullName.Contains(d.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (driver == null) continue; 

                // 4. Zapisujemy lub aktualizujemy rekord w bazie danych
                var championshipEntry = await _dbContext.DriverChampionships
                    .FirstOrDefaultAsync(dc => dc.DriverId == driver.Id && dc.Season == season);

                if (championshipEntry == null)
                {
                    championshipEntry = new DriverChampionship
                    {
                        DriverId = driver.Id,
                        Season = season,
                        Points = (int)Math.Round(apiDriverData.PointsCurrent ?? 0),
                        Position = apiDriverData.PositionCurrent ?? 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _dbContext.DriverChampionships.Add(championshipEntry);
                }
                else
                {
                    championshipEntry.Points = (int)Math.Round(apiDriverData.PointsCurrent ?? 0);
                    championshipEntry.Position = apiDriverData.PositionCurrent ?? 0;
                    championshipEntry.LastUpdated = DateTime.UtcNow;
                    _dbContext.DriverChampionships.Update(championshipEntry);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
        // Metody RecalculateChampionshipAsync i UpdateChampionshipFromRaceResultsAsync 
        // można teraz usunąć lub zostawić jako fallback dla ręcznych modyfikacji.
        /// <summary>
        /// Aktualizuje tabelę klasyfikacji na podstawie wyników pojedynczego ukończonego wyścigu.
        /// </summary>
        public async Task UpdateChampionshipFromRaceResultsAsync(int raceId)
        {
            var race = await _dbContext.Races.FindAsync(raceId);
            if (race == null) return;

            // Pobierz wyniki dla tego wyścigu
            var raceResults = await _dbContext.Results
                .Where(r => r.RaceId == raceId)
                .ToListAsync();

            if (!raceResults.Any()) return;

            foreach (var result in raceResults)
            {
                // Znajdź lub utwórz rekord klasyfikacji generalnej dla kierowcy w danym sezonie
                var championshipEntry = await _dbContext.DriverChampionships
                    .FirstOrDefaultAsync(dc => dc.DriverId == result.DriverId && dc.Season == race.Season);

                if (championshipEntry == null)
                {
                    championshipEntry = new DriverChampionship
                    {
                        DriverId = result.DriverId,
                        Season = race.Season,
                        Points = 0,
                        Position = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _dbContext.DriverChampionships.Add(championshipEntry);
                    // Zapisujemy, aby wygenerować Id wymagane do powiązania tabeli wyścigów
                    await _dbContext.SaveChangesAsync();
                }

                // Sprawdź, czy punkty z tego wyścigu nie zostały już przypadkowo dodane (idempotentność)
                var alreadyProcessed = await _dbContext.DriverChampionshipRaces
                    .AnyAsync(r => r.DriverChampionshipId == championshipEntry.Id && r.RaceId == raceId);

                if (!alreadyProcessed)
                {
                    var raceEntry = new DriverChampionshipRace
                    {
                        DriverChampionshipId = championshipEntry.Id,
                        RaceId = raceId,
                        PointsEarned = result.Points,
                        Position = result.Position
                    };

                    _dbContext.DriverChampionshipRaces.Add(raceEntry);
                    championshipEntry.Points += result.Points;
                    championshipEntry.LastUpdated = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();

            // Po dodaniu punktów, przelicz pozycje wszystkich kierowców w tym sezonie
            await RecalculatePositionsAsync(race.Season);
        }

        /// <summary>
        /// Czyści i całkowicie od nowa generuje klasyfikację dla wybranego sezonu.
        /// </summary>
        public async Task RecalculateChampionshipAsync(int season)
        {
            // Usunięcie starych danych dla wybranego sezonu (Cascade usunie rekordy z tabeli DriverChampionshipRaces)
            var existingEntries = await _dbContext.DriverChampionships
                .Where(dc => dc.Season == season)
                .ToListAsync();

            _dbContext.DriverChampionships.RemoveRange(existingEntries);
            await _dbContext.SaveChangesAsync();

            // Pobranie wszystkich ukończonych wyścigów z tego sezonu, które mają przypisane wyniki
            var races = await _dbContext.Races
                .Where(r => r.Season == season)
                .OrderBy(r => r.Date)
                .ToListAsync();

            foreach (var race in races)
            {
                var raceResults = await _dbContext.Results
                    .Where(r => r.RaceId == race.Id)
                    .ToListAsync();

                foreach (var result in raceResults)
                {
                    var championshipEntry = await _dbContext.DriverChampionships
                        .FirstOrDefaultAsync(dc => dc.DriverId == result.DriverId && dc.Season == season);

                    if (championshipEntry == null)
                    {
                        championshipEntry = new DriverChampionship
                        {
                            DriverId = result.DriverId,
                            Season = season,
                            Points = 0,
                            Position = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        _dbContext.DriverChampionships.Add(championshipEntry);
                        await _dbContext.SaveChangesAsync();
                    }

                    var raceEntry = new DriverChampionshipRace
                    {
                        DriverChampionshipId = championshipEntry.Id,
                        RaceId = race.Id,
                        PointsEarned = result.Points,
                        Position = result.Position
                    };

                    _dbContext.DriverChampionshipRaces.Add(raceEntry);
                    championshipEntry.Points += result.Points;
                    championshipEntry.LastUpdated = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();

            // Przydzielenie ostatecznych pozycji w tabeli
            await RecalculatePositionsAsync(season);
        }

        /// <summary>
        /// Prywatna metoda pomocnicza do sortowania kierowców i aktualizacji ich pozycji (1, 2, 3...) w bazie danych.
        /// </summary>
        private async Task RecalculatePositionsAsync(int season)
        {
            var standings = await _dbContext.DriverChampionships
                .Where(dc => dc.Season == season)
                .OrderByDescending(dc => dc.Points)
                .ToListAsync();

            int position = 1;
            foreach (var entry in standings)
            {
                entry.Position = position++;
                entry.LastUpdated = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        

        public async Task StoreRaceResultAsync(int raceId, List<PositionEntryDto> positions, int? fastestLapDriverId = null)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");

            // var currentSeason = DateTime.UtcNow.Year;

            // // Only store results for current season
            // if (race.Season != currentSeason)
            //     return;

            // Validate positions
            if (positions == null || !positions.Any())
                throw new ArgumentException("At least one position entry is required.");

            foreach (var pos in positions)
            {
                if (pos.Position < 1)
                    throw new ArgumentException($"Position must be at least 1.");
                if (pos.DriverId <= 0)
                    throw new ArgumentException($"Invalid driver ID: {pos.DriverId}. Must be greater than 0.");
            }

            // Validate no duplicate drivers
            var driverIds = positions.Select(p => p.DriverId).ToList();
            var duplicates = driverIds.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Any())
                throw new ArgumentException($"Drivers cannot appear in multiple positions: {string.Join(", ", duplicates)}");

            if (fastestLapDriverId.HasValue && fastestLapDriverId.Value <= 0)
                throw new ArgumentException($"Invalid fastest lap driver ID: {fastestLapDriverId.Value}. Must be greater than 0.");

            var raceResult = await _dbContext.RaceResults
                .Include(r => r.Positions)
                .FirstOrDefaultAsync(r => r.RaceId == raceId);

            if (raceResult == null)
            {
                raceResult = new RaceResult
                {
                    RaceId = raceId,
                    Season = race.Season,
                    Positions = new List<RaceResultPosition>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _dbContext.RaceResults.Add(raceResult);
            }

            // Fetch drivers to get their associated TeamIds
            var driverIdsForPositions = positions.Select(p => p.DriverId).ToList();
            var driversForPositions = await _dbContext.Drivers
                .Where(d => driverIdsForPositions.Contains(d.Id))
                .ToListAsync();
            var driverTeamMap = driversForPositions.ToDictionary(d => d.Id, d => d.TeamId);

            // Update positions
            raceResult.Positions.Clear();
            foreach (var pos in positions.OrderBy(p => p.Position))
            {
                var points = CalculatePointsForPosition(pos.Position);
                
                // Get the correct TeamId or default to 0 if not found
                var actualTeamId = driverTeamMap.TryGetValue(pos.DriverId, out var mappedTeamId) 
                    ? mappedTeamId 
                    : 0;

                raceResult.Positions.Add(new RaceResultPosition
                {
                    Position = pos.Position,
                    DriverId = pos.DriverId,
                    TeamId = actualTeamId, // No longer hardcoded to 0
                    Points = points
                });
            }
            raceResult.FastestLapDriverId = fastestLapDriverId;
            raceResult.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<RaceResultDto?> GetStoredRaceResultAsync(int raceId)
        {
            // var currentSeason = DateTime.UtcNow.Year;

            var raceResult = await _dbContext.RaceResults
                .Include(r => r.Positions)
                .Include(r => r.FastestLapDriver)
                // .Where(r => r.RaceId == raceId && r.Season == currentSeason)
                .Where(r => r.RaceId == raceId)

                .FirstOrDefaultAsync();

            if (raceResult == null)
                return null;

            // var allDrivers = await _driverRepository.GetAllAsync();
            var allDrivers = await _dbContext.Drivers
                .Include(d => d.Team)
                .ToListAsync();
            var driverLookup = allDrivers.ToDictionary(d => d.Id, d => d);

            var positions = raceResult.Positions
                .OrderBy(p => p.Position)
                .Select(p => new PositionDto
                {
                    Position = p.Position,
                    DriverId = p.DriverId,
                    DriverName = driverLookup.ContainsKey(p.DriverId) ? driverLookup[p.DriverId].Name : "Unknown",
                    TeamId = p.TeamId,
                    TeamName = driverLookup.ContainsKey(p.DriverId) ? driverLookup[p.DriverId].Team?.Name ?? "TBD" : "TBD",
                    Points = p.Points,
                    FastestLap = null
                })
                .ToList();

            // Find winner (position 1)
            var winnerPosition = positions.FirstOrDefault(p => p.Position == 1);
            var winnerDriverId = winnerPosition?.DriverId ?? 0;
            var winnerDriver = driverLookup.ContainsKey(winnerDriverId) ? driverLookup[winnerDriverId] : null;

            // Find fastest lap driver
            var fastestLapDriverId = raceResult.FastestLapDriverId ?? 0;
            var fastestLapDriver = fastestLapDriverId > 0 && driverLookup.ContainsKey(fastestLapDriverId)
                ? driverLookup[fastestLapDriverId]
                : null;

            // Mark positions with fastest lap indicator
            foreach (var pos in positions)
            {
                if (pos.DriverId == fastestLapDriverId)
                {
                    pos.FastestLap = TimeSpan.Zero;
                }
            }

            return new RaceResultDto
            {
                RaceId = raceResult.RaceId,
                RaceName = raceResult.Race?.Name ?? "Unknown",
                Circuit = raceResult.Race?.Circuit ?? "Unknown",
                Country = raceResult.Race?.Country ?? "Unknown",
                RaceDate = raceResult.Race?.Date ?? DateTime.UtcNow,
                WinnerDriverId = winnerDriver?.Id ?? 0,
                WinnerDriverName = winnerDriver?.Name ?? "TBD",
                WinnerTeamId = winnerDriver?.TeamId ?? 0,
                WinnerTeamName = winnerDriver?.Team?.Name ?? "TBD",
                FastestLapDriverId = raceResult.FastestLapDriverId,
                FastestLapDriverName = fastestLapDriver?.Name ?? "TBD",
                FastestLapTime = raceResult.FastLapTime,
                Positions = positions
            };
        }

    }
}
