using F1BettingApp.Application.DTOs;
using F1BettingApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Interfaces
{
    /// <summary>
    /// Interface for betting-related operations
    /// </summary>
    public interface IBettingService
    {
        /// <summary>
        /// Places a bet on a specific driver in a race
        /// </summary>
        /// <param name="userId">The authenticated user ID</param>
        /// <param name="dto">The PlaceBetDto containing all bet information</param>
        /// <returns>The created BetResponseDto with complete bet details</returns>
        Task<BetResponseDto> PlaceBetAsync(int userId, PlaceBetDto dto);

        /// <summary>
        /// Cancels an existing bet
        /// </summary>
        /// <param name="betId">The ID of the bet to cancel</param>
        /// <param name="userId">The user attempting to cancel (for authorization)</param>
        /// <returns>The updated BetResponseDto</returns>
        Task<BetResponseDto> CancelBetAsync(int betId, int userId);

        /// <summary>
        /// Gets all bets for a specific user
        /// </summary>
        /// <param name="userId">The user ID from JWT token (string format)</param>
        /// <returns>Collection of BetResponseDto objects</returns>
        Task<IEnumerable<BetResponseDto>> GetUserBetsAsync(int userId);

        /// <summary>
        /// Gets a specific bet by ID
        /// </summary>
        /// <param name="betId">The ID of the bet</param>
        /// <param name="userId">The user requesting the bet (for authorization)</param>
        /// <returns>The BetResponseDto or null if not found</returns>
        Task<BetResponseDto?> GetBetByIdAsync(int betId, int userId);

        /// <summary>
        /// Processes race results and updates bet statuses
        /// </summary>
        /// <param name="raceId">The ID of the completed race to process</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ProcessRaceResultsAsync(int raceId);

        /// <summary>
        /// Calculates winnings for a bet based on race results
        /// </summary>
        /// <param name="bet">The bet to calculate winnings for</param>
        /// <param name="result">The race result containing outcome information</param>
        /// <returns>The calculated winnings amount</returns>
        Task<decimal> CalculateWinningsAsync(Bet bet, Result result);

        /// <summary>
        /// Gets user's bet history with pagination support
        /// </summary>
        /// <param name="userId">The user ID from JWT token</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paginated bet history with metadata</returns>
        Task<BetHistoryResponseDto> GetUserBetHistoryAsync(int userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Validates a bet before placing it (without creating)
        /// </summary>
        /// <param name="userId">The authenticated user ID</param>
        /// <param name="dto">The PlaceBetDto to validate</param>
        /// <returns>Validation result with any errors found</returns>
        Task<BetValidationResult> ValidateBetAsync(int userId, PlaceBetDto dto);

        /// <summary>
        /// Gets available races that can accept bets
        /// </summary>
        /// <param name="userId">The user ID for authorization</param>
        /// <returns>List of available races with betting information</returns>
        Task<IEnumerable<RaceDetailDto>> GetAvailableRacesAsync(int userId);
    }

    /// <summary>
    /// Result of bet validation
    /// </summary>
    public class BetValidationResult
    {
        /// <summary>
        /// Whether the bet is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors if invalid
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Pre-calculated odds for the bet (if valid)
        /// </summary>
        public decimal? Odds { get; set; }

        /// <summary>
        /// Potential winnings (if valid)
        /// </summary>
        public decimal? PotentialWinnings { get; set; }
    }
}