using System;

namespace F1BettingApp.Domain.ValueObjects
{
    public readonly struct RaceDate : IEquatable<RaceDate>
    {
        private readonly DateTime _date;

        private RaceDate(DateTime date)
        {
            if (date == default(DateTime))
            {
                throw new ArgumentException("Race date cannot be default.");
            }
            // Normalize date to midnight to avoid time zone/time component issues
            _date = date.Date;
        }

        public DateTime Value => _date;

        public static RaceDate FromDateTime(DateTime dateTime)
        {
            return new RaceDate(dateTime);
        }

        public static RaceDate Today()
        {
            return new RaceDate(DateTime.Today);
        }

        public bool Equals(RaceDate other)
        {
            return _date == other._date;
        }

        public override bool Equals(object obj)
        {
            return obj is RaceDate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _date.GetHashCode();
        }

        public static bool operator ==(RaceDate left, RaceDate right) => left.Equals(right);
        public static bool operator !=(RaceDate left, RaceDate right) => !left.Equals(right);
    }
}