using F1BettingApp.Domain.Enums;
using System;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for bet response with complete bet information
    /// </summary>
    public class BetResponseDto
    {
        /// <summary>
        /// Bet identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User who placed the bet
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Race the bet is for
        /// </summary>
        public int RaceId { get; set; }

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
        /// Bet creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Bet resolution timestamp (if resolved)
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Prediction value (if applicable)
        /// </summary>
        public int? Prediction { get; set; }

        /// <summary>
        /// Prediction result (if bet resolved)
        /// </summary>
        public bool? PredictionResult { get; set; }
    }
}