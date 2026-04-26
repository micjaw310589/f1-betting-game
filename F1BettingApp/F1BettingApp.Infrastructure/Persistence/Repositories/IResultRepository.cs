using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IResultRepository : IRepository<Result>
    {
        Task<IQueryable<Result>> GetRaceResultsAsync(int raceId);
        Task<IQueryable<Result>> GetDriverResultsAsync(int driverId);
    }
}