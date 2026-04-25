using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<LeaderboardHistory> LeaderboardHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired();
                entity.Property(u => u.Email).IsRequired();
            });

            modelBuilder.Entity<Bet>(entity =>
            {
                entity.HasIndex(b => new { b.UserId, b.Status });
                entity.HasOne(b => b.User).WithMany(u => u.Bets).HasForeignKey(b => b.UserId);
                entity.HasOne(b => b.Race).WithMany(r => r.Bets).HasForeignKey(b => b.RaceId);
                entity.HasOne(b => b.Driver).WithMany(d => d.Bets).HasForeignKey(b => b.DriverId);
            });

            modelBuilder.Entity<Race>(entity =>
            {
                entity.HasIndex(r => r.Status);
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasOne(r => r.Race).WithMany(race => race.Results).HasForeignKey(r => r.RaceId);
                entity.HasOne(r => r.Driver).WithMany(driver => driver.Results).HasForeignKey(r => r.DriverId);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId);
            });

            modelBuilder.Entity<LeaderboardHistory>(entity =>
            {
                entity.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId);
                entity.HasOne(l => l.Race).WithMany().HasForeignKey(l => l.RaceId);
            });
        }
    }
}