using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using F1BettingApp.Infrastructure.OpenF1;
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

        public RaceService(
            IRaceRepositoryExtensions raceRepository,
            IRepository<Result> resultRepository,
            IRepository<Driver> driverRepository,
            IOpenF1ApiClient openF1ApiClient)
        {
            _raceRepository = raceRepository;
            _resultRepository = resultRepository;
            _driverRepository = driverRepository;
            _openF1ApiClient = openF1ApiClient;
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
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null)
            {
                throw new KeyNotFoundException($"Race with ID {raceId} not found.");
            }

            if (race.IsRaceFinished() && !dto.Positions.Any())
            {
                throw new InvalidOperationException("Race already finished and no positions provided.");
            }

            // Validate positions
            if (dto.Positions.Any(p => p.Position < 1))
            {
                throw new ArgumentException("Position must be at least 1.");
            }

            // Delete existing results for this race
            var existingResults = await _resultRepository.GetAllAsync();
            var raceResults = existingResults.Where(r => r.RaceId == raceId).ToList();
            foreach (var existingResult in raceResults)
            {
                await _resultRepository.DeleteAsync(existingResult.Id);
            }

            // Insert new results
            foreach (var positionEntry in dto.Positions.OrderBy(p => p.Position))
            {
                // Calculate points based on position (standard F1 scoring)
                var points = CalculatePointsForPosition(positionEntry.Position);

                var result = new Result(
                    raceId,
                    positionEntry.DriverId,
                    positionEntry.Position,
                    points,
                    TimeSpan.Zero,
                    TimeSpan.Zero
                );

                await _resultRepository.AddAsync(result);
            }

            // Set fastest lap if provided
            if (dto.FastestLapDriverId.HasValue)
            {
                var fastestLapResult = raceResults.FirstOrDefault(r => r.DriverId == dto.FastestLapDriverId.Value);
                if (fastestLapResult != null)
                {
                    fastestLapResult.FastestLap = TimeSpan.Zero;
                    await _resultRepository.UpdateAsync(fastestLapResult);
                }
            }

            // Mark race as manually overridden
            race.IsManuallyOverridden = true;
            await _raceRepository.UpdateAsync(race);

            // Set status to Finished if not already
            if (race.Status != RaceStatus.Finished && race.Status != RaceStatus.ResultsProcessed)
            {
                race.Status = RaceStatus.Finished;
                await _raceRepository.UpdateAsync(race);
            }

            await _raceRepository.SaveChangesAsync();
            await _resultRepository.SaveChangesAsync();
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
                TimeAttackLaps = ""
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
    }
}
