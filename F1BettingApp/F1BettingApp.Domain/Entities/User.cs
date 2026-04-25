using System.Collections.Generic;

namespace F1BettingApp.Domain.Entities
{
    public class User
    {
        public User()
        {
            Bets = new List<Bet>();
            Notifications = new List<Notification>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int Points { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Bet> Bets { get; set; }
        public ICollection<Notification> Notifications { get; set; }

        public void AddPoints(int amount)
        {
            if (amount <= 0)
                return;

            Points += amount;
        }

        public bool DeductPoints(int amount)
        {
            if (amount <= 0 || !HasSufficientBalance(amount))
                return false;

            Points -= amount;
            return true;
        }

        public bool HasSufficientBalance(int amount)
        {
            return amount > 0 && Points >= amount;
        }
    }
}