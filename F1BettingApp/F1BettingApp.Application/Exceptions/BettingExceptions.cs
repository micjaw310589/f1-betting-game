namespace F1BettingApp.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not found
    /// </summary>
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when user has insufficient funds
    /// </summary>
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(decimal requiredAmount, decimal availableAmount)
            : base($"Insufficient funds. Required: {requiredAmount}, Available: {availableAmount}") { }
    }

    /// <summary>
    /// Exception thrown when a race is not found
    /// </summary>
    public class RaceNotFoundException : Exception
    {
        public RaceNotFoundException(int raceId) : base($"Race with ID {raceId} not found") { }
    }

    /// <summary>
    /// Exception thrown when a race is not scheduled/upcoming
    /// </summary>
    public class RaceNotUpcomingException : Exception
    {
        public RaceNotUpcomingException() : base("Race is not scheduled") { }
    }

    /// <summary>
    /// Exception thrown when a driver is not found
    /// </summary>
    public class DriverNotFoundException : Exception
    {
        public DriverNotFoundException(int driverId) : base($"Driver with ID {driverId} not found") { }
    }

    /// <summary>
    /// Exception thrown when a bet is not found
    /// </summary>
    public class BetNotFoundException : Exception
    {
        public BetNotFoundException(int betId) : base($"Bet with ID {betId} not found") { }
    }

    /// <summary>
    /// Exception thrown when attempting to cancel a bet after race has started
    /// </summary>
    public class RaceAlreadyStartedException : Exception
    {
        public RaceAlreadyStartedException() : base("Cannot cancel bet after race has started") { }
    }

    /// <summary>
    /// Exception thrown when attempting to place a bet on a completed race
    /// </summary>
    public class RaceCompletedException : Exception
    {
        public RaceCompletedException() : base("Cannot place bet on completed race") { }
    }
}