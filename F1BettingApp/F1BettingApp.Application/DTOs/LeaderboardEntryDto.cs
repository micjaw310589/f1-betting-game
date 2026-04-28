using System;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Represents a single entry in the leaderboard.
    /// </summary>
    public class LeaderboardEntryDto
    {
        /// <summary>
        /// The rank position of the player (1-based).
        /// </summary>
        [Required]
        public int Rank { get; set; }

        /// <summary>
        /// The username or display name of the player.
        /// </summary>
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The total points accumulated by the player.
        /// </summary>
        [Range(0, int.MaxValue)]
        public long TotalPoints { get; set; }

        /// <summary>
        /// The number of bets placed by the player.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int BetsPlaced { get; set; }

        /// <summary>
        /// The win rate as a percentage (0-100).
        /// </summary>
        [Range(0, 100)]
        public double WinRate { get; set; }

        /// <summary>
        /// The total profit/loss for the player.
        /// </summary>
        public long ProfitLoss { get; set; }

        /// <summary>
        /// Indicates if this is the current user's entry.
        /// </summary>
        public bool IsCurrentUser { get; set; }
    }
}