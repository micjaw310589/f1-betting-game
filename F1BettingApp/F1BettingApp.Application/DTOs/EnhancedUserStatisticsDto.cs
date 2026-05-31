using System;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Enhanced data transfer object for comprehensive user statistics
    /// </summary>
    public class EnhancedUserStatisticsDto : UserStatisticsDto
    {
        // New enhanced fields
        public int LosingBets { get; set; }
        public int PushBets { get; set; } // Refunded bets
        public decimal ReturnOnInvestment { get; set; } // ROI percentage
        public int CurrentWinStreak { get; set; }
        public int CurrentLoseStreak { get; set; }
        public int LongestWinStreak { get; set; }
        public int FavoriteDriverId { get; set; }
        public string FavoriteDriverName { get; set; }
        public decimal AverageBetAmount { get; set; }
        public decimal LargestWin { get; set; }
        public decimal LargestLoss { get; set; }
        public DateTime? LastBetDate { get; set; }
        public decimal TotalAmountBet { get; set; }
        public int BetsThisWeek { get; set; }
        public int BetsThisMonth { get; set; }
    }
}