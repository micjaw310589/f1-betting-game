using F1BettingApp.Domain.Enums;
using System.Collections.Generic;

namespace F1BettingApp.Application.DTOs
{
    public class RaceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Circuit { get; set; }
        public DateTime RaceDate { get; set; }
        public string Country { get; set; }
        public RaceStatus Status { get; set; }
        public Dictionary<int, decimal> Odds { get; set; } = new Dictionary<int, decimal>();
    }
}
