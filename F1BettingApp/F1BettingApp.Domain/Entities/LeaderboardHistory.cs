using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1BettingApp.Domain.Entities
{
    public class LeaderboardHistory
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign Keys
        public int UserId { get; set; }
        public User User { get; set; }

        public int RaceId { get; set; }
        public Race Race { get; set; }

        [Required]
        public string Season { get; set; }

        public int TotalPoints { get; set; }
        public int Rank { get; set; }

        public DateTime CreatedAt { get; set; }

        public LeaderboardHistory(int userId, int raceId, string season, int totalPoints, int rank)
        {
            // Implement validation logic
            if (userId <= 0 || raceId <= 0 || string.IsNullOrWhiteSpace(season) || totalPoints < 0 || rank <= 0)
            {
                throw new ArgumentException("Invalid parameters provided for LeaderboardHistory.");
            }
            
            this.UserId = userId;
            this.RaceId = raceId;
            this.Season = season;
            this.TotalPoints = totalPoints;
            this.Rank = rank;
            this.CreatedAt = DateTime.UtcNow;
        }
    }
}