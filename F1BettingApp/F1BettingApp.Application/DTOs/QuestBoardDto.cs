namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for the quest board endpoint (user-facing).
    /// Returns all active quest definitions with optional progress for authenticated users.
    /// </summary>
    public class QuestBoardDto
    {
        /// <summary>
        /// Unique quest identifier.
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the quest.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description/tooltip.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category: Betting, Engagement, or Achievement.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Whether this quest is one-time (never resets) or recurring (weekly reset).
        /// </summary>
        public bool IsOneTime { get; set; }

        /// <summary>
        /// Target value to complete the quest.
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        /// Points awarded upon completion.
        /// </summary>
        public int PointsReward { get; set; }

        /// <summary>
        /// Whether this quest is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Display order in the UI (lower values appear first).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Current progress for the authenticated user (only populated if authenticated).
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        /// Whether the quest has been completed by the authenticated user.
        /// </summary>
        public bool? IsCompleted { get; set; }

        /// <summary>
        /// Whether points have been claimed/awarded for the authenticated user.
        /// </summary>
        public bool? IsClaimed { get; set; }
    }
}
