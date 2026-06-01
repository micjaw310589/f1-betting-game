namespace F1BettingApp.Application.DTOs // Dopasuj namespace
{
    public class DriverChampionshipRaceDto
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; }
        public int PointsEarned { get; set; }
        public int PositionInRace { get; set; }
    }
}