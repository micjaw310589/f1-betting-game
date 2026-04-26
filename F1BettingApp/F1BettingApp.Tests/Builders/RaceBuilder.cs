using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Tests.Builders
{
    /// <summary>
    /// Builder for creating test Race entities
    /// </summary>
    public class RaceBuilder
    {
        private int _id = 1;
        private string _name = "Test Grand Prix";
        private DateTime _date = DateTime.UtcNow.AddDays(7);
        private RaceStatus _status = RaceStatus.Scheduled;
        private string _circuit = "Test Circuit";
        private string _country = "Test Country";
        private string _openF1RaceId = "race123";
        private int _season = 2023;
        private List<Bet> _bets = new List<Bet>();

        /// <summary>
        /// Sets the race ID
        /// </summary>
        /// <param name="id">The race ID</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the race name
        /// </summary>
        /// <param name="name">The race name</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the race date
        /// </summary>
        /// <param name="date">The race date</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithDate(DateTime date)
        {
            _date = date;
            return this;
        }

        /// <summary>
        /// Sets the race status
        /// </summary>
        /// <param name="status">The race status</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithStatus(RaceStatus status)
        {
            _status = status;
            return this;
        }

        /// <summary>
        /// Sets the race as finished
        /// </summary>
        /// <returns>The builder instance</returns>
        public RaceBuilder AsFinished()
        {
            _status = RaceStatus.Finished;
            return this;
        }

        /// <summary>
        /// Sets the race as in progress
        /// </summary>
        /// <returns>The builder instance</returns>
        public RaceBuilder AsInProgress()
        {
            _status = RaceStatus.InProgress;
            return this;
        }

        /// <summary>
        /// Sets the circuit name
        /// </summary>
        /// <param name="circuit">The circuit name</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithCircuit(string circuit)
        {
            _circuit = circuit;
            return this;
        }

        /// <summary>
        /// Sets the country
        /// </summary>
        /// <param name="country">The country</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithCountry(string country)
        {
            _country = country;
            return this;
        }

        /// <summary>
        /// Sets the OpenF1 race ID
        /// </summary>
        /// <param name="openF1RaceId">The OpenF1 race ID</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithOpenF1RaceId(string openF1RaceId)
        {
            _openF1RaceId = openF1RaceId;
            return this;
        }

        /// <summary>
        /// Sets the season
        /// </summary>
        /// <param name="season">The season year</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithSeason(int season)
        {
            _season = season;
            return this;
        }

        /// <summary>
        /// Adds bets to the race
        /// </summary>
        /// <param name="bets">The bets to add</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder WithBets(List<Bet> bets)
        {
            _bets = bets ?? new List<Bet>();
            return this;
        }

        /// <summary>
        /// Adds a single bet to the race
        /// </summary>
        /// <param name="bet">The bet to add</param>
        /// <returns>The builder instance</returns>
        public RaceBuilder AddBet(Bet bet)
        {
            _bets.Add(bet);
            return this;
        }

        /// <summary>
        /// Builds the Race entity
        /// </summary>
        /// <returns>The constructed Race entity</returns>
        public Race Build()
        {
            // Use the Race constructor for validation
            var race = new Race(_name, _date, _circuit, _country, _openF1RaceId, _season)
            {
                Id = _id,
                Status = _status,
                Bets = _bets
            };

            return race;
        }

        /// <summary>
        /// Builds a list of races with sequential IDs
        /// </summary>
        /// <param name="count">The number of races to create</param>
        /// <returns>List of Race entities</returns>
        public List<Race> BuildList(int count)
        {
            var races = new List<Race>();
            for (int i = 0; i < count; i++)
            {
                races.Add(WithId(_id + i).Build());
            }
            return races;
        }

        /// <summary>
        /// Builds an upcoming race (scheduled in the future)
        /// </summary>
        /// <returns>The constructed Race entity</returns>
        public Race BuildUpcomingRace()
        {
            return WithDate(DateTime.UtcNow.AddDays(7))
                .WithStatus(RaceStatus.Scheduled)
                .Build();
        }

        /// <summary>
        /// Builds a finished race (in the past)
        /// </summary>
        /// <returns>The constructed Race entity</returns>
        public Race BuildFinishedRace()
        {
            return WithDate(DateTime.UtcNow.AddDays(-1))
                .WithStatus(RaceStatus.Finished)
                .Build();
        }
    }
}