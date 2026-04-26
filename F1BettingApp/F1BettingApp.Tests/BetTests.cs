using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Xunit;
using System;

namespace F1BettingApp.Tests
{
    public class BetTests
    {
        [Fact]
        public void Constructor_ValidInputs_CreatesBetObject()
        {
            // Arrange
            var bet = new Bet(1, 1, 1, 100.00m, BetType.RaceWinner, 2.5m);

            // Act
            // Assert
            Assert.Equal(100.00m * 2.5m, bet.PotentialWinnings);
            Assert.Equal(BetType.RaceWinner, bet.BetType);
        }

        [Fact]
        public void Constructor_InvalidAmount_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Bet(1, 1, 1, 0.00m, BetType.RaceWinner, 2.5m));
            Assert.Throws<ArgumentException>(() => new Bet(1, 1, 1, -1.00m, BetType.RaceWinner, 2.5m));
        }

        [Fact]
        public void Constructor_InvalidOdds_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Bet(1, 1, 1, 10.00m, BetType.RaceWinner, 0.00m));
            Assert.Throws<ArgumentException>(() => new Bet(1, 1, 1, 10.00m, BetType.RaceWinner, -1.00m));
        }

        [Fact]
        public void ValidateBet_InvalidBetType_ThrowsInvalidOperationException()
        {
            // Arrange
            // Using Unknown (0) which represents an invalid type
            var bet = new Bet(1, 1, 1, 10.00m, BetType.Unknown, 2.5m);

            // Act & Assert
            var ex = Record.Exception(() => bet.ValidateBet());
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void ValidateBet_ZeroOdds_ThrowsInvalidOperationException()
        {
            // Arrange
            var bet = new Bet(1, 1, 1, 10.00m, BetType.RaceWinner, 0.0m);

            // Act & Assert
            var ex = Record.Exception(() => bet.ValidateBet());
            Assert.IsType<InvalidOperationException>(ex);
        }
    }
}