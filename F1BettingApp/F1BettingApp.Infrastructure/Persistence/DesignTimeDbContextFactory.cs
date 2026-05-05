using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace F1BettingApp.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time DbContext factory for EF Core migrations.
    /// Allows the EF Core tools to create a DbContext during design-time
    /// without requiring the full application to be running.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Host=localhost;Database=F1BettingApp;Username=postgres;Password=";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}