using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Domain.Specifications
{
    /// <summary>
    /// Specification for finding pending bets for a specific user
    /// </summary>
    public class UserPendingBetsSpecification : BaseSpecification<Bet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPendingBetsSpecification"/> class
        /// </summary>
        /// <param name="userId">The user ID</param>
        public UserPendingBetsSpecification(int userId)
            : base(bet => bet.UserId == userId && bet.Status == BetStatus.Pending)
        {
        }
    }

    /// <summary>
    /// Specification for finding bets on a specific race
    /// </summary>
    public class RaceBetsSpecification : BaseSpecification<Bet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RaceBetsSpecification"/> class
        /// </summary>
        /// <param name="raceId">The race ID</param>
        public RaceBetsSpecification(int raceId)
            : base(bet => bet.RaceId == raceId)
        {
            // Note: Bet entity doesn't have navigation properties, so no includes needed
        }
    }

    /// <summary>
    /// Specification for finding winning bets for a user
    /// </summary>
    public class UserWinningBetsSpecification : BaseSpecification<Bet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserWinningBetsSpecification"/> class
        /// </summary>
        /// <param name="userId">The user ID</param>
        public UserWinningBetsSpecification(int userId)
            : base(bet => bet.UserId == userId && bet.Status == BetStatus.Won)
        {
        }
    }

    /// <summary>
    /// Specification for finding high-value bets (over a certain amount)
    /// </summary>
    public class HighValueBetsSpecification : BaseSpecification<Bet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighValueBetsSpecification"/> class
        /// </summary>
        /// <param name="minimumAmount">The minimum bet amount</param>
        public HighValueBetsSpecification(decimal minimumAmount)
            : base(bet => bet.Amount >= minimumAmount)
        {
        }
    }

    /// <summary>
    /// Specification for finding bets with specific status
    /// </summary>
    public class BetStatusSpecification : BaseSpecification<Bet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BetStatusSpecification"/> class
        /// </summary>
        /// <param name="status">The bet status to filter by</param>
        public BetStatusSpecification(BetStatus status)
            : base(bet => bet.Status == status)
        {
        }
    }
}