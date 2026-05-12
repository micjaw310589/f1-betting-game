using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.API.BackgroundWorkers;

/// <summary>
/// Background worker that periodically checks for newly finished races
/// and triggers automatic bet processing.
/// Runs continuously as a hosted service.
/// </summary>
public class RaceStatusMonitorJob : BackgroundService
{
    private readonly ILogger<RaceStatusMonitorJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;

    public RaceStatusMonitorJob(
        ILogger<RaceStatusMonitorJob> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;

        // Read check interval from configuration (in minutes, default 5 minutes)
        var intervalMinutes = configuration.GetValue<int>("BackgroundWorkers:RaceStatusMonitor:CheckIntervalMinutes", 5);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RaceStatusMonitorJob is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("RaceStatusMonitorJob checking for finished races.");
                await ProcessFinishedRacesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in RaceStatusMonitorJob.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("RaceStatusMonitorJob is stopping.");
    }

    private async Task ProcessFinishedRacesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var bettingService = scope.ServiceProvider.GetRequiredService<IBettingService>();

        try
        {
            // Find races that are Finished but not yet ResultsProcessed
            var finishedRaces = await dbContext.Races
                .Where(r => r.Status == RaceStatus.Finished)
                .ToListAsync(stoppingToken);

            if (!finishedRaces.Any())
            {
                _logger.LogInformation("No finished races found to process.");
                return;
            }

            _logger.LogInformation("Found {Count} finished race(s) to process.", finishedRaces.Count);

            foreach (var race in finishedRaces)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation("Processing results for race ID {RaceId} - {RaceName}.", race.Id, race.Name);

                    // Verify the race still has results in the database before processing
                    var existingResults = await dbContext.Results
                        .Where(r => r.RaceId == race.Id)
                        .AnyAsync(stoppingToken);

                    if (!existingResults)
                    {
                        _logger.LogWarning("Race ID {RaceId} has no results yet. Skipping.", race.Id);
                        continue;
                    }

                    // Process the race results (this is idempotent)
                    await bettingService.ProcessRaceResultsAsync(race.Id);

                    _logger.LogInformation("Successfully processed results for race ID {RaceId} - {RaceName}.", race.Id, race.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing results for race ID {RaceId}.", race.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying for finished races.");
            throw;
        }
    }
}