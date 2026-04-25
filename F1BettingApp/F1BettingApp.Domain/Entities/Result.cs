using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace F1BettingApp.Domain.Entities
{
    public class Result
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign Keys
        public int RaceId { get; set; }
        public Race Race { get; set; }
        public int DriverId { get; set; }
        public Driver Driver { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } // Assuming we track which user's result this is tied to, or if it's just a record of the race. Based on the properties, I'll keep it focused on the race outcome.

        [Required]
        public int Position { get; set; }

        public int Points { get; set; }

        // Could be null if not applicable
        public TimeSpan? FastestLap { get; set; } 
        public TimeSpan? PitStopTime { get; set; }

        public Result(int raceId, int driverId, int position, int points, TimeSpan fastestLap, TimeSpan? pitStopTime)
        {
            // Implement basic validation logic in constructor
            if (raceId <= 0 || driverId <= 0 || position <= 0 || points < 0)
            {
                throw new ArgumentException("Invalid race, driver, position, or points data provided.");
            }
            
            this.RaceId = raceId;
            this.DriverId = driverId;
            this.Position = position;
            this.Points = points;
            this.FastestLap = fastestLap;
            this.PitStopTime = pitStopTime;
        }

        /// <summary>
        /// Checks if the driver finished in a podium position (1st, 2nd, or 3rd).
        /// </summary>
        /// <returns>True if the position is 1, 2, or 3.</returns>
        public bool IsPodiumFinish()
        {
            return Position <= 3;
        }

        /// <summary>
        /// Checks if the driver earned points for the race.
        /// </summary>
        /// <returns>True if the points awarded are greater than 0.</returns>
        public bool IsPointsFinish()
        {
            return Points > 0;
        }
    }
}