namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO representing the current daily login streak information.
    /// </summary>
    public class DailyStreakInfoDto
    {
        /// <summary>
        /// Current consecutive login streak count.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// The UTC date of the last login (ISO 8601 format).
        /// </summary>
        public string LastLoginDate { get; set; } = string.Empty;

        /// <summary>
        /// Points awarded for today's login (0 if not yet claimed).
        /// </summary>
        public int PointsToday { get; set; }

        /// <summary>
        /// Whether daily points have already been claimed for today.
        /// </summary>
        public bool ClaimedToday { get; set; }

        /// <summary>
        /// The streak day at which the next bonus multiplier kicks in.
        /// Null if the user has reached the maximum multiplier (7+ days).
        /// </summary>
        public int? NextBonusMilestone { get; set; }

        /// <summary>
        /// Points the user will earn when reaching the next bonus milestone.
        /// Null if the user has reached the maximum multiplier.
        /// </summary>
        public int? PointsAtNextMilestone { get; set; }
    }
}
