using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Events;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service implementation for daily login streak management.
    /// Awards points based on consecutive login streaks with increasing multipliers.
    /// </summary>
    public class DailyLoginService : IDailyLoginService
    {
        private const int DailyBasePoints = 10;

        private readonly IDailyLoginStreakRepository _streakRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IDomainEventPublisher _eventPublisher;
        private readonly IPointHistoryService _pointHistoryService;

        public DailyLoginService(
            IDailyLoginStreakRepository streakRepository,
            IRepository<User> userRepository,
            IDomainEventPublisher eventPublisher,
            IPointHistoryService pointHistoryService)
        {
            _streakRepository = streakRepository;
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _pointHistoryService = pointHistoryService;
        }

        /// <inheritdoc />
        public async Task<int> ProcessDailyLoginAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;

            // Check if the user already has a streak record
            var existingStreak = await _streakRepository.GetByUserIdAsync(userId);

            if (existingStreak == null)
            {
                // New user — create streak with day 1
                var streak = new DailyLoginStreak
                {
                    UserId = userId,
                    CurrentStreak = 1,
                    LastLoginDate = today,
                    ClaimedToday = true,
                    UpdatedAt = DateTime.UtcNow
                };

                await _streakRepository.UpsertAsync(streak);

                // Award points for day 1
                var points = CalculatePoints(1);
                await AwardPointsAsync(userId, points, "Daily login streak day 1");
                return points;
            }

            // User already has a streak record
            if (existingStreak.ClaimedToday)
            {
                // Already claimed today — return 0 points
                return 0;
            }

            // Check if last login was yesterday (streak continues) or earlier (streak resets)
            var lastLoginDate = existingStreak.LastLoginDate.Date;
            var yesterday = today.AddDays(-1);

            if (lastLoginDate == yesterday)
            {
                // Consecutive day — increment streak
                existingStreak.CurrentStreak++;
            }
            else
            {
                // Missed a day — reset streak to 1
                existingStreak.CurrentStreak = 1;
            }

            // Update the streak record
            existingStreak.LastLoginDate = today;
            existingStreak.ClaimedToday = true;
            existingStreak.UpdatedAt = DateTime.UtcNow;

            await _streakRepository.UpsertAsync(existingStreak);

            // Award points based on new streak
            var earnedPoints = CalculatePoints(existingStreak.CurrentStreak);
            await AwardPointsAsync(userId, earnedPoints, $"Daily login streak day {existingStreak.CurrentStreak}");
            return earnedPoints;
        }

        /// <inheritdoc />
        public async Task<DailyStreakInfoDto?> GetStreakInfoAsync(int userId)
        {
            var streak = await _streakRepository.GetByUserIdAsync(userId);

            if (streak == null)
            {
                return null;
            }

            var pointsToday = streak.ClaimedToday ? CalculatePoints(streak.CurrentStreak) : 0;
            var (nextMilestone, pointsAtNextMilestone) = GetNextBonusMilestone(streak.CurrentStreak);

            return new DailyStreakInfoDto
            {
                CurrentStreak = streak.CurrentStreak,
                LastLoginDate = streak.LastLoginDate.ToString("yyyy-MM-dd"),
                PointsToday = pointsToday,
                NextBonusMilestone = nextMilestone,
                PointsAtNextMilestone = pointsAtNextMilestone
            };
        }

        /// <summary>
        /// Calculates the effective points based on the current streak day.
        /// </summary>
        /// <param name="streakDays">The current consecutive login streak count.</param>
        /// <returns>The number of points to award.</returns>
        private static int CalculatePoints(int streakDays)
        {
            var multiplier = GetMultiplier(streakDays);
            return (int)(DailyBasePoints * multiplier);
        }

        /// <summary>
        /// Returns the bonus multiplier for a given streak day.
        /// </summary>
        /// <param name="streakDays">The current consecutive login streak count.</param>
        /// <returns>The bonus multiplier.</returns>
        private static double GetMultiplier(int streakDays)
        {
            return streakDays switch
            {
                >= 7 => 2.5,
                >= 5 => 2.0,
                >= 3 => 1.5,
                _ => 1.0
            };
        }

        /// <summary>
        /// Determines the next bonus milestone and the points at that milestone.
        /// </summary>
        /// <param name="currentStreak">The current streak count.</param>
        /// <returns>A tuple of (next milestone day, points at that milestone), or (null, null) if max reached.</returns>
        private static (int? Milestone, int? Points) GetNextBonusMilestone(int currentStreak)
        {
            if (currentStreak >= 7)
            {
                return (null, null);
            }

            if (currentStreak >= 5)
            {
                return (7, CalculatePoints(7));
            }

            if (currentStreak >= 3)
            {
                return (5, CalculatePoints(5));
            }

            if (currentStreak >= 1)
            {
                return (3, CalculatePoints(3));
            }

            return (1, CalculatePoints(1));
        }

        /// <summary>
        /// Awards points to a user, publishes the domain event, and saves changes.
        /// </summary>
        /// <param name="userId">The user ID to award points to.</param>
        /// <param name="pointsAwarded">The number of points to award.</param>
        /// <param name="reason">The reason for the points award.</param>
        private async Task AwardPointsAsync(int userId, int pointsAwarded, string reason)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            user.AddPoints(pointsAwarded);
            await _userRepository.UpdateAsync(user);

            // Record point history
            await _pointHistoryService.RecordPointChangeAsync(userId, pointsAwarded, "DailyLogin", reason, "System");

            // Publish the domain event
            await _eventPublisher.PublishAsync(new PointsAwardedEvent(userId, pointsAwarded, reason));
        }
    }
}
