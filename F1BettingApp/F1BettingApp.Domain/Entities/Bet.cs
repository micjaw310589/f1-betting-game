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

        public User? User { get; set; }
        public Race? Race { get; set; }
        public Driver? Driver { get; set; }

        public decimal CalculatePotentialWinnings()
        {
            var winnings = Amount * Odds;
            PotentialWinnings = winnings;
            return winnings;
        }

        public bool ValidateBet()
        {
            return Amount > 0 && Odds > 0 && BetType != default && UserId > 0 && RaceId > 0 && DriverId > 0;
        }

        public bool IsWon() => Status == BetStatus.Won;
        public bool IsLost() => Status == BetStatus.Lost;
        public bool IsPending() => Status == BetStatus.Pending;
    }
}