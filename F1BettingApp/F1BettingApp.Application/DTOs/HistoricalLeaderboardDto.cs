using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Represents historical leaderboard data for a specific season or time period.
    /// </summary>
    public class HistoricalLeaderboardDto
    {
        /// <summary>
        /// The season identifier (e.g., "2024", "2023").
        /// </summary>
        [Required]
        public string Season { get; set; } = string.Empty;

        /// <summary>
        /// The start date of the historical period.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of the historical period.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The total number of entries in this historical leaderboard.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TotalEntries { get; set; }

        /// <summary>
        /// The list of top players during this period.
        /// </summary>
        public List<LeaderboardEntryDto> Entries { get; set; } = new();

        /// <summary>
        /// Indicates if this is the current season's historical data.
        /// </summary>
        public bool IsCurrentSeason { get; set; }
    }

    /// <summary>
    /// Represents a snapshot of leaderboard state at a specific point in time.
    /// </summary>
    public class LeaderboardSnapshotDto
    {
        /// <summary>
        /// The date and time when the snapshot was taken.
        /// </summary>
        [Required]
        public DateTime SnapshotDate { get; set; }

        /// <summary>
        /// The rank position at this point in time.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Rank { get; set; }

        /// <summary>
        /// The total points at this snapshot.
        /// </summary>
        [Range(0, long.MaxValue)]
        public long TotalPoints { get; set; }

        /// <summary>
        /// The username of the player.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The user ID associated with this snapshot.
        /// </summary>
        public int UserId { get; set; }
    }
}