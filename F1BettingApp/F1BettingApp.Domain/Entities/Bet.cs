namespace F1BettingApp.Domain.Entities
{
    public class Bet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RaceId { get; set; }
        public int DriverId { get; set; }
        public decimal Amount { get; set; }
        public BetStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}