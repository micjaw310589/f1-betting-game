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
        public DbSet<Result> Results { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<LeaderboardHistory> LeaderboardHistories { get; set; }

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
            });

            // Configure Race entity
            modelBuilder.Entity<Race>(entity =>
            {
                entity.HasIndex(r => r.OpenF1RaceId).IsUnique();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Circuit).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Country).IsRequired().HasMaxLength(50);
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
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.RaceId, r.DriverId }).IsUnique();
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
        }
    }
}