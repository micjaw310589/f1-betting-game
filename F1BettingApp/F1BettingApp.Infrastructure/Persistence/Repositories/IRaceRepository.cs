using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IRaceRepository : IRepository<Race>
    {
        Task<IQueryable<Race>> GetUpcomingRacesAsync();
        Task<Race> GetRaceWithResultsAsync(int raceId);
        Task<IQueryable<Race>> GetCurrentSeasonRacesAsync(int seasonId);
    }
}