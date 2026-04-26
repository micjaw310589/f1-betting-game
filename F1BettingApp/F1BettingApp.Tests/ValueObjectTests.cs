using Xunit;
using System;
using F1BettingApp.Domain.ValueObjects;

namespace F1BettingApp.Tests
{
    public class ValueObjectTests
    {
        [Fact]
        public void Money_Constructor_ThrowsOnNegativeAmount()
        {
            Assert.Throws<ArgumentException>(() => Money.FromDecimal(-1m));
        }

        [Fact]
        public void Money_Operations_WorkCorrectly()
        {
            Money startMoney = Money.FromDecimal(100m);
            Money addedMoney = startMoney.Add(Money.FromDecimal(50m));
            Assert.Equal(150m, addedMoney.Amount);

            Money remainingMoney = startMoney.Subtract(Money.FromDecimal(30m));
            Assert.Equal(70m, remainingMoney.Amount);

            Money multipliedMoney = startMoney.Multiply(1.5m);
            Assert.Equal(150m, multipliedMoney.Amount);
        }

        [Fact]
        public void Money_Equality_Comparison()
        {
            var m1 = Money.FromDecimal(100m);
            var m2 = Money.FromDecimal(100m);
            var m3 = Money.FromDecimal(200m);

            Assert.True(m1.Equals(m2));
            Assert.Equal(m1.GetHashCode(), m2.GetHashCode());
            Assert.False(m1.Equals(m3));
        }

        [Fact]
        public void RaceDate_Constructor_ThrowsOnDefaultDate()
        {
            Assert.Throws<ArgumentException>(() => RaceDate.FromDateTime(default(DateTime)));
        }

        [Fact]
        public void RaceDate_Equality_Comparison()
        {
            var date1 = RaceDate.FromDateTime(new DateTime(2024, 10, 20, 10, 0, 0));
            var date2 = RaceDate.FromDateTime(new DateTime(2024, 10, 20, 23, 59, 59));
            var date3 = RaceDate.FromDateTime(new DateTime(2024, 10, 21, 0, 0, 0));

            Assert.True(date1.Equals(date2));
            Assert.Equal(date1.GetHashCode(), date2.GetHashCode());
            Assert.False(date1.Equals(date3));
        }

        [Fact]
        public void Odds_Constructor_ThrowsOnZeroOrNegativeValue()
        {
            Assert.Throws<ArgumentException>(() => Odds.FromDecimal(0m));
            Assert.Throws<ArgumentException>(() => Odds.FromDecimal(-1m));
        }

        [Fact]
        public void Odds_PayoutCalculation_WorksCorrectly()
        {
            Money bet = Money.FromDecimal(100m);
            var odds = Odds.FromDecimal(2.5m);
            var payout = odds.CalculatePayout(bet);

            Assert.Equal(250m, payout.Amount);
        }

        [Fact]
        public void Odds_Equality_Comparison()
        {
            var o1 = Odds.FromDecimal(3m);
            var o2 = Odds.FromDecimal(3m);
            var o3 = Odds.FromDecimal(1m);

            Assert.True(o1.Equals(o2));
            Assert.Equal(o1.GetHashCode(), o2.GetHashCode());
            Assert.False(o1.Equals(o3));
        }
    }
}