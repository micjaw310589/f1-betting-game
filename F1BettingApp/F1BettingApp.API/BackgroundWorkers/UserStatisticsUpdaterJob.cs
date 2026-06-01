using F1BettingApp.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Dodano using
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UserStatisticsUpdaterJob> _logger;

        public UserStatisticsUpdaterJob(IServiceScopeFactory scopeFactory, ILogger<UserStatisticsUpdaterJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting user statistics update...");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        // Wyciągamy instancję IUserService przypisaną do tego konkretnego zakresu
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                        // Update statistics for all active users
                        var activeUsers = await userService.GetAllUsersAsync();
                        foreach (var user in activeUsers.Items)
                        {
                            await userService.UpdateUserStatisticsCacheAsync(user.Id);
                        }
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