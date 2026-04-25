using System;

namespace F1BettingApp.Domain.ValueObjects
{
    public readonly struct Odds : IEquatable<Odds>
    {
        private readonly decimal _value;

        private Odds(decimal value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Odds value must be greater than zero.");
            }
            _value = value;
        }

        public decimal Value => _value;

        public static Odds FromDecimal(decimal value)
        {
            return new Odds(value);
        }

        // Common operation: Calculate potential payout
        public Money CalculatePayout(Money betAmount)
        {
            return betAmount.Multiply(this.Value);
        }

        public bool Equals(Odds other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is Odds other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value);
        }

        public static bool operator ==(Odds left, Odds right) => left.Equals(right);
        public static bool operator !=(Odds left, Odds right) => !left.Equals(right);
    }
}