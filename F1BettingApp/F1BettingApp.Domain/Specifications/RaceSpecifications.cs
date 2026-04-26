using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Domain.Specifications
{
    /// <summary>
    /// Specification for finding upcoming races
    /// </summary>
    public class UpcomingRacesSpecification : BaseSpecification<Race>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpcomingRacesSpecification"/> class
        /// </summary>
        public UpcomingRacesSpecification()
            : base(race => race.Status == RaceStatus.Scheduled && race.Date > DateTime.UtcNow)
        {
        }
    }

    /// <summary>
    /// Specification for finding races by season
    /// </summary>
    public class RacesBySeasonSpecification : BaseSpecification<Race>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RacesBySeasonSpecification"/> class
        /// </summary>
        /// <param name="season">The season year</param>
        public RacesBySeasonSpecification(int season)
            : base(race => race.Season == season)
        {
        }
    }

    /// <summary>
    /// Specification for finding finished races
    /// </summary>
    public class FinishedRacesSpecification : BaseSpecification<Race>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinishedRacesSpecification"/> class
        /// </summary>
        public FinishedRacesSpecification()
            : base(race => race.Status == RaceStatus.Finished)
        {
        }
    }

    /// <summary>
    /// Specification for finding races that allow betting
    /// </summary>
    public class BettingAllowedRacesSpecification : BaseSpecification<Race>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BettingAllowedRacesSpecification"/> class
        /// </summary>
        public BettingAllowedRacesSpecification()
            : base(race => race.CanPlaceBets())
        {
        }
    }
}