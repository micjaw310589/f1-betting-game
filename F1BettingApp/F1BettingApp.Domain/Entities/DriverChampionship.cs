using System;
using System.Collections.Generic;
using F1BettingApp.Domain.Entities;

namespace F1BettingGame.Domain.Entities // Zmień na swój właściwy namespace
{
    public class DriverChampionship
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public Driver Driver { get; set; }
        public int Season { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
        public DateTime LastUpdated { get; set; }
        
        // Relacja powiązana z wyścigami w danym sezonie
        public ICollection<DriverChampionshipRace> RaceResults { get; set; } = new List<DriverChampionshipRace>();
    }
}