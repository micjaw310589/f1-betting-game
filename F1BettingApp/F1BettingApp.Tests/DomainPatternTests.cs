using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Events;
using F1BettingApp.Domain.Specifications;
using F1BettingApp.Infrastructure.Events;
using System;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    public class DomainPatternTests
    {
        [Fact]
        public void BetPlacedEvent_ShouldContainCorrectData()
        {
            // Arrange
            var bet = new Bet(1, 1, 1, 100, Domain.Enums.BetType.RaceWinner, 2.5m);
            var userId = 1;
            var raceId = 1;
            var amount = 100m;

            // Act
            var betPlacedEvent = new BetPlacedEvent(bet, userId, raceId, amount);

            // Assert
            Assert.NotNull(betPlacedEvent);
            Assert.Equal(bet, betPlacedEvent.Bet);
            Assert.Equal(userId, betPlacedEvent.UserId);
            Assert.Equal(raceId, betPlacedEvent.RaceId);
            Assert.Equal(amount, betPlacedEvent.Amount);
            Assert.True(betPlacedEvent.EventTimestamp <= DateTime.UtcNow);
            Assert.True(betPlacedEvent.EventTimestamp >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void RaceFinishedEvent_ShouldContainCorrectData()
        {
            // Arrange
            var race = new Race("Test Race", DateTime.UtcNow.AddDays(-1), "Test Circuit", "Test Country", "race1", 2023);
            race.Status = Domain.Enums.RaceStatus.Finished;

            // Act
            var raceFinishedEvent = new RaceFinishedEvent(race);

            // Assert
            Assert.NotNull(raceFinishedEvent);
            Assert.Equal(race, raceFinishedEvent.Race);
            Assert.Equal(race.Id, raceFinishedEvent.RaceId);
            Assert.True(raceFinishedEvent.EventTimestamp <= DateTime.UtcNow);
            Assert.True(raceFinishedEvent.EventTimestamp >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void PointsAwardedEvent_ShouldContainCorrectData()
        {
            // Arrange
            var userId = 1;
            var points = 100;
            var reason = "Race win bonus";

            // Act
            var pointsAwardedEvent = new PointsAwardedEvent(userId, points, reason);

            // Assert
            Assert.NotNull(pointsAwardedEvent);
            Assert.Equal(userId, pointsAwardedEvent.UserId);
            Assert.Equal(points, pointsAwardedEvent.Points);
            Assert.Equal(reason, pointsAwardedEvent.Reason);
            Assert.True(pointsAwardedEvent.EventTimestamp <= DateTime.UtcNow);
            Assert.True(pointsAwardedEvent.EventTimestamp >= DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public void UserPendingBetsSpecification_ShouldCreateCorrectCriteria()
        {
            // Arrange
            var userId = 1;

            // Act
            var specification = new UserPendingBetsSpecification(userId);

            // Assert
            Assert.NotNull(specification);
            Assert.NotNull(specification.Criteria);
            Assert.NotNull(specification.Includes);
            Assert.NotNull(specification.IncludeStrings);
            Assert.Empty(specification.Includes);
            Assert.Empty(specification.IncludeStrings);
        }

        [Fact]
        public void RaceBetsSpecification_ShouldCreateCorrectCriteria()
        {
            // Arrange
            var raceId = 1;

            // Act
            var specification = new RaceBetsSpecification(raceId);

            // Assert
            Assert.NotNull(specification);
            Assert.NotNull(specification.Criteria);
            Assert.NotNull(specification.Includes);
            Assert.NotNull(specification.IncludeStrings);
            Assert.Empty(specification.Includes);
            Assert.Empty(specification.IncludeStrings);
        }

        [Fact]
        public void UpcomingRacesSpecification_ShouldCreateCorrectCriteria()
        {
            // Act
            var specification = new UpcomingRacesSpecification();

            // Assert
            Assert.NotNull(specification);
            Assert.NotNull(specification.Criteria);
            Assert.NotNull(specification.Includes);
            Assert.NotNull(specification.IncludeStrings);
        }

        [Fact]
        public async Task DomainEventPublisher_ShouldPublishEventsToSubscribers()
        {
            // Arrange
            var publisher = new DomainEventPublisher();
            var eventHandled = false;
            var testEvent = new BetPlacedEvent(
                new Bet(1, 1, 1, 100, Domain.Enums.BetType.RaceWinner, 2.5m),
                1, 1, 100);

            publisher.Subscribe<BetPlacedEvent>(async @event =>
            {
                eventHandled = true;
                await Task.CompletedTask;
            });

            // Act
            await publisher.PublishAsync(testEvent);

            // Assert
            Assert.True(eventHandled);
        }

        [Fact]
        public async Task DomainEventPublisher_ShouldHandleMultipleSubscribers()
        {
            // Arrange
            var publisher = new DomainEventPublisher();
            var subscriber1Handled = false;
            var subscriber2Handled = false;
            var testEvent = new RaceFinishedEvent(
                new Race("Test Race", DateTime.UtcNow.AddDays(-1), "Test Circuit", "Test Country", "race1", 2023));

            publisher.Subscribe<RaceFinishedEvent>(async @event =>
            {
                subscriber1Handled = true;
                await Task.CompletedTask;
            });

            publisher.Subscribe<RaceFinishedEvent>(async @event =>
            {
                subscriber2Handled = true;
                await Task.CompletedTask;
            });

            // Act
            await publisher.PublishAsync(testEvent);

            // Assert
            Assert.True(subscriber1Handled);
            Assert.True(subscriber2Handled);
        }

        [Fact]
        public async Task DomainEventPublisher_ShouldHandleSubscriberErrorsGracefully()
        {
            // Arrange
            var publisher = new DomainEventPublisher();
            var successfulHandlerCalled = false;
            var testEvent = new PointsAwardedEvent(1, 100, "Test");

            publisher.Subscribe<PointsAwardedEvent>(async @event =>
            {
                throw new Exception("Test exception");
            });

            publisher.Subscribe<PointsAwardedEvent>(async @event =>
            {
                successfulHandlerCalled = true;
                await Task.CompletedTask;
            });

            // Act
            await publisher.PublishAsync(testEvent);

            // Assert - Should not throw and should call the successful handler
            Assert.True(successfulHandlerCalled);
        }

        [Fact]
        public void DomainEventPublisher_UnsubscribeShouldWork()
        {
            // Arrange
            var publisher = new DomainEventPublisher();
            var handlerCalled = false;

            Func<BetPlacedEvent, Task> handler = async @event =>
            {
                handlerCalled = true;
                await Task.CompletedTask;
            };

            publisher.Subscribe(handler);
            publisher.Unsubscribe(handler);

            var testEvent = new BetPlacedEvent(
                new Bet(1, 1, 1, 100, Domain.Enums.BetType.RaceWinner, 2.5m),
                1, 1, 100);

            // Act
            publisher.PublishAsync(testEvent).Wait();

            // Assert
            Assert.False(handlerCalled);
        }
    }
}