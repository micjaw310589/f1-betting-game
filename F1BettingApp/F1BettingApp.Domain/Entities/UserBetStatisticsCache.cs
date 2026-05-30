using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1BettingApp.Domain.Entities
{
    /// <summary>
    /// Cache table for user bet statistics to improve performance
    /// </summary>
    public class UserBetStatisticsCache
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public int LosingBets { get; set; }
        public int PushBets { get; set; }
        public decimal TotalWinnings { get; set; }
        public decimal TotalAmountBet { get; set; }
        public int CurrentWinStreak { get; set; }
        public int CurrentLoseStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public DateTime LastUpdated { get; set; }
        public int FavoriteDriverId { get; set; }
        public decimal LargestWin { get; set; }
        public decimal LargestLoss { get; set; }
    }
}