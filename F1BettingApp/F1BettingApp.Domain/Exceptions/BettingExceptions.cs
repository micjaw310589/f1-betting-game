using System;

namespace F1BettingApp.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when user has insufficient funds for a bet.
    /// </summary>
    public class InsufficientFundsException : ApplicationException
    {
        public InsufficientFundsException(string message) 
            : base(message) { }
    }

    /// <summary>
    /// Exception thrown when the race is already completed or in progress.
    /// </summary>
    public class RaceCompletedException : ApplicationException
    {
        public RaceCompletedException(string? message = null) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a bet is not found for the given ID and user.
    /// </summary>
    public class BetNotFoundException : ApplicationException
    {
        public int BetId { get; }

        public BetNotFoundException(int betId, string? message = null) 
            : base(message) => BetId = betId;
    }

    /// <summary>
    /// Exception thrown when a user tries to cancel a bet that doesn't belong to them.
    /// </summary>
    public class UnauthorizedAccessException : ApplicationException
    {
        public UnauthorizedAccessException(string message) 
            : base(message) { }
    }

    /// <summary>
    /// Exception thrown when race is already in progress and can't be modified.
    /// </summary>
    public class RaceAlreadyStartedException : ApplicationException
    {
        public RaceAlreadyStartedException(string? message = null) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when invalid bet type is specified for position-based bets.
    /// </summary>
    public class InvalidBetTypeException : ApplicationException
    {
        public InvalidBetTypeException(string? message = null) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when position value is out of valid range.
    /// </summary>
    public class InvalidPositionValueException : ApplicationException
    {
        public int Position { get; }

        public InvalidPositionValueException(int position, string? message = null) 
            : base(message) => Position = position;
    }

    /// <summary>
    /// Exception thrown when nationality code is invalid or empty.
    /// </summary>
    public class InvalidNationalityException : ApplicationException
    {
        public InvalidNationalityException(string? message = null) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when constructor position is out of valid range.
    /// </summary>
    public class InvalidConstructorPositionException : ApplicationException
    {
        public int Position { get; }

        public InvalidConstructorPositionException(int position, string? message = null) 
            : base(message) => Position = position;
    }

    /// <summary>
    /// Exception thrown when odds position is out of valid range.
    /// </summary>
    public class InvalidOddsPositionException : ApplicationException
    {
        public int Position { get; }

        public InvalidOddsPositionException(int position, string? message = null) 
            : base(message) => Position = position;
    }

    /// <summary>
    /// Exception thrown when qualifying position is out of valid range.
    /// </summary>
    public class InvalidQualifyingPositionException : ApplicationException
    {
        public int Position { get; }

        public InvalidQualifyingPositionException(int position, string? message = null) 
            : base(message) => Position = position;
    }

    /// <summary>
    /// Exception thrown when statistics position is out of valid range.
    /// </summary>
    public class InvalidStatisticsPositionException : ApplicationException
    {
        public int Position { get; }

        public InvalidStatisticsPositionException(int position, string? message = null) 
            : base(message) => Position = position;
    }

    /// <summary>
    /// Exception thrown when bet validation fails.
    /// </summary>
    public class BetValidationException : ApplicationException
    {
        public List<string> Errors { get; }

        public BetValidationException(List<string> errors, string? message = null) 
            : base(message) => Errors = errors ?? new List<string>();
    }
}