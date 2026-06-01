namespace F1BettingApp.Domain.Entities
{
    /// <summary>
    /// Records a single point change (earning or spending) for audit trail and history.
    /// </summary>
    public class PointHistory
    {
        /// <summary>
        /// Unique identifier for this history entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the User entity.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Points change: positive = earned, negative = spent.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Category of the point change (e.g. "DailyLogin", "Quest", "BetWin", "BetLoss", "BetPlacement", "BetCancellation", "AdminAdjustment").
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable description of the point change.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional reference ID (e.g. bet id, quest id, or other related entity id).
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

        /// <summary>
        /// Navigation property to the associated user.
        /// </summary>
        public virtual User? User { get; set; }
    }
}
