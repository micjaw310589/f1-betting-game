namespace F1BettingApp.Application.DTOs
{
    public class RaceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Circuit { get; set; }
        public DateTime RaceDate { get; set; }
        public string Country { get; set; }
    }
}