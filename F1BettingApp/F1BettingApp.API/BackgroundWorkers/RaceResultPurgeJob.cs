using F1BettingApp.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace F1BettingApp.API.BackgroundWorkers;

/// <summary>
/// Background job that purges race results for seasons older than the current season.
/// Runs daily at midnight UTC.
/// </summary>
public class RaceResultPurgeJob : IHostedService
{
    private readonly ILogger<RaceResultPurgeJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer;

    public RaceResultPurgeJob(
        ILogger<RaceResultPurgeJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RaceResultPurgeJob starting.");

        // Run initial purge on startup
        _ = Task.Run(() => PurgeOldResultsAsync(cancellationToken), cancellationToken);

        // Schedule recurring purge every 24 hours
        _timer = new Timer(
            async state => await PurgeOldResultsAsync(cancellationToken),
            null,
            TimeSpan.FromHours(24),
            TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RaceResultPurgeJob stopping.");

        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        _timer?.Dispose();

        return Task.CompletedTask;
    }

    private async Task PurgeOldResultsAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting race result purge operation.");

            using var scope = _serviceProvider.CreateScope();
            var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();

            var purgedCount = await raceService.PurgeOldSeasonResultsAsync();

            if (purgedCount > 0)
            {
                _logger.LogInformation("RaceResultPurgeJob: Purged {Count} old season race results.", purgedCount);
            }
            else
            {
                _logger.LogInformation("RaceResultPurgeJob: No old season race results to purge.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaceResultPurgeJob: Error during race result purge operation.");
        }
    }
}