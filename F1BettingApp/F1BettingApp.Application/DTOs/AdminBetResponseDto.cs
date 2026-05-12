using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for admin view of bets, includes user and race context.
    /// </summary>
    public class AdminBetResponseDto
    {
        /// <summary>
        /// Bet identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User who placed the bet
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username of the bettor
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Race the bet is for
        /// </summary>
        public int RaceId { get; set; }

        /// <summary>
        /// Race name
        /// </summary>
        public string? RaceName { get; set; }

        /// <summary>
        /// Driver the bet is placed on
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Driver name
        /// </summary>
        public string? DriverName { get; set; }

        /// <summary>
        /// Bet amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Odds for this bet
        /// </summary>
        public decimal Odds { get; set; }

        /// <summary>
        /// Bet type
        /// </summary>
        public BetType BetType { get; set; }

        /// <summary>
        /// Bet status
        /// </summary>
        public BetStatus Status { get; set; }

        /// <summary>
        /// Winnings (if bet is resolved)
        /// </summary>
        public decimal? Winnings { get; set; }

        /// <summary>
        /// Potential winnings
        /// </summary>
        public decimal? PotentialWinnings { get; set; }

        /// <summary>
        /// Bet creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Bet resolution timestamp (if applicable)
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }
}
