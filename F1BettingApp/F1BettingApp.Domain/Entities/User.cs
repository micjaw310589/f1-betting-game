using Microsoft.AspNetCore.Identity;

namespace F1BettingApp.Domain.Entities
{
    public class User : Microsoft.AspNetCore.Identity.IdentityUser<int>
    {
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        public User(string userName, string email, string passwordHash) : this(userName, email, passwordHash, true, false)
        {
            // Additional validation can be added here or in dedicated validation methods
        }

        public User(string userName, string email, string passwordHash, bool isActive, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("Username cannot be empty.", nameof(userName));
            }
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                throw new ArgumentException("Invalid email format.", nameof(email));
            }
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }

            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            Points = 10000; // Start with 0 points or initial points if provided
            CreatedAt = DateTime.UtcNow;
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
