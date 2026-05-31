using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Infrastructure.OpenF1
{
    public interface IOpenF1ApiClient
    {
        // Metody wymagane przez obecny kod (Kompatybilność wsteczna)
        Task<IEnumerable<OpenF1Race>> GetRacesAsync();
        Task<OpenF1Race?> GetRaceByIdAsync(string raceId);
        Task<IEnumerable<OpenF1DriverSessionData>> GetDriversAsync(string raceId);
        Task<OpenF1Race?> GetLatestRaceAsync();

        // NOWE METODY: Wydarzenia i Zespoły
        Task<IEnumerable<OpenF1Race>> GetUpcomingRacesAsync();
        Task<IEnumerable<OpenF1Race>> GetPastRacesAsync();
        Task<IEnumerable<OpenF1TeamData>> GetTeamsAsync(string raceId);
    }

    public class OpenF1Race
    {
        // Pola wymagane przez obecny kod
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }
        public string Circuit { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Season { get; set; }

        // NOWE POLA: Pełne informacje o wydarzeniu
        public string Location { get; set; } = string.Empty;
        public bool IsUpcoming { get; set; }
    }

    public class OpenF1DriverSessionData
    {
        // Pola wymagane przez obecny kod
        public int RaceId { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public System.DateTime Date { get; set; }

        // NOWE POLA: Pełne informacje o kierowcy
        public string NameAcronym { get; set; } = string.Empty;
        public string TeamColour { get; set; } = string.Empty;
        public int DriverNumber { get; set; }
    }

    // NOWY MODEL: Pełne informacje o zespołach
    public class OpenF1TeamData
    {
        public string TeamName { get; set; } = string.Empty;
        public string TeamColour { get; set; } = string.Empty; // Hex kolor zespołu z OpenF1
    }
}