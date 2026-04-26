using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Domain.Entities
{
    public class Bet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RaceId { get; set; }
        public int DriverId { get; set; }
        public BetType BetType { get; set; }
        public decimal Amount { get; set; }
        public decimal Odds { get; set; }
        public decimal PotentialWinnings { get; set; }
        public BetStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        // New properties
        public BetType BetType { get; set; }
        public decimal Odds { get; set; }
        public decimal PotentialWinnings { get; set; }

        public Bet(int userId, int raceId, int driverId, decimal amount, BetType betType, decimal odds)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Bet amount must be positive.", nameof(amount));
            }
            if (odds <= 0)
            {
                throw new ArgumentException("Bet odds must be positive.", nameof(odds));
            }
            
            // Initialize default potential winnings (assuming 1 bet unit for now, but calculated based on odds later)
            PotentialWinnings = amount * odds;

            UserId = userId;
            RaceId = raceId;
            DriverId = driverId;
            Amount = amount;
            BetType = betType;
            Odds = odds;
            Status = BetStatus.Pending;
            CreatedAt = DateTime.Now;
        }

        public Bet() { }

        // Validation logic (assuming User.HasSufficientBalance and other services handle point checks)
        public void ValidateBet()
        {
            if (BetType <= 0)
            {
                throw new InvalidOperationException("Invalid Bet Type specified.");
            }
            if (Odds <= 0)
            {
                throw new InvalidOperationException("Odds must be greater than zero.");
            }
            // Further validation based on race status and bet type would happen in the Service layer
        }
    }
}