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
        Task SyncRaceDataFromOpenF1Async();

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
        /// Gets race results for a completed race
        /// </summary>
        /// <param name="raceId">The ID of the race</param>
        /// <returns>Collection of results for the race</returns>
        Task<IEnumerable<Result>> GetResultsAsync(int raceId);
    }
}
