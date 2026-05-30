using F1BettingApp.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace F1BettingApp.API.BackgroundWorkers
{
    public class UserStatisticsUpdaterJob : BackgroundService
    {
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(1);
        private readonly IUserService _userService;
        private readonly ILogger<UserStatisticsUpdaterJob> _logger;

        public UserStatisticsUpdaterJob(IUserService userService, ILogger<UserStatisticsUpdaterJob> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting user statistics update...");

                    // Update statistics for all active users
                    var activeUsers = await _userService.GetAllUsersAsync();
                    foreach (var user in activeUsers.Items)
                    {
                        await _userService.UpdateUserStatisticsCacheAsync(user.Id);
                    }

                    _logger.LogInformation("User statistics update completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user statistics");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
        }
    }
}