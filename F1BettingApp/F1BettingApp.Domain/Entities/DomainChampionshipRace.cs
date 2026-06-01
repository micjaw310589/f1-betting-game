using F1BettingApp.Domain.Entities;

namespace F1BettingGame.Domain.Entities // Zmień na swój właściwy namespace
{
    public class DriverChampionshipRace
    {
        public int Id { get; set; }
        public int DriverChampionshipId { get; set; }
        public DriverChampionship DriverChampionship { get; set; }
        public int RaceId { get; set; }
        public Race Race { get; set; }
        public int PointsEarned { get; set; }
        public int Position { get; set; }
    }
}