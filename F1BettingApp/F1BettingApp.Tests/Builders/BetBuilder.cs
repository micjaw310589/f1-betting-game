using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Tests.Builders
{
    /// <summary>
    /// Builder for creating test Bet entities
    /// </summary>
    public class BetBuilder
    {
        private int _id = 1;
        private int _userId = 1;
        private int _raceId = 1;
        private int _driverId = 1;
        private BetType _betType = BetType.RaceWinner;
        private decimal _amount = 100.00m;
        private decimal _odds = 2.5m;
        private decimal _potentialWinnings = 250.00m;
        private BetStatus _status = BetStatus.Pending;
        private DateTime _createdAt = DateTime.UtcNow;

        /// <summary>
        /// Sets the bet ID
        /// </summary>
        /// <param name="id">The bet ID</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithUserId(int userId)
        {
            _userId = userId;
            return this;
        }

        /// <summary>
        /// Sets the race ID
        /// </summary>
        /// <param name="raceId">The race ID</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithRaceId(int raceId)
        {
            _raceId = raceId;
            return this;
        }

        /// <summary>
        /// Sets the driver ID
        /// </summary>
        /// <param name="driverId">The driver ID</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithDriverId(int driverId)
        {
            _driverId = driverId;
            return this;
        }

        /// <summary>
        /// Sets the bet type
        /// </summary>
        /// <param name="betType">The bet type</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithBetType(BetType betType)
        {
            _betType = betType;
            return this;
        }

        /// <summary>
        /// Sets the bet amount
        /// </summary>
        /// <param name="amount">The bet amount</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            _potentialWinnings = amount * _odds; // Recalculate potential winnings
            return this;
        }

        /// <summary>
        /// Sets the odds
        /// </summary>
        /// <param name="odds">The odds</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithOdds(decimal odds)
        {
            _odds = odds;
            _potentialWinnings = _amount * odds; // Recalculate potential winnings
            return this;
        }

        /// <summary>
        /// Sets the bet status
        /// </summary>
        /// <param name="status">The bet status</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithStatus(BetStatus status)
        {
            _status = status;
            return this;
        }

        /// <summary>
        /// Sets the bet as won
        /// </summary>
        /// <returns>The builder instance</returns>
        public BetBuilder AsWon()
        {
            _status = BetStatus.Won;
            return this;
        }

        /// <summary>
        /// Sets the bet as lost
        /// </summary>
        /// <returns>The builder instance</returns>
        public BetBuilder AsLost()
        {
            _status = BetStatus.Lost;
            return this;
        }

        /// <summary>
        /// Sets the bet as canceled
        /// </summary>
        /// <returns>The builder instance</returns>
        public BetBuilder AsCanceled()
        {
            _status = BetStatus.Canceled;
            return this;
        }

        /// <summary>
        /// Sets the creation date
        /// </summary>
        /// <param name="createdAt">The creation date</param>
        /// <returns>The builder instance</returns>
        public BetBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        /// <summary>
        /// Builds the Bet entity
        /// </summary>
        /// <returns>The constructed Bet entity</returns>
        public Bet Build()
        {
            // Use the Bet constructor for validation
            var bet = new Bet(_userId, _raceId, _driverId, _amount, _betType, _odds)
            {
                Id = _id,
                Status = _status,
                CreatedAt = _createdAt,
                PotentialWinnings = _potentialWinnings
            };

            return bet;
        }

        /// <summary>
        /// Builds a list of bets with sequential IDs
        /// </summary>
        /// <param name="count">The number of bets to create</param>
        /// <returns>List of Bet entities</returns>
        public List<Bet> BuildList(int count)
        {
            var bets = new List<Bet>();
            for (int i = 0; i < count; i++)
            {
                bets.Add(WithId(_id + i).Build());
            }
            return bets;
        }
    }
}