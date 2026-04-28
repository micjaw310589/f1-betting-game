using F1BettingApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for placing a new bet with validation attributes
    /// </summary>
    public class PlaceBetDto
    {
        /// <summary>
        /// ID of the race to place bet on
        /// </summary>
        [Required(ErrorMessage = "Race ID is required")]
        public int RaceId { get; set; }

        /// <summary>
        /// ID of the driver to bet on
        /// </summary>
        [Required(ErrorMessage = "Driver ID is required")]
        public int DriverId { get; set; }

        /// <summary>
        /// Amount to wager (minimum 0.1, maximum 10000)
        /// </summary>
        [Range(0.1, 10000, ErrorMessage = "Amount must be between 0.1 and 10000")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Type of bet: RaceWinner (default), Top3, or Place
        /// </summary>
        [Required(ErrorMessage = "Bet type is required")]
        public BetType BetType { get; set; } = BetType.RaceWinner;

        /// <summary>
        /// Position required to win a place bet (1-3)
        /// Only used when BetType is Place
        /// </summary>
        [Range(1, 3, ErrorMessage = "Place position must be between 1 and 3")]
        public int? PlacePosition { get; set; }

        /// <summary>
        /// Constructor with default values
        /// </summary>
        public PlaceBetDto()
        {
            BetType = BetType.RaceWinner;
            PlacePosition = null;
        }
    }
}