using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets would be defined here
        // public DbSet<User> Users { get; set; }
        // public DbSet<Bet> Bets { get; set; }
        // public DbSet<Race> Races { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuration would go here
        }
    }
}