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
        private readonly IOpenF1ApiClient _openF1ApiClient;

        public RaceService(
            IRaceRepositoryExtensions raceRepository,
            IRepository<Result> resultRepository,
            IOpenF1ApiClient openF1ApiClient)
        {
            _raceRepository = raceRepository;
            _resultRepository = resultRepository;
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

        public async Task SyncRaceDataFromOpenF1Async()
        {
            try
            {
                var openF1Races = await _openF1ApiClient.GetRacesAsync();

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        foreach (var openF1Race in openF1Races)
                        {
                            var existingRace = (await _raceRepository.GetAllAsync())
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
                            }
                            else
                            {
                                existingRace.Name = openF1Race.Name;
                                existingRace.Date = openF1Race.Date;
                                existingRace.Circuit = openF1Race.Circuit;
                                existingRace.Country = openF1Race.Country;
                                existingRace.Season = openF1Race.Season;

                                await _raceRepository.UpdateAsync(existingRace);
                            }
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
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to sync race data from OpenF1 API", ex);
            }
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
    }
}
