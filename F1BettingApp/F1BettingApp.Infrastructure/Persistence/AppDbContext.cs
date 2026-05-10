using Microsoft.EntityFrameworkCore;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;

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
        public DbSet<Result> Results { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LeaderboardHistory> LeaderboardHistories { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Points).HasDefaultValue(10000);
            });

            // Configure Bet entity
            modelBuilder.Entity<Bet>(entity =>
            {
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Race>()
                      .WithMany(r => r.Bets)
                      .HasForeignKey(b => b.RaceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");
                entity.Property(b => b.Odds).HasColumnType("decimal(18,2)");
                entity.Property(b => b.PotentialWinnings).HasColumnType("decimal(18,2)");
                entity.Property(b => b.Winnings).HasColumnType("decimal(18,2)");

                // Map enums to strings for readability
                entity.Property(b => b.BetType)
                      .HasConversion(
                          v => v.ToString(),
                          v => (BetType)Enum.Parse(typeof(BetType), v));

                entity.Property(b => b.Status)
                      .HasConversion(
                          v => v.ToString(),
                          v => (BetStatus)Enum.Parse(typeof(BetStatus), v));
            });

            // Configure Race entity
            modelBuilder.Entity<Race>(entity =>
            {
                entity.HasIndex(r => r.OpenF1RaceId).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Circuit).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Country).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Date)
                      .HasConversion(
                          v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

                // Map race status enum to string
                entity.Property(r => r.Status)
                      .HasConversion(
                          v => v.ToString(),
                          v => (RaceStatus)Enum.Parse(typeof(RaceStatus), v));
            });

            // Configure Result entity
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasOne(r => r.Race)
                      .WithMany()
                      .HasForeignKey(r => r.RaceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Driver)
                      .WithMany()
                      .HasForeignKey(r => r.DriverId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(r => new { r.RaceId, r.DriverId }).IsUnique();
                entity.Property(r => r.Position).IsRequired();
                entity.Property(r => r.Points).HasDefaultValue(0);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany()
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(n => n.Title).IsRequired().HasMaxLength(255);
                entity.Property(n => n.Message).IsRequired();
            });

            // Configure LeaderboardHistory entity
            modelBuilder.Entity<LeaderboardHistory>(entity =>
            {
                entity.HasOne(l => l.User)
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Race)
                      .WithMany()
                      .HasForeignKey(l => l.RaceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(l => new { l.UserId, l.RaceId }).IsUnique();
                entity.Property(l => l.Season).IsRequired().HasMaxLength(50);
            });

            // Configure Driver entity
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.HasIndex(d => d.OpenF1DriverId).IsUnique();
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Country).IsRequired().HasMaxLength(50);
                entity.Property(d => d.OpenF1DriverId).IsRequired().HasMaxLength(50);

                entity.HasOne(d => d.Team)
                      .WithMany(t => t.Drivers)
                      .HasForeignKey(d => d.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Team entity
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasIndex(t => t.OpenF1TeamId).IsUnique();
                entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Country).IsRequired().HasMaxLength(50);
                entity.Property(t => t.OpenF1TeamId).IsRequired().HasMaxLength(50);
            });
        }
    }
}
