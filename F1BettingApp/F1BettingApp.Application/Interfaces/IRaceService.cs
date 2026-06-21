using F1BettingApp.Application.DTOs;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for race-related operations
    /// </summary>
    public interface IRaceService
    {
        /// <summary>
        /// Gets a race by its ID
        /// </summary>
        /// <param name="id">The ID of the race</param>
        /// <returns>Race DTO</returns>
        Task<RaceDto> GetRaceByIdAsync(int id);

        /// <summary>
        /// Gets all races
        /// </summary>
        /// <returns>Collection of all race DTOs</returns>
        Task<IEnumerable<RaceDto>> GetAllRacesAsync();

        /// <summary>
        /// Gets upcoming races
        /// </summary>
        /// <returns>Collection of upcoming race DTOs</returns>
        Task<IEnumerable<RaceDto>> GetUpcomingRacesAsync();

        /// <summary>
        /// Synchronizes race data from OpenF1 API
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task<SyncResultDto> SyncRaceDataFromOpenF1Async();

        /// <summary>
        /// Gets upcoming races with odds information
        /// </summary>
        /// <returns>Collection of race DTOs with odds</returns>
        Task<IEnumerable<RaceDto>> GetUpcomingRacesWithOddsAsync();

        /// <summary>
        /// Updates the status of a race
        /// </summary>
        /// <param name="raceId">The ID of the race to update</param>
        /// <param name="newStatus">The new status of the race</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateRaceStatusAsync(int raceId, RaceStatus newStatus);

        /// <summary>
        /// Gets multiple races by their IDs efficiently (batch operation)
        /// </summary>
        /// <param name="ids">Collection of race IDs to retrieve</param>
        /// <returns>Collection of race DTOs matching the provided IDs</returns>
        Task<IEnumerable<RaceDto>> GetRacesByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Gets race drivers with their odds for a given race
        /// </summary>
        /// <param name="raceId">The ID of the race</param>
        /// <returns>Drivers with odds</returns>
        Task<IEnumerable<DriverWithOddsDto>> GetDriversWithOddsForRaceAsync(int raceId);

        /// <summary>
        /// Gets race results for a completed race
        /// </summary>
        /// <param name="raceId">The ID of the race</param>
        /// <returns>Collection of results for the race</returns>
        Task<IEnumerable<Result>> GetResultsAsync(int raceId);

        /// <summary>
        /// Manually overrides race results (admin only).
        /// Sets IsManuallyOverridden to prevent future auto-sync from reverting.
        /// </summary>
        /// <param name="raceId">The ID of the race to override.</param>
        /// <param name="dto">The override data with positions.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task OverrideRaceResultAsync(int raceId, OverrideRaceResultDto dto);

        /// <summary>
        /// Gets race results with driver details for display (admin).
        /// </summary>
        /// <param name="raceId">The ID of the race.</param>
        /// <returns>Race result DTO with filled-in data.</returns>
        Task<RaceResultDto> GetRaceResultDtoAsync(int raceId);

        /// <summary>
        /// Updates race metadata (name, date, status, circuit, country) - admin only.
        /// Sets IsManuallyOverridden to prevent future auto-sync from reverting.
        /// </summary>
        /// <param name="raceId">The ID of the race to update.</param>
        /// <param name="dto">The metadata to update.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateRaceMetadataAsync(int raceId, UpdateRaceMetadataDto dto);

        /// <summary>
        /// Creates a new race (admin only).
        /// </summary>
        /// <param name="dto">The race creation data.</param>
        /// <returns>The created race DTO.</returns>
        Task<RaceDto> CreateRaceAsync(CreateRaceDto dto);

        /// <summary>
        /// Deletes a race (admin only). Only allowed if the race has no bets.
        /// </summary>
        /// <param name="raceId">The ID of the race to delete.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteRaceAsync(int raceId);

        /// <summary>
        /// Gets all drivers with their team information.
        /// </summary>
        /// <returns>Collection of all driver DTOs.</returns>
        Task<IEnumerable<DriverDto>> GetAllDriversAsync();

        /// <summary>
        /// Pobiera klasyfikację generalną kierowców dla danego sezonu.
        /// </summary>
        Task<IEnumerable<DriverChampionshipDto>> GetDriverChampionshipStandingsAsync(int season);

        /// <summary>
        /// Pobiera szczegółową historię startów konkretnego kierowcy w danym sezonie.
        /// </summary>
        Task<DriverChampionshipDto?> GetDriverChampionshipDetailsAsync(int driverId, int season);

        /// <summary>
        /// Przelicza od nowa całą tabelę klasyfikacji dla wybranego sezonu (przydatne przy korektach).
        /// </summary>
        Task RecalculateChampionshipAsync(int season);

        /// <summary>
        /// Aktualizuje tabelę klasyfikacji o wyniki konkretnego wyścigu.
        /// </summary>
        Task UpdateChampionshipFromRaceResultsAsync(int raceId);

        /// <summary>
        /// Stores race results in the database for finished races from the current season.
        /// Called after race completion or via admin override.
        /// </summary>
        /// <param name="raceId">The ID of the race.</param>
        /// <param name="positions">List of position entries.</param>
        /// <param name="fastestLapDriverId">Optional fastest lap driver ID.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task StoreRaceResultAsync(int raceId, List<PositionEntryDto> positions, int? fastestLapDriverId = null);

        /// <summary>
        /// Retrieves race results from the RaceResult entity (current season only).
        /// Returns null if the race has no stored results or is not from the current season.
        /// </summary>
        /// <param name="raceId">The ID of the race.</param>
        /// <returns>Race result DTO or null if not found.</returns>
        Task<RaceResultDto?> GetStoredRaceResultAsync(int raceId);
    }
}
