using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for bet data transfer between layers
    /// </summary>
    public class BetDto
    {
        /// <summary>
        /// Unique identifier for the bet
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User who placed the bet
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Race associated with this bet
        /// </summary>
        public int RaceId { get; set; }

        /// <summary>
        /// Driver selected for the bet
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Amount wagered on this bet
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Type of bet placed (RaceWinner, Top3, Place)
        /// </summary>
        public BetType BetType { get; set; }

        /// <summary>
        /// Odds for this specific bet selection
        /// </summary>
        public decimal Odds { get; set; }

        /// <summary>
        /// Potential winnings if the bet wins (Amount * Odds)
        /// </summary>
        public decimal? PotentialWinnings { get; set; }

        /// <summary>
        /// Actual winnings received when bet is won
        /// </summary>
        public decimal? Winnings { get; set; }

        /// <summary>
        /// Position required to win a place bet (1-3)
        /// </summary>
        public int? PlacePosition { get; set; }

        /// <summary>
        /// Current status of the bet
        /// </summary>
        public BetStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the bet was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the bet was resolved (if applicable)
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }
}
