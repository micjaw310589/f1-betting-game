namespace F1BettingApp.Domain.Entities
{
    /// <summary>
    /// Tracks per-user progress toward completing a quest.
    /// For recurring quests, tracked per ISO week. For one-time quests, uses sentinel week 0.
    /// </summary>
    public class WeeklyQuestProgress
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the User entity.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The quest identifier (matches QuestDefinition.QuestId).
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// ISO week number for the tracking period.
        /// For one-time quests, always 0.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Year for disambiguation.
        /// For one-time quests, always 0.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Current progress value (e.g., number of bets placed).
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Target value from the quest definition.
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        /// Whether the quest has been completed (progress >= target).
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Points already awarded for this quest instance.
        /// </summary>
        public int PointsAwarded { get; set; }

        /// <summary>
        /// Whether the points have been claimed/awarded.
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// Optional reference ID for special tracking (e.g., last counted date for consistent_bettor quest).
        /// </summary>
        public string? ReferenceId { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the associated user.
        /// </summary>
        public virtual User? User { get; set; }
    }
}
