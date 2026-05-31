//cd F1BettingApp/F1BettingApp.Tests
//dotnet test --filter "F1BettingApp.Tests.UserStatisticsUpdaterJobTests"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.API.BackgroundWorkers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace F1BettingApp.Tests
{
    public class UserStatisticsUpdaterJobTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<UserStatisticsUpdaterJob>> _loggerMock;
        private readonly UserStatisticsUpdaterJob _job;
        private readonly CancellationTokenSource _cts;

        public UserStatisticsUpdaterJobTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UserStatisticsUpdaterJob>>();
            _job = new UserStatisticsUpdaterJob(_userServiceMock.Object, _loggerMock.Object);
            _cts = new CancellationTokenSource();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateStatisticsForAllActiveUsers()
        {
            // Arrange
            var activeUsers = new PagedResult<AdminUserDto>
            {
                Items = new List<AdminUserDto>
                {
                    new AdminUserDto { Id = 1, Username = "user1" },
                    new AdminUserDto { Id = 2, Username = "user2" },
                    new AdminUserDto { Id = 3, Username = "user3" }
                },
                Page = 1,
                PageSize = 20,
                TotalItems = 3,
                TotalPages = 1
            };

            _userServiceMock.Setup(service => service.GetAllUsersAsync(1, 20, null, null))
                .ReturnsAsync(activeUsers);

            // Act
            // Run the job for a short period to allow one iteration
            var task = _job.StartAsync(_cts.Token);
            await Task.Delay(100); // Wait for the job to start
            _cts.Cancel(); // Stop the job
            await task; // Wait for cleanup

            // Assert
            _userServiceMock.Verify(service => service.GetAllUsersAsync(1, 20, null, null), Times.Once);
            _userServiceMock.Verify(service => service.UpdateUserStatisticsCacheAsync(1), Times.Once);
            _userServiceMock.Verify(service => service.UpdateUserStatisticsCacheAsync(2), Times.Once);
            _userServiceMock.Verify(service => service.UpdateUserStatisticsCacheAsync(3), Times.Once);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting user statistics update")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User statistics update completed")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleErrorsGracefully()
        {
            // Arrange
            _userServiceMock.Setup(service => service.GetAllUsersAsync(1, 20, null, null))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var task = _job.StartAsync(_cts.Token);
            await Task.Delay(100); // Wait for the job to start and fail
            _cts.Cancel(); // Stop the job
            await task; // Wait for cleanup

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error updating user statistics")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStopWhenCancellationRequested()
        {
            // Arrange
            var activeUsers = new PagedResult<AdminUserDto>
            {
                Items = new List<AdminUserDto>
                {
                    new AdminUserDto { Id = 1, Username = "user1" }
                },
                Page = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            };

            _userServiceMock.Setup(service => service.GetAllUsersAsync(1, 20, null, null))
                .ReturnsAsync(activeUsers);

            // Act
            var task = _job.StartAsync(_cts.Token);
            await Task.Delay(50); // Wait briefly
            _cts.Cancel(); // Stop the job
            await task; // Wait for cleanup

            // Assert
            // The job should have started but been cancelled before completing
            _userServiceMock.Verify(service => service.GetAllUsersAsync(1, 20, null, null), Times.AtMostOnce);
            _userServiceMock.Verify(service => service.UpdateUserStatisticsCacheAsync(It.IsAny<int>()), Times.AtMostOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldRunContinuouslyUntilCancelled()
        {
            // Arrange
            var activeUsers = new PagedResult<AdminUserDto>
            {
                Items = new List<AdminUserDto>
                {
                    new AdminUserDto { Id = 1, Username = "user1" }
                },
                Page = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            };

            _userServiceMock.Setup(service => service.GetAllUsersAsync(1, 20, null, null))
                .ReturnsAsync(activeUsers);

            // Act
            var task = _job.StartAsync(_cts.Token);
            await Task.Delay(100); // Wait briefly for the job to start
            _cts.Cancel(); // Stop the job
            await task; // Wait for cleanup

            // Assert
            // Should have run at least once
            _userServiceMock.Verify(service => service.GetAllUsersAsync(1, 20, null, null), Times.AtLeastOnce);
        }
    }
}