using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    public interface IRaceService
    {
        Task<RaceDto> GetRaceByIdAsync(int id);
        Task<IEnumerable<RaceDto>> GetAllRacesAsync();
        Task<IEnumerable<RaceDto>> GetUpcomingRacesAsync();
    }
}