namespace F1BettingApp.Domain.Entities
{
    /// <summary>
    /// Represents a quest definition — an admin-configurable template for quests.
    /// Quests can be recurring (weekly reset) or one-time (lifetime).
    /// </summary>
    public class QuestDefinition
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique identifier for the quest (e.g., "betting_marathon").
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the quest.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description/tooltip shown to the user.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category: Betting, Engagement, or Achievement.
        /// </summary>
        public QuestCategory Category { get; set; }

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
        /// Whether this quest is currently active and visible to users.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Display order in the UI (lower values appear first).
        /// </summary>
        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Quest category enum.
    /// </summary>
    public enum QuestCategory
    {
        Betting,
        Engagement,
        Achievement
    }
}
