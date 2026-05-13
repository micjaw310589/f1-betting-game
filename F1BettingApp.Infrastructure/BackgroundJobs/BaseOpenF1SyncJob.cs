using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using F1BettingApp.Domain.OpenF1;

namespace F1BettingApp.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Base class for OpenF1 synchronization hosted services.
    /// </summary>
    public abstract class BaseOpenF1SyncJob : IHostedService
    {
        protected readonly ILogger<BaseOpenF1SyncJob> _logger;
        protected readonly IOpenF1ApiClient _openF1ApiClient;
        protected readonly IServiceProvider _serviceProvider;

        public BaseOpenF1SyncJob(ILogger<BaseOpenF1SyncJob> logger, IOpenF1ApiClient openF1ApiClient, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _openF1ApiClient = openF1ApiClient;
            _serviceProvider = serviceProvider;
        }

        // The actual synchronization logic is implemented in derived classes
        protected abstract Task PerformSyncAsync(int season);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{GetType().Name}] Sync Job starting...");

            // Determine the current season (hardcoded for now, should be dynamically fetched)
            int currentSeason = DateTime.Now.Year;

            try
            {
                await PerformSyncAsync(currentSeason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{GetType().Name}] Sync Job failed to execute successfully.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{GetType().Name}] Sync Job stopping.");
        }
    }
}