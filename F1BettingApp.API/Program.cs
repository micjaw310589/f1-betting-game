using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using F1BettingApp.Infrastructure.OpenF1;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Infrastructure.BackgroundJobs;
using F1BettingApp.Application.Services;
using F1BettingApp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Configure OpenF1 API Client
// Use a HttpClient configured with the base URL
builder.Services.AddHttpClient<IOpenF1ApiClient, OpenF1Client>();

// 2. Register Sync Services (Application layer coordination)
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<ISyncPersistenceService, SyncPersistenceService>();

// 3. Register Background Services (Synchronization Jobs)
// Use the background service pattern provided by IHostedService
builder.Services.AddHostedService<RaceCalendarSyncJob>();
builder.Services.AddHostedService<StandingsSyncJob>();
builder.Services.AddHostedService<DriverTeamSyncJob>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Note: Background services (HostedServices) are automatically managed by the Host Builder.
// They will run when the application starts.

app.Run();