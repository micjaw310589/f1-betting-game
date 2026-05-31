using F1BettingApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace F1BettingApp.API.BackgroundWorkers;

public class OpenF1SyncJob : BackgroundService
{
    private readonly ILogger<OpenF1SyncJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _checkInterval;

    public OpenF1SyncJob(
        ILogger<OpenF1SyncJob> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        var intervalMinutes = configuration.GetValue<int>("BackgroundWorkers:OpenF1Sync:CheckIntervalMinutes", 60);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OpenF1SyncJob is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("OpenF1SyncJob starting synchronization.");
                await SyncRacesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in OpenF1SyncJob.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("OpenF1SyncJob is stopping.");
    }

    private async Task SyncRacesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();

        var result = await raceService.SyncRaceDataFromOpenF1Async();

        if (result.Success)
        {
            _logger.LogInformation(
                "OpenF1 sync completed. Processed: {Processed}, Created: {Created}, Updated: {Updated}.",
                result.RacesProcessed,
                result.RacesCreated,
                result.RacesUpdated);
        }
        else
        {
            _logger.LogWarning("OpenF1 sync failed: {ErrorMessage}", result.ErrorMessage);
        }
    }
}