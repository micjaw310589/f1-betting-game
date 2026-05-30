using System;
using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs // Dopasuj namespace
{
    public class DriverChampionshipDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverCountry { get; set; }
        public string TeamName { get; set; }
        public int Season { get; set; }
        public int TotalPoints { get; set; }
        public int Position { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<DriverChampionshipRaceDto> RaceResults { get; set; } = new List<DriverChampionshipRaceDto>();
    }
}