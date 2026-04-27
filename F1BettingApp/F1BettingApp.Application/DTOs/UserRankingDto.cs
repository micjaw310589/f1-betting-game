using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Represents the ranking information for a specific user.
    /// </summary>
    public class UserRankingDto
    {
        /// <summary>
        /// The unique identifier for the user.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// The username or display name of the user.
        /// </summary>
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The current rank position of the user in the leaderboard.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int CurrentRank { get; set; }

        /// <summary>
        /// The total points accumulated by the user.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long TotalPoints { get; set; }

        /// <summary>
        /// The number of bets placed by the user.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int BetsPlaced { get; set; }

        /// <summary>
        /// The win rate as a percentage (0-100).
        /// </summary>
        [Range(0, 100)]
        public double WinRate { get; set; }

        /// <summary>
        /// The total profit/loss for the user.
        /// </summary>
        public long ProfitLoss { get; set; }

        /// <summary>
        /// The number of users ranked above this user.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int UsersAbove { get; set; }

        /// <summary>
        /// The rank difference from the previous session (positive = dropped).
        /// </summary>
        public int RankChange { get; set; }

        /// <summary>
        /// The points needed to reach the next rank.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long PointsToNextRank { get; set; }

        /// <summary>
        /// Indicates if this is the current user's ranking.
        /// </summary>
        public bool IsCurrentUser { get; set; } = true;
    }
}