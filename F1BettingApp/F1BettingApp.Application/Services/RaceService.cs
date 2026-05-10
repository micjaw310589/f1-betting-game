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
            });
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

            // Validate that all driver IDs are positive
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
    }
}
