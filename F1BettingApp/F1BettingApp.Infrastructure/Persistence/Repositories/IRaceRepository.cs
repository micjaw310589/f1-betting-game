using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IRaceRepository : IRepository<Race>
    {
        IQueryable<Race> GetUpcomingRaces();
        Task<Race?> GetRaceWithResultsAsync(int raceId);
        IQueryable<Race> GetCurrentSeasonRaces();
    }
}
