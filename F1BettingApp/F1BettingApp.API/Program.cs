using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.API.BackgroundWorkers;
using F1BettingApp.API.Jobs;
using F1BettingApp.Domain.Events;
using F1BettingApp.Infrastructure.Events;
using F1BettingApp.Infrastructure.OpenF1;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings for frontend compatibility
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Use camelCase property names to match frontend expectations
        // options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("https://f1-betting-game-qy5l.vercel.app"
        // ,"http://localhost:4200" //TODO: DELETE THIS LATER PRBLY
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    try
    {
        // Parse the Render URL into a standard Npgsql connection string
        // Supports formats: postgres://user:pass@host:port/db or postgresql://user:pass@host:port/db
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo;

        // Split userinfo - password may contain ':' so split only on first ':'
        var firstColonIndex = userInfo.IndexOf(':');
        if (firstColonIndex == -1)
        {
            throw new InvalidOperationException($"Invalid DATABASE_URL format: missing password separator. Expected format: postgres://username:password@host:port/database");
        }

        var username = userInfo.Substring(0, firstColonIndex);
        var password = userInfo.Substring(firstColonIndex + 1);

        // URL-decode username and password (passwords may contain encoded characters like %40 for @)
        username = Uri.UnescapeDataString(username);
        password = Uri.UnescapeDataString(password);

        var databaseName = databaseUri.LocalPath.TrimStart('/');

        var npgsqlBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port == -1 ? 5432 : databaseUri.Port,
            Username = username,
            Password = password,
            Database = databaseName,
            SslMode = SslMode.Require,
            // Prevent connection string from including the original URL (which could leak credentials)
            Pooling = true
        };
        connectionString = npgsqlBuilder.ToString();
    }
    catch (Exception ex)
    {
        // Log the error but provide a readable connection string for debugging
        Console.Error.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        Console.Error.WriteLine($"DATABASE_URL pattern: {databaseUrl.Substring(0, Math.Min(20, databaseUrl.Length))}...{databaseUrl.Substring(Math.Max(0, databaseUrl.Length - 10))}");
        throw;
    }
}

// // Use fallback only if no connection string was configured
// if (string.IsNullOrEmpty(connectionString))
// {
//     connectionString = "Host=localhost;Database=F1BettingApp;Username=postgres;Password=";
//     Console.WriteLine("WARNING: Using default local connection string. No DefaultConnection or DATABASE_URL found.");
// }

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("F1BettingApp.Infrastructure")));

        // Register repositories and Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IDriverRepository, DriverRepository>();
        builder.Services.AddScoped<ITeamRepository, TeamRepository>();
        builder.Services.AddScoped<IBetRepository, BetRepository>();
        builder.Services.AddScoped<IRaceRepository, RaceRepository>();
        builder.Services.AddScoped<IResultRepository, ResultRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IDailyLoginStreakRepository, DailyLoginStreakRepository>();
        builder.Services.AddScoped<IQuestDefinitionRepository, QuestDefinitionRepository>();
        builder.Services.AddScoped<IWeeklyQuestProgressRepository, WeeklyQuestProgressRepository>();
        builder.Services.AddScoped<IPointHistoryRepository, PointHistoryRepository>();
        builder.Services.AddScoped(typeof(F1BettingApp.Infrastructure.Persistence.Repositories.IRepository<>), typeof(F1BettingApp.Infrastructure.Persistence.Repositories.Repository<>));
        builder.Services.AddScoped<IBetRepositoryExtensions, BetRepositoryExtensions>();
        // Explicit registration for UserBetStatisticsCache repository
        builder.Services.AddScoped<IRepository<F1BettingApp.Domain.Entities.UserBetStatisticsCache>, F1BettingApp.Infrastructure.Persistence.Repositories.Repository<F1BettingApp.Domain.Entities.UserBetStatisticsCache>>();
        builder.Services.AddScoped<IRaceRepositoryExtensions, RaceRepositoryExtensions>();

// Register OpenF1 settings and HttpClient
builder.Services.Configure<OpenF1Client.OpenF1Settings>(builder.Configuration.GetSection("OpenF1"));
builder.Services.AddHttpClient("OpenF1", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("OpenF1:BaseUrl") ?? "https://api.openf1.org");
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("OpenF1:TimeoutSeconds", 30));
});

// Register application services
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IBettingService, BettingService>();
builder.Services.AddScoped<IRaceService, RaceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDailyLoginService, DailyLoginService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IQuestDefinitionService, QuestDefinitionService>();
builder.Services.AddScoped<IPointHistoryService, PointHistoryService>();
builder.Services.AddScoped<IOpenF1ApiClient, OpenF1Client>();

// Register domain event publisher
builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

 // Register background workers
builder.Services.AddHostedService<RaceStatusMonitorJob>();
builder.Services.AddHostedService<UserStatisticsUpdaterJob>();
builder.Services.AddHostedService<OpenF1SyncJob>();

// Configure ASP.NET Core Identity for user management
//builder.Services.AddIdentity<F1BettingApp.Domain.Entities.User, Microsoft.AspNetCore.Identity.IdentityRole<int>>()
//    .AddEntityFrameworkStores<AppDbContext>()
//    .AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSettings);

// Register JWT authentication handler
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "fallback-secret-key")),
        ValidIssuer = jwtSettings["Issuer"] ?? "F1BettingApp",
        ValidAudience = jwtSettings["Audience"] ?? "F1BettingApp",
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.Headers.Append("X-Error", "Authentication failed");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("X-Error", "Access denied");
            return Task.CompletedTask;
        }
    };
});

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
// Register JWT authentication in the pipeline
builder.Services.AddAuthorization();

// Register HttpContext access for application services
builder.Services.AddHttpContextAccessor();

// Memory cache for caching
builder.Services.AddMemoryCache();

var app = builder.Build();

// Helper function for applying migrations and seeding data
// async Task ApplyMigrationsAndSeedAsync()
// {
//     using var scope = app.Services.CreateScope();
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<AppDbContext>();
//     var logger = services.GetRequiredService<ILogger<Program>>();

//     try
//     {
//         await context.Database.MigrateAsync();
//     }
//     catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
//     {
//         logger.LogWarning(ex, "Pending model changes detected - applying missing migration manually");
//         // Apply the IsManuallyOverridden column if it doesn't exist
//         await context.Database.ExecuteSqlRawAsync(
//             "ALTER TABLE \"Races\" ADD COLUMN IF NOT EXISTS \"IsManuallyOverridden\" boolean NOT NULL DEFAULT false");
//     }

//     // Seed initial data (admin user, teams, etc.)
//     await SeedData.Initialize(context);
// }

// Apply migrations and seed data at startup (always)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            // Seed initial data
            await SeedData.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while applying migrations or seeding data.");
        }
    }

    // Apply migrations and seed data in development
    // await ApplyMigrationsAndSeedAsync();
// }

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 30;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
