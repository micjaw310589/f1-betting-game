namespace F1BettingApp.Domain.Entities
{
    /// <summary>
    /// Tracks a user's daily login streak and points awarded for consecutive logins.
    /// </summary>
    public class DailyLoginStreak
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the User entity.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Number of consecutive days the user has logged in.
        /// Resets to 1 on a new streak, or resets to 0 if a day is missed.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// The UTC date of the last login.
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Whether daily points have already been claimed for today.
        /// </summary>
        public bool ClaimedToday { get; set; }

        /// <summary>
        /// Last time this record was updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the associated user.
        /// </summary>
        public virtual User? User { get; set; }
    }
}
