namespace F1BettingApp.Application.DTOs
{
    /// <summary>
    /// DTO for creating a new race (admin only).
    /// </summary>
    public class CreateRaceDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public string Circuit { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Season { get; set; } = 2025;
    }
}
