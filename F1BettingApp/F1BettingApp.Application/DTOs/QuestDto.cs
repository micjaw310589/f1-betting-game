namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO representing a quest with the user's current progress.
    /// </summary>
    public class QuestDto
    {
        /// <summary>
        /// Numeric ID for admin operations.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Number of users who have completed this quest (lifetime count).
        /// </summary>
        public int CompletedCount { get; set; }

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
        /// Current progress value.
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Whether the quest has been completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Whether points have been claimed/awarded.
        /// </summary>
        public bool IsClaimed { get; set; }

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
    }

    /// <summary>
    /// Wrapper DTO for the quests API response.
    /// </summary>
    public class QuestResponseDto
    {
        public List<QuestDto> Quests { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a new quest definition (admin).
    /// </summary>
    public class CreateQuestDto
    {
        public string QuestId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsOneTime { get; set; }
        public int Target { get; set; }
        public int PointsReward { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing quest definition (admin).
    /// </summary>
    public class UpdateQuestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool? IsOneTime { get; set; }
        public int? Target { get; set; }
        public int? PointsReward { get; set; }
        public bool? IsActive { get; set; }
        public int? Order { get; set; }
    }

    /// <summary>
    /// DTO for toggling quest active status (admin).
    /// </summary>
    public class ToggleQuestActiveDto
    {
        public bool IsActive { get; set; }
    }
}
