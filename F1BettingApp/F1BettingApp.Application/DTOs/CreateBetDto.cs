using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for creating a bet as an admin.
    /// </summary>
    public class CreateBetDto
    {
        /// <summary>
        /// ID of the user placing the bet
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// ID of the race
        /// </summary>
        public int RaceId { get; set; }

        /// <summary>
        /// ID of the driver
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Bet amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Type of bet
        /// </summary>
        public BetType BetType { get; set; }
    }
}
