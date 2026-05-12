using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Tests.Builders;
using Xunit;

namespace F1BettingApp.Tests
{
    /// <summary>
    /// Tests for the test data builders
    /// </summary>
    public class BuilderTests
    {
        [Fact]
        public void UserBuilder_ShouldCreateValidUser()
        {
            // Act
            var user = new UserBuilder()
                .WithId(1)
                .WithUsername("testuser")
                .WithEmail("test@example.com")
                .WithPoints(1000)
                .Build();

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user.Id);
            Assert.Equal("testuser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal(1000, user.Points);
            Assert.True(user.IsActive);
            Assert.False(user.IsAdmin);
        }

        [Fact]
        public void UserBuilder_ShouldCreateAdminUser()
        {
            // Act
            var user = new UserBuilder()
                .AsAdmin()
                .Build();

            // Assert
            Assert.True(user.IsAdmin);
        }

        [Fact]
        public void UserBuilder_ShouldCreateInactiveUser()
        {
            // Act
            var user = new UserBuilder()
                .AsInactive()
                .Build();

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void UserBuilder_ShouldCreateUserList()
        {
            // Act
            var users = new UserBuilder()
                .BuildList(3);

            // Assert
            Assert.NotNull(users);
            Assert.Equal(3, users.Count);
            Assert.Equal(1, users[0].Id);
            Assert.Equal(2, users[1].Id);
            Assert.Equal(3, users[2].Id);
        }

        [Fact]
        public void BetBuilder_ShouldCreateValidBet()
        {
            // Act
            var bet = new BetBuilder()
                .WithId(1)
                .WithUserId(1)
                .WithRaceId(1)
                .WithDriverId(1)
                .WithAmount(100)
                .WithOdds(2.5m)
                .Build();

            // Assert
            Assert.NotNull(bet);
            Assert.Equal(1, bet.Id);
            Assert.Equal(1, bet.UserId);
            Assert.Equal(1, bet.RaceId);
            Assert.Equal(1, bet.DriverId);
            Assert.Equal(100, bet.Amount);
            Assert.Equal(2.5m, bet.Odds);
            Assert.Equal(250, bet.PotentialWinnings);
            Assert.Equal(BetStatus.Pending, bet.Status);
        }

        [Fact]
        public void BetBuilder_ShouldCreateWonBet()
        {
            // Act
            var bet = new BetBuilder()
                .AsWon()
                .Build();

            // Assert
            Assert.Equal(BetStatus.Won, bet.Status);
        }

        [Fact]
        public void BetBuilder_ShouldCreateLostBet()
        {
            // Act
            var bet = new BetBuilder()
                .AsLost()
                .Build();

            // Assert
            Assert.Equal(BetStatus.Lost, bet.Status);
        }

        [Fact]
        public void BetBuilder_ShouldCreateCanceledBet()
        {
            // Act
            var bet = new BetBuilder()
                .AsCanceled()
                .Build();

            // Assert
            Assert.Equal(BetStatus.Canceled, bet.Status);
        }

        [Fact]
        public void BetBuilder_ShouldRecalculatePotentialWinnings()
        {
            // Act
            var bet = new BetBuilder()
                .WithAmount(200)
                .WithOdds(3.0m)
                .Build();

            // Assert
            Assert.Equal(600, bet.PotentialWinnings); // 200 * 3.0
        }

        [Fact]
        public void BetBuilder_ShouldCreateBetList()
        {
            // Act
            var bets = new BetBuilder()
                .BuildList(2);

            // Assert
            Assert.NotNull(bets);
            Assert.Equal(2, bets.Count);
            Assert.Equal(1, bets[0].Id);
            Assert.Equal(2, bets[1].Id);
        }

        [Fact]
        public void RaceBuilder_ShouldCreateValidRace()
        {
            // Act
            var race = new RaceBuilder()
                .WithId(1)
                .WithName("Test Grand Prix")
                .WithCircuit("Test Circuit")
                .WithCountry("Test Country")
                .WithSeason(2023)
                .Build();

            // Assert
            Assert.NotNull(race);
            Assert.Equal(1, race.Id);
            Assert.Equal("Test Grand Prix", race.Name);
            Assert.Equal("Test Circuit", race.Circuit);
            Assert.Equal("Test Country", race.Country);
            Assert.Equal(2023, race.Season);
            Assert.Equal(RaceStatus.Scheduled, race.Status);
        }

        [Fact]
        public void RaceBuilder_ShouldCreateFinishedRace()
        {
            // Act
            var race = new RaceBuilder()
                .AsFinished()
                .Build();

            // Assert
            Assert.Equal(RaceStatus.Finished, race.Status);
        }

        [Fact]
        public void RaceBuilder_ShouldCreateUpcomingRace()
        {
            // Act
            var race = new RaceBuilder()
                .BuildUpcomingRace();

            // Assert
            Assert.True(race.Date > DateTime.UtcNow);
            Assert.Equal(RaceStatus.Scheduled, race.Status);
        }

        [Fact]
        public void RaceBuilder_ShouldCreateFinishedRaceShortcut()
        {
            // Act
            var race = new RaceBuilder()
                .BuildFinishedRace();

            // Assert
            Assert.True(race.Date < DateTime.UtcNow);
            Assert.Equal(RaceStatus.Finished, race.Status);
        }

        [Fact]
        public void RaceBuilder_ShouldCreateRaceList()
        {
            // Act
            var races = new RaceBuilder()
                .BuildList(2);

            // Assert
            Assert.NotNull(races);
            Assert.Equal(2, races.Count);
            Assert.Equal(1, races[0].Id);
            Assert.Equal(2, races[1].Id);
        }

        [Fact]
        public void ResultBuilder_ShouldCreateValidResult()
        {
            // Act
            var result = new ResultBuilder()
                .WithId(1)
                .WithRaceId(1)
                .WithDriverId(1)
                .WithPosition(1)
                .WithPoints(25)
                .Build();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.RaceId);
            Assert.Equal(1, result.DriverId);
            Assert.Equal(1, result.Position);
            Assert.Equal(25, result.Points);
            Assert.NotNull(result.FastestLap);
            Assert.NotNull(result.PitStopTime);
        }

        [Fact]
        public void ResultBuilder_ShouldCreatePodiumFinish()
        {
            // Act
            var result = new ResultBuilder()
                .AsPodiumFinish(2) // Position 2
                .Build();

            // Assert
            Assert.Equal(2, result.Position);
            Assert.Equal(18, result.Points); // Position 2 gets 18 points
        }

        [Fact]
        public void ResultBuilder_ShouldCreateDNF()
        {
            // Act
            var result = new ResultBuilder()
                .AsDNF()
                .Build();

            // Assert
            Assert.Equal(0, result.Position);
            Assert.Equal(0, result.Points);
            Assert.Null(result.FastestLap);
            Assert.Null(result.PitStopTime);
        }

        [Fact]
        public void ResultBuilder_ShouldCreateResultWithoutOptionalTimes()
        {
            // Act
            var result = new ResultBuilder()
                .WithoutFastestLap()
                .WithoutPitStopTime()
                .Build();

            // Assert
            Assert.Null(result.FastestLap);
            Assert.Null(result.PitStopTime);
        }

        [Fact]
        public void ResultBuilder_ShouldCreateRaceResults()
        {
            // Act
            var results = new ResultBuilder()
                .BuildRaceResults();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(10, results.Count);
            Assert.Equal(1, results[0].Position);
            Assert.Equal(25, results[0].Points);
            Assert.Equal(10, results[9].Position);
            Assert.Equal(1, results[9].Points);
        }

        [Fact]
        public void ResultBuilder_ShouldCreateResultList()
        {
            // Act
            var results = new ResultBuilder()
                .BuildList(3);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
            Assert.Equal(1, results[0].Id);
            Assert.Equal(2, results[1].Id);
            Assert.Equal(3, results[2].Id);
        }

        [Fact]
        public void ResultBuilder_PodiumFinishShouldValidatePosition()
        {
            // Assert
            Assert.Throws<ArgumentException>(() =>
                new ResultBuilder().AsPodiumFinish(4));
        }
    }
}