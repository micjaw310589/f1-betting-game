using F1BettingApp.Domain.Enums;
using System;
using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs
{
    public class UserBetAnalysisDto
    {
        public int UserId { get; set; }
        public Dictionary<BetType, BetTypeAnalysisDto> BetTypeAnalysis { get; set; }
        public Dictionary<int, DriverAnalysisDto> DriverAnalysis { get; set; }
        public Dictionary<int, TeamAnalysisDto> TeamAnalysis { get; set; }
        public MonthlyAnalysisDto[] MonthlyAnalysis { get; set; }
        public TimeOfDayAnalysisDto TimeOfDayAnalysis { get; set; }
    }

    public class BetTypeAnalysisDto
    {
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal WinRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalWinnings { get; set; }
        public decimal ROI { get; set; }
    }

    public class DriverAnalysisDto
    {
        public string DriverName { get; set; }
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal WinRate { get; set; }
        public decimal TotalWinnings { get; set; }
    }

    public class TeamAnalysisDto
    {
        public string TeamName { get; set; }
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal WinRate { get; set; }
        public decimal TotalWinnings { get; set; }
    }

    public class MonthlyAnalysisDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalBets { get; set; }
        public int WinningBets { get; set; }
        public decimal TotalWinnings { get; set; }
    }

    public class TimeOfDayAnalysisDto
    {
        public int MorningBets { get; set; }
        public int AfternoonBets { get; set; }
        public int EveningBets { get; set; }
        public int NightBets { get; set; }
        public decimal MorningWinRate { get; set; }
        public decimal AfternoonWinRate { get; set; }
        public decimal EveningWinRate { get; set; }
        public decimal NightWinRate { get; set; }
    }
}