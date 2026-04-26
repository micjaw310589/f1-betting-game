using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Tests.Builders
{
    /// <summary>
    /// Builder for creating test Result entities
    /// </summary>
    public class ResultBuilder
    {
        private int _id = 1;
        private int _raceId = 1;
        private int _driverId = 1;
        private int _position = 1;
        private int _points = 25;
        private TimeSpan? _fastestLap = TimeSpan.FromMinutes(1);
        private TimeSpan? _pitStopTime = TimeSpan.FromSeconds(20);

        /// <summary>
        /// Sets the result ID
        /// </summary>
        /// <param name="id">The result ID</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the race ID
        /// </summary>
        /// <param name="raceId">The race ID</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithRaceId(int raceId)
        {
            _raceId = raceId;
            return this;
        }

        /// <summary>
        /// Sets the driver ID
        /// </summary>
        /// <param name="driverId">The driver ID</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithDriverId(int driverId)
        {
            _driverId = driverId;
            return this;
        }

        /// <summary>
        /// Sets the position
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithPosition(int position)
        {
            _position = position;
            return this;
        }

        /// <summary>
        /// Sets the points
        /// </summary>
        /// <param name="points">The points</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithPoints(int points)
        {
            _points = points;
            return this;
        }

        /// <summary>
        /// Sets the fastest lap time
        /// </summary>
        /// <param name="fastestLap">The fastest lap time</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithFastestLap(TimeSpan fastestLap)
        {
            _fastestLap = fastestLap;
            return this;
        }

        /// <summary>
        /// Sets the pit stop time
        /// </summary>
        /// <param name="pitStopTime">The pit stop time</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithPitStopTime(TimeSpan pitStopTime)
        {
            _pitStopTime = pitStopTime;
            return this;
        }

        /// <summary>
        /// Removes the fastest lap time
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithoutFastestLap()
        {
            _fastestLap = null;
            return this;
        }

        /// <summary>
        /// Removes the pit stop time
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithoutPitStopTime()
        {
            _pitStopTime = null;
            return this;
        }

        /// <summary>
        /// Sets the result as a podium finish (position 1-3)
        /// </summary>
        /// <param name="position">The podium position (1, 2, or 3)</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder AsPodiumFinish(int position)
        {
            if (position < 1 || position > 3)
                throw new ArgumentException("Podium position must be between 1 and 3", nameof(position));

            _position = position;
            _points = position switch
            {
                1 => 25,
                2 => 18,
                3 => 15,
                _ => 0
            };
            return this;
        }

        /// <summary>
        /// Sets the result as a DNF (Did Not Finish)
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder AsDNF()
        {
            _position = 0;
            _points = 0;
            _fastestLap = null;
            _pitStopTime = null;
            return this;
        }

        /// <summary>
        /// Builds the Result entity
        /// </summary>
        /// <returns>The constructed Result entity</returns>
        public Result Build()
        {
            // Use the Result constructor for validation
            var result = new Result(_raceId, _driverId, _position, _points, _fastestLap ?? TimeSpan.Zero, _pitStopTime)
            {
                Id = _id
            };

            // Handle nullable times properly
            if (_fastestLap == null)
                result.FastestLap = null;
            if (_pitStopTime == null)
                result.PitStopTime = null;

            return result;
        }

        /// <summary>
        /// Builds a list of results with sequential IDs
        /// </summary>
        /// <param name="count">The number of results to create</param>
        /// <returns>List of Result entities</returns>
        public List<Result> BuildList(int count)
        {
            var results = new List<Result>();
            for (int i = 0; i < count; i++)
            {
                results.Add(WithId(_id + i).WithPosition(i + 1).Build());
            }
            return results;
        }

        /// <summary>
        /// Builds a race result with typical positions and points
        /// </summary>
        /// <returns>List of Result entities representing a full race result</returns>
        public List<Result> BuildRaceResults()
        {
            return new List<Result>
            {
                Build(), // Position 1
                new ResultBuilder().WithId(2).WithDriverId(2).WithPosition(2).WithPoints(18).Build(), // Position 2
                new ResultBuilder().WithId(3).WithDriverId(3).WithPosition(3).WithPoints(15).Build(), // Position 3
                new ResultBuilder().WithId(4).WithDriverId(4).WithPosition(4).WithPoints(12).Build(), // Position 4
                new ResultBuilder().WithId(5).WithDriverId(5).WithPosition(5).WithPoints(10).Build(), // Position 5
                new ResultBuilder().WithId(6).WithDriverId(6).WithPosition(6).WithPoints(8).Build(),   // Position 6
                new ResultBuilder().WithId(7).WithDriverId(7).WithPosition(7).WithPoints(6).Build(),   // Position 7
                new ResultBuilder().WithId(8).WithDriverId(8).WithPosition(8).WithPoints(4).Build(),   // Position 8
                new ResultBuilder().WithId(9).WithDriverId(9).WithPosition(9).WithPoints(2).Build(),   // Position 9
                new ResultBuilder().WithId(10).WithDriverId(10).WithPosition(10).WithPoints(1).Build() // Position 10
            };
        }
    }
}