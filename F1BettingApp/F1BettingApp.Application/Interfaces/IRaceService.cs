using F1BettingApp.Application.DTOs;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Service interface for managing race data operations.
    /// </summary>
    public interface IRaceService
    {
        /// <summary>
        /// Retrieves a race by its ID.
        /// </summary>
        Task<DTOs.RaceDto> GetRaceByIdAsync(int id);

        /// <summary>
        /// Retrieves all races.
        /// </summary>
        Task<IEnumerable<DTOs.RaceDto>> GetAllRacesAsync();

        /// <summary>
        /// Retrieves upcoming scheduled races.
        /// </summary>
        Task<IEnumerable<DTOs.RaceDto>> GetUpcomingRacesAsync();

        /// <summary>
        /// Retrieves upcoming races with betting odds.
        /// </summary>
        Task<IEnumerable<DTOs.RaceDto>> GetUpcomingRacesWithOddsAsync();

        /// <summary>
        /// Retrieves races by their IDs.
        /// </summary>
        Task<IEnumerable<DTOs.RaceDto>> GetRacesByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Synchronizes race data from the OpenF1 API.
        /// </summary>
        Task<SyncResultDto> SyncRaceDataFromOpenF1Async();

        /// <summary>
        /// Updates the status of a race.
        /// </summary>
        Task UpdateRaceStatusAsync(int raceId, RaceStatus newStatus);

        /// <summary>
        /// Retrieves results for a specific race.
        /// </summary>
        Task<IEnumerable<Result>> GetResultsAsync(int raceId);

        /// <summary>
        /// Overrides race results with custom data.
        /// </summary>
        Task OverrideRaceResultAsync(int raceId, OverrideRaceResultDto dto);

        /// <summary>
        /// Retrieves detailed race results as a DTO.
        /// </summary>
        Task<DTOs.RaceResultDto> GetRaceResultDtoAsync(int raceId);

        /// <summary>
        /// Updates race metadata (name, date, circuit, country, status).
        /// </summary>
        Task UpdateRaceMetadataAsync(int raceId, UpdateRaceMetadataDto dto);

        /// <summary>
        /// Creates a new race.
        /// </summary>
        Task<DTOs.RaceDto> CreateRaceAsync(CreateRaceDto dto);

        /// <summary>
        /// Deletes a race by its ID.
        /// </summary>
        Task DeleteRaceAsync(int raceId);

        /// <summary>
        /// Retrieves drivers with their betting odds for a specific race.
        /// </summary>
        Task<IEnumerable<DriverWithOddsDto>> GetDriversWithOddsForRaceAsync(int raceId);
    }
}