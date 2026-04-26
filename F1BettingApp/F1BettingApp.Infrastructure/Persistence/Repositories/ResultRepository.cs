using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence.Repositories
{
    public class ResultRepository : Repository<Result>, IResultRepository
    {
        public ResultRepository(AppDbContext context) : base(context)
        {
        }

        public IQueryable<Result> GetRaceResults(int raceId)
        {
            return (IQueryable<Result>)_dbSet
                .Include(r => r.Driver)
                .Include(r => r.Race)
                .Where(r => r.RaceId == raceId)
                .OrderBy(r => r.Position);
        }

        public IQueryable<Result> GetDriverResults(int driverId, int season)
        {
            return (IQueryable<Result>)_dbSet
                .Include(r => r.Race)
                .Where(r => r.DriverId == driverId && r.Season == season)
                .OrderBy(r => r.Race.Date);
        }
    }
}
