using F1BettingApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace F1BettingApp.API.BackgroundWorkers;

public class OpenF1SyncJob : BackgroundService
{
    private readonly ILogger<OpenF1SyncJob> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _checkInterval;
    private DateTime _lastRaceSyncTime = DateTime.MinValue;

    public OpenF1SyncJob(
        ILogger<OpenF1SyncJob> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        
        // Zmieniamy interwał na bezpieczne 30 sekund - koniec z bombardowaniem API
        _checkInterval = TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OpenF1SyncJob is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("OpenF1SyncJob executing synchronization loop iteration.");
                await SyncDataSmartAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during OpenF1SyncJob execution.");
            }

            // Oczekiwanie 30 sekund przed kolejnym obrotem
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("OpenF1SyncJob is stopping.");
    }

    private async Task SyncDataSmartAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();
        
        var now = DateTime.UtcNow;

        // 1. INTELIGENTNA SYNCHRONIZACJA WYŚCIGÓW - tylko raz na godzinę!
        if ((now - _lastRaceSyncTime).TotalMinutes >= 60)
        {
            try
            {
                _logger.LogInformation("Syncing heavy race calendar data (once per hour)...");
                var result = await raceService.SyncRaceDataFromOpenF1Async();
                if (result.Success)
                {
                    _logger.LogInformation("Race data calendar successfully refreshed.");
                    _lastRaceSyncTime = now; // Aktualizujemy czas ostatniego sukcesu
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync race calendar data due to heavy API restrictions. Will retry later.");
            }
        }

        // 2. SYNCHRONIZACJA KLASYFIKACJI - leci co 30 sekund (bardzo lekkie zapytanie)
        try
        {
            int currentSeason = DateTime.UtcNow.Year;
            _logger.LogInformation("Syncing driver championship standings from 'latest' session...");
            await raceService.SyncChampionshipFromOpenF1Async(currentSeason);
            _logger.LogInformation("Championship standings updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute automatic championship standings update.");
        }
    }
}