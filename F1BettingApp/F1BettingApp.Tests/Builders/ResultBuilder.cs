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
        private TimeSpan _fastestLap = TimeSpan.FromMinutes(1);
        private TimeSpan? _pitStopTime = TimeSpan.FromSeconds(20);
        private bool _hasFastestLap = true;

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
        /// Removes the fastest lap time
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithoutFastestLap()
        {
            _hasFastestLap = false;
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
        /// Removes the pit stop time
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder WithoutPitStopTime()
        {
            _pitStopTime = null;
            return this;
        }

        /// <summary>
        /// Sets the result as a podium finish with the specified position
        /// </summary>
        /// <param name="position">The position (1, 2, or 3)</param>
        /// <returns>The builder instance</returns>
        public ResultBuilder AsPodiumFinish(int position)
        {
            if (position < 1 || position > 3)
            {
                throw new ArgumentException("Podium finish must be position 1, 2, or 3");
            }

            _position = position;
            _points = position == 1 ? 25 : (position == 2 ? 18 : 15);
            return this;
        }

        /// <summary>
        /// Sets the result as DNF (Did Not Finish)
        /// </summary>
        /// <returns>The builder instance</returns>
        public ResultBuilder AsDNF()
        {
            _position = 0;
            _points = 0;
            _hasFastestLap = false;
            _pitStopTime = null;
            return this;
        }

        /// <summary>
        /// Builds the Result entity
        /// </summary>
        /// <returns>The constructed Result entity</returns>
        public Result Build()
        {
            TimeSpan? fastestLapValue = _hasFastestLap && _position != 0 ? _fastestLap : null;
            var result = new Result(_raceId, _driverId, _position, _points, fastestLapValue ?? TimeSpan.FromMinutes(1), _pitStopTime);
            result.FastestLap = fastestLapValue;
            result.Id = _id;
            return result;
        }

        /// <summary>
        /// Builds a list of race results
        /// </summary>
        /// <param name="count">The number of results to create</param>
        /// <returns>List of Result entities</returns>
        public List<Result> BuildRaceResults(int count = 10)
        {
            var results = new List<Result>();
            // F1 Points system: 1st=25, 2nd=18, 3rd=15, 4th=12, 5th=10, 6th=8, 7th=6, 8th=4, 9th=2, 10th=1
            int[] f1Points = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };
            for (int i = 1; i <= count; i++)
            {
                int points = i <= 10 ? f1Points[i - 1] : 0;
                TimeSpan? fastestLap = i == 1 ? TimeSpan.FromMinutes(1) : null;
                TimeSpan? pitStopTime = i == 1 ? TimeSpan.FromSeconds(20) : null;
                TimeSpan? fastestLapValue = (i == 1 && _points > 0) ? TimeSpan.FromMinutes(1) : null;
                var result = new Result(_raceId, i, i, points, fastestLapValue ?? TimeSpan.FromMinutes(1), pitStopTime);
                result.FastestLap = fastestLapValue;
                result.Id = i;
                results.Add(result);
            }
            return results;
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
                var result = new Result(i + 1, i + 1, i + 1, i + 1, default, default);
                result.Id = i + 1;
                result.Position = i + 1;
                result.Points = i + 1;
                results.Add(result);
            }
            return results;
        }
    }
}