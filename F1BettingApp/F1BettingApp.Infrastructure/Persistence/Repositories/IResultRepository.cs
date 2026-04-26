using System.Collections.Generic;
using System.Threading.Tasks;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public interface IResultRepository : IRepository<Result>
    {
        IQueryable<Result> GetRaceResults(int raceId);
        IQueryable<Result> GetDriverResults(int driverId, int season);
    }
}
