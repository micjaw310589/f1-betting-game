using F1BettingApp.Domain.Enums;
using System;
using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// Race results DTO with comprehensive race outcome data
    /// </summary>
    public class RaceResultDto
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; }
        public string Circuit { get; set; }
        public string Country { get; set; }
        public DateTime RaceDate { get; set; }
        public int WinnerDriverId { get; set; }
        public string WinnerDriverName { get; set; }
        public int WinnerTeamId { get; set; }
        public string WinnerTeamName { get; set; }
        public int WinningMargin { get; set; }
        public int FastestLapDriverId { get; set; }
        public string FastestLapDriverName { get; set; }
        public int PolePositionDriverId { get; set; }
        public string PolePositionDriverName { get; set; }
        public int SafetyCar { get; set; }
        public int VirtualSafetyCar { get; set; }
        public int RedFlag { get; set; }
        public int YellowFlag { get; set; }
        public int BlackFlag { get; set; }
        public int BlueFlag { get; set; }
        public int BlackAndWhiteFlag { get; set; }
        public int ChequeredFlag { get; set; }
        public int RaceDistance { get; set; }
        public int RaceDistanceUnit { get; set; }
        public int Laps { get; set; }
        public int LapsCompleted { get; set; }
        public int LapsToFinish { get; set; }
        public int RaceControlMessage { get; set; }
        public string RaceControlMessageText { get; set; }
        public string TimeAttack { get; set; }
        public string TimeAttackResult { get; set; }
        public string TimeAttackComment { get; set; }
        public string TimeAttackStatus { get; set; }
        public string TimeAttackLaps { get; set; }
    }
}