namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO representing the weekly point summary for a user.
    /// </summary>
    public class WeeklyPointSummaryDto
    {
        /// <summary>
        /// ISO week number.
        /// </summary>
        public int WeekNumber { get; set; }

        /// <summary>
        /// Year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Total points earned during the week.
        /// </summary>
        public int TotalEarned { get; set; }

        /// <summary>
        /// Total points spent during the week.
        /// </summary>
        public int TotalSpent { get; set; }

        /// <summary>
        /// Net points change for the week (earned - spent).
        /// </summary>
        public int NetChange => TotalEarned - TotalSpent;
    }
}
