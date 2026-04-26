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
        public string? ProfileImageUrl { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        public User(string username, string email, string passwordHash) : this(username, email, passwordHash, true, false)
        {
            // Additional validation can be added here or in dedicated validation methods
        }

        public User(string username, string email, string passwordHash, bool isActive, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            }
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                throw new ArgumentException("Invalid email format.", nameof(email));
            }
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }
            if (Points < 0)
            {
                throw new InvalidOperationException("Points cannot be negative.");
            }
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Points = 10000; // Start with 0 points or initial points if provided
            CreatedAt = DateTime.Now;
            IsActive = isActive;
            IsAdmin = isAdmin;
            LastLogin = null;
        }

        public void AddPoints(int points)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Points must be positive.", nameof(points));
            }
            Points += points;
        }

        public void DeductPoints(int points)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Points must be positive.", nameof(points));
            }
            if (Points < points)
            {
                throw new InvalidOperationException("Insufficient balance to deduct points.");
            }
            Points -= points;
        }

        public bool HasSufficientBalance(int requiredPoints)
        {
            return Points >= requiredPoints;
        }
    }
}