using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IRepository<Notification>> _notificationRepositoryMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<IRepository<Notification>>();
            _userRepositoryMock = new Mock<IRepository<User>>();
            _notificationService = new NotificationService(
                _notificationRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateNotification_WithValidData_Succeeds()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1 };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _notificationService.CreateNotificationAsync(1, "Test message", "SystemMessage");

            // Assert
            _notificationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Notification>()), Times.Once);
            _notificationRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkNotificationAsRead_UpdatesStatus()
        {
            // Arrange
            var notification = new Notification(1, "Test Title", "Test Message") { Id = 1, IsRead = false };

            _notificationRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(notification);

            // Act
            await _notificationService.MarkNotificationAsReadAsync(1);

            // Assert
            Assert.True(notification.IsRead);
            _notificationRepositoryMock.Verify(x => x.UpdateAsync(notification), Times.Once);
            _notificationRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUnreadNotifications_ReturnsOnlyUnread()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1 };
            var notifications = new List<Notification>
            {
                new Notification(1, "Title1", "Message1") { Id = 1, IsRead = false },
                new Notification(1, "Title2", "Message2") { Id = 2, IsRead = true },
                new Notification(1, "Title3", "Message3") { Id = 3, IsRead = false }
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            _notificationRepositoryMock.Setup(x => x.GetAllAsync())
                .Returns(Task.FromResult(notifications.AsQueryable()));

            // Act
            var result = await _notificationService.GetUnreadNotificationsAsync(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, n => Assert.False(n.IsRead));
            Assert.Contains(result, n => n.Id == 1);
            Assert.Contains(result, n => n.Id == 3);
            Assert.DoesNotContain(result, n => n.Id == 2);
        }

        [Fact]
        public async Task CreateNotification_ForMultipleUsers_Succeeds()
        {
            // Arrange
            var user1 = new User("user1", "user1@example.com", "password1") { Id = 1 };
            var user2 = new User("user2", "user2@example.com", "password2") { Id = 2 };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user1);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(2))
                .ReturnsAsync(user2);

            // Act
            await _notificationService.CreateNotificationAsync(1, "Message for user1", "BetWon");
            await _notificationService.CreateNotificationAsync(2, "Message for user2", "RaceResultProcessed");

            // Assert
            _notificationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Notification>()), Times.Exactly(2));
            _notificationRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateNotification_WithInvalidType_ThrowsException()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "password") { Id = 1 };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _notificationService.CreateNotificationAsync(1, "Test message", "InvalidType"));
        }

        [Fact]
        public async Task CreateNotification_ForNonExistentUser_ThrowsException()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _notificationService.CreateNotificationAsync(999, "Test message", "SystemMessage"));
        }
    }
}