using Xunit;
using System;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Tests
{
    public class DomainEntityTests
    {
        // --- Result Entity Tests ---
        [Fact]
        public void Result_Constructor_ValidData_ShouldCreateInstance()
        {
            // Arrange
            int raceId = 1;
            int driverId = 1;
            int position = 1;
            int points = 25;
            TimeSpan fastestLap = TimeSpan.FromSeconds(100);
            TimeSpan? pitStopTime = TimeSpan.FromSeconds(60);

            // Act
            var result = new Result(raceId, driverId, position, points, fastestLap, pitStopTime);

            // Assert
            Assert.Equal(raceId, result.RaceId);
            Assert.Equal(driverId, result.DriverId);
            Assert.Equal(position, result.Position);
            Assert.Equal(points, result.Points);
            Assert.Equal(fastestLap, result.FastestLap);
            Assert.Equal(pitStopTime, result.PitStopTime);
        }

        [Fact]
        public void Result_Constructor_InvalidData_ShouldThrowArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new Result(0, 1, 1, 1, TimeSpan.Zero, null));
            Assert.Throws<ArgumentException>(() => new Result(1, 0, 1, 1, TimeSpan.Zero, null));
            Assert.Throws<ArgumentException>(() => new Result(1, 1, 0, 1, TimeSpan.Zero, null));
        }

        [Fact]
        public void Result_IsPodiumFinish_ShouldReturnTrueForPositions1To3()
        {
            // Arrange
            var result1 = new Result(1, 1, 1, 25, TimeSpan.Zero, null);
            var result2 = new Result(1, 1, 2, 18, TimeSpan.Zero, null);
            var result3 = new Result(1, 1, 3, 15, TimeSpan.Zero, null);
            var result4 = new Result(1, 1, 4, 0, TimeSpan.Zero, null);

            // Act & Assert
            Assert.True(result1.IsPodiumFinish());
            Assert.True(result2.IsPodiumFinish());
            Assert.True(result3.IsPodiumFinish());
            Assert.False(result4.IsPodiumFinish());
        }

        [Fact]
        public void Result_IsPointsFinish_ShouldReturnTrueForPositivePoints()
        {
            // Arrange
            var result1 = new Result(1, 1, 1, 25, TimeSpan.Zero, null);
            var result2 = new Result(1, 1, 4, 1, TimeSpan.Zero, null);
            var result3 = new Result(1, 1, 4, 0, TimeSpan.Zero, null);

            // Act & Assert
            Assert.True(result1.IsPointsFinish());
            Assert.True(result2.IsPointsFinish());
            Assert.False(result3.IsPointsFinish());
        }

        // --- Notification Entity Tests ---
        [Fact]
        public void Notification_Constructor_ValidData_ShouldCreateInstance()
        {
            // Arrange
            int userId = 1;
            string title = "Race Finished";
            string message = "You scored 5 points!";

            // Act
            var notification = new Notification(userId, title, message);

            // Assert
            Assert.Equal(userId, notification.UserId);
            Assert.Equal(title, notification.Title);
            Assert.Equal(message, notification.Message);
            Assert.False(notification.IsRead);
            Assert.NotNull(notification.CreatedAt);
        }

        [Fact]
        public void Notification_Constructor_InvalidData_ShouldThrowArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new Notification(0, "Title", "Message"));
            Assert.Throws<ArgumentException>(() => new Notification(1, null, "Message"));
            Assert.Throws<ArgumentException>(() => new Notification(1, "Title", ""));
        }

        [Fact]
        public void Notification_MarkAsRead_ShouldUpdateStatus()
        {
            // Arrange
            var notification = new Notification(1, "Test", "Test");

            // Act
            notification.MarkAsRead();

            // Assert
            Assert.True(notification.IsRead);
        }
        
        // --- LeaderboardHistory Entity Tests ---
        [Fact]
        public void LeaderboardHistory_Constructor_ValidData_ShouldCreateInstance()
        {
            // Arrange
            int userId = 1;
            int raceId = 1;
            string season = "2026";
            int totalPoints = 500;
            int rank = 5;

            // Act
            var history = new LeaderboardHistory(userId, raceId, season, totalPoints, rank);

            // Assert
            Assert.Equal(userId, history.UserId);
            Assert.Equal(raceId, history.RaceId);
            Assert.Equal(season, history.Season);
            Assert.Equal(totalPoints, history.TotalPoints);
            Assert.Equal(rank, history.Rank);
            Assert.NotNull(history.CreatedAt);
        }

        [Fact]
        public void LeaderboardHistory_Constructor_InvalidData_ShouldThrowArgumentException()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new LeaderboardHistory(0, 1, "2026", 100, 1));
            Assert.Throws<ArgumentException>(() => new LeaderboardHistory(1, 0, "2026", 100, 1));
            Assert.Throws<ArgumentException>(() => new LeaderboardHistory(1, 1, null, 100, 1));
            Assert.Throws<ArgumentException>(() => new LeaderboardHistory(1, 1, "2026", -10, 1));
        }
    }
}