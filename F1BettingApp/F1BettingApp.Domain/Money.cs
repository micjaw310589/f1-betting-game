using System;

namespace F1BettingApp.Domain.ValueObjects
{
    public readonly struct Money : IEquatable<Money>
    {
        private readonly decimal _amount;

        // Private constructor to enforce creation through factory methods or within the struct itself
        private Money(decimal amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Money amount cannot be negative.");
            }
            _amount = amount;
        }

        public decimal Amount => _amount;

        public static Money FromDecimal(decimal amount)
        {
            return new Money(amount);
        }

        public static Money Zero()
        {
            return new Money(0);
        }

        public Money Add(Money other)
        {
            return new Money(_amount + other.Amount);
        }

        public Money Subtract(Money other)
        {
            if (other.Amount > _amount)
            {
                throw new InvalidOperationException("Cannot subtract larger amount from current balance.");
            }
            return new Money(_amount - other.Amount);
        }

        public Money Multiply(decimal multiplier)
        {
            if (multiplier < 0)
            {
                throw new ArgumentException("Multiplier must be non-negative.");
            }
            return new Money(_amount * multiplier);
        }

        public bool Equals(Money other)
        {
            return _amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_amount);
        }

        public static bool operator ==(Money left, Money right) => left.Equals(right);
        public static bool operator !=(Money left, Money right) => !left.Equals(right);
    }
}