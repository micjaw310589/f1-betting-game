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
        // UserId is optional - race results are not tied to a specific user
        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int Position { get; set; }

        public int Points { get; set; }

        // Could be null if not applicable
        public TimeSpan? FastestLap { get; set; } 
        public TimeSpan? PitStopTime { get; set; }

        // Parameterless constructor required by EF Core
        public Result() { }

        public Result(int raceId, int driverId, int position, int points, TimeSpan fastestLap, TimeSpan? pitStopTime)
            : this(raceId, driverId, position, points, fastestLap, pitStopTime, null)
        {
        }

        public Result(int raceId, int driverId, int position, int points, TimeSpan fastestLap, TimeSpan? pitStopTime, int? userId)
        {
            // Validate raceId - must be positive
            if (raceId <= 0)
            {
                throw new ArgumentException("RaceId must be greater than 0.");
            }
            
            // Validate driverId - must be positive
            if (driverId <= 0)
            {
                throw new ArgumentException("DriverId must be greater than 0.");
            }
            
            // If position is 0 (DNF - Did Not Finish), points must also be 0
            if (position == 0 && points != 0)
            {
                throw new ArgumentException("Position 0 (DNF) requires points to be 0.");
            }
            
            // Validate points are non-negative for finishing positions
            if (points < 0)
            {
                throw new ArgumentException("Points cannot be negative.");
            }
            
            // Validate position range (1-20 for finishing positions, 0 for DNF)
            if ((position != 0 && position < 1) || position > 22)
            {
                throw new ArgumentException("Position must be between 0 and 20.");
            }
            
            this.RaceId = raceId;
            this.DriverId = driverId;
            this.Position = position;
            this.Points = points;
            this.FastestLap = fastestLap;
            this.PitStopTime = pitStopTime;
            this.UserId = userId;
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