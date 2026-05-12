using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for updating a bet as an admin.
    /// All fields are optional to allow partial updates.
    /// </summary>
    public class UpdateBetDto
    {
        /// <summary>
        /// ID of the driver (for changing the selection)
        /// </summary>
        public int? DriverId { get; set; }

        /// <summary>
        /// Bet amount
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Type of bet
        /// </summary>
        public BetType? BetType { get; set; }

        /// <summary>
        /// Bet status (for admin override)
        /// </summary>
        public BetStatus? Status { get; set; }

        /// <summary>
        /// Winnings (for admin override)
        /// </summary>
        public decimal? Winnings { get; set; }
    }
}
