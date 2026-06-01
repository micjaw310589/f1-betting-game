using System;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO representing a single point history entry.
    /// </summary>
    public class PointHistoryDto
    {
        /// <summary>
        /// Unique identifier for this history entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Points change: positive = earned, negative = spent.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Category of the point change (e.g. "DailyLogin", "Quest", "BetWin", "BetLoss", "BetPlacement", "AdminAdjustment").
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable description of the point change.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional reference ID (e.g. bet id, quest id).
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Source of the point change ("System", "Admin", "Bet").
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when this point change occurred.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
