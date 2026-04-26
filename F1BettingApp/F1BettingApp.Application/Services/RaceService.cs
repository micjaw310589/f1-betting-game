using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Application.Services
{
    public class RaceService : IRaceService
    {
        private readonly IRaceRepository _raceRepository;

        public RaceService(IRaceRepository raceRepository)
        {
            _raceRepository = raceRepository;
        }

        public async Task<RaceDto> GetRaceByIdAsync(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return null;

            return new RaceDto
            {
                Id = race.Id,
                Name = race.Name,
                RaceDate = race.Date,
                Status = race.Status
            };
        }

        public async Task<IEnumerable<RaceDto>> GetAllRacesAsync()
        {
            var races = _raceRepository.GetAll();
            return await races.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                RaceDate = r.Date,
                Status = r.Status
            }).ToListAsync();
        }

        public async Task<IEnumerable<RaceDto>> GetUpcomingRacesAsync()
        {
            var races = _raceRepository.GetUpcomingRaces();
            return await races.Select(r => new RaceDto
            {
                Id = r.Id,
                Name = r.Name,
                RaceDate = r.Date,
                Status = r.Status
            }).ToListAsync();
        }
    }
}