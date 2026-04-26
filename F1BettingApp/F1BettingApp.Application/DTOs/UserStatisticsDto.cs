namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Data transfer object for user statistics
    /// </summary>
    public class UserStatisticsDto
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the total number of bets placed
        /// </summary>
        public int TotalBets { get; set; }

        /// <summary>
        /// Gets or sets the number of winning bets
        /// </summary>
        public int WinningBets { get; set; }

        /// <summary>
        /// Gets or sets the win rate percentage
        /// </summary>
        public decimal WinRate { get; set; }

        /// <summary>
        /// Gets or sets the total winnings
        /// </summary>
        public decimal TotalWinnings { get; set; }

        /// <summary>
        /// Gets or sets the current points
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets the leaderboard rank
        /// </summary>
        public int Rank { get; set; }
    }
}