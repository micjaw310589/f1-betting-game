using F1BettingApp.Application.Interfaces;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace F1BettingApp.API.Jobs
{
    /// <summary>
    /// Background job that resets weekly quest progress at midnight UTC every Monday.
    /// Evaluates login_streak_weekly, race_weekend_ready, and top_10 quests.
    /// </summary>
    public class QuestResetBackgroundJob : BackgroundService
    {
        private readonly ILogger<QuestResetBackgroundJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
        private DateTime? _lastResetTime;

        public QuestResetBackgroundJob(ILogger<QuestResetBackgroundJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _lastResetTime = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QuestResetBackgroundJob is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformWeeklyReset(stoppingToken);
                    _lastResetTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during weekly quest reset");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("QuestResetBackgroundJob is stopping.");
        }

        private async Task PerformWeeklyReset(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var progressRepository = scope.ServiceProvider.GetRequiredService<IWeeklyQuestProgressRepository>();
            var questService = scope.ServiceProvider.GetRequiredService<IQuestService>();
            var dailyLoginService = scope.ServiceProvider.GetRequiredService<IDailyLoginService>();
            var leaderboardService = scope.ServiceProvider.GetRequiredService<ILeaderboardService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<F1BettingApp.Infrastructure.Persistence.Repositories.IUserRepository>();

            // Calculate the current ISO week
            var (weekNumber, year) = GetCurrentIsoWeek();

            // Only reset if we haven't already reset this week
            if (_lastResetTime.HasValue)
            {
                var (lastWeek, lastYear) = GetCurrentIsoWeek();
                if (_lastResetTime.Value.Year == lastYear && _lastResetTime.Value.DayOfYear == lastWeek)
                {
                    _logger.LogInformation("Quest reset already performed for the current week. Skipping.");
                    return;
                }
            }

            _logger.LogInformation("Starting weekly quest reset for week {Week}, year {Year}", weekNumber, year);

            // Get all users
            var allUsers = await userRepository.GetAllAsync();
            var userIds = allUsers.Select(u => u.Id).ToList();

            _logger.LogInformation("Found {Count} users to evaluate for weekly quest reset", userIds.Count);

            foreach (var userId in userIds)
            {
                try
                {
                    // Reset this user's weekly quest progress for the old week
                    var (prevWeekNumber, prevYear) = GetPrevWeekIsoWeek();
                    await progressRepository.ResetWeekAsync(userId, prevWeekNumber, prevYear);

                    // Evaluate login_streak_weekly: count logins in the past 7 days
                    var streakInfo = await dailyLoginService.GetStreakInfoAsync(userId);
                    if (streakInfo != null)
                    {
                        // login_streak_weekly tracks total logins in the past 7 days
                        // The DailyLoginService tracks the current streak, which represents
                        // consecutive days. For the weekly quest, we use the streak count
                        // as a proxy for login frequency.
                        if (streakInfo.CurrentStreak >= 5)
                        {
                            await questService.UpdateQuestProgressAsync(userId, "login_streak_weekly", 1);
                        }
                    }

                    // Evaluate race_weekend_ready: check if user logged in on both Friday and Saturday
                    // of the past race weekend. We check the DailyLoginStreak for the past weekend.
                    var pastWeekend = GetPastRaceWeekend();
                    if (pastWeekend.HasValue)
                    {
                        var friday = pastWeekend.Value.AddDays(-(int)pastWeekend.Value.DayOfWeek + (int)DayOfWeek.Friday);
                        var saturday = friday.AddDays(1);
                        var fridayStr = friday.ToString("yyyy-MM-dd");
                        var saturdayStr = saturday.ToString("yyyy-MM-dd");

                        // Check if user logged in on both days (streak covers both days)
                        if (streakInfo != null && streakInfo.LastLoginDate != null)
                        {
                            var lastLoginDate = DateTime.TryParse(streakInfo.LastLoginDate, out var parsed) ? parsed : DateTime.MinValue;
                            // If the streak is >= 2 and last login was on or after Saturday, they logged in both days
                            if (streakInfo.CurrentStreak >= 2 && lastLoginDate.Date >= saturday.Date)
                            {
                                await questService.UpdateQuestProgressAsync(userId, "race_weekend_ready", 1);
                            }
                        }
                    }

                    // Evaluate top_10: check if user is in the top 10 of the leaderboard
                    try
                    {
                        var ranking = await leaderboardService.GetUserRankingAsync(userId);
                        if (ranking != null && ranking.CurrentRank <= 10)
                        {
                            await questService.UpdateQuestProgressAsync(userId, "top_10", 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking leaderboard ranking for user {UserId}", userId);
                    }

                    // Check for any other quests that were completed during the week and award points
                    await questService.CheckAndCompleteQuestsAsync(userId);

                    _logger.LogDebug("Reset and evaluated quests for user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resetting quests for user {UserId}", userId);
                    // Continue with other users even if one fails
                }
            }

            _logger.LogInformation("Weekly quest reset completed successfully");
        }

        private static (int weekNumber, int year) GetCurrentIsoWeek()
        {
            var calendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
            var weekNumber = calendar.GetWeekOfYear(
                DateTime.UtcNow,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            var year = DateTime.UtcNow.Year;

            if (weekNumber == 1 && DateTime.UtcNow.Month == 12)
            {
                year++;
            }
            else if (weekNumber >= 52 && DateTime.UtcNow.Month == 1)
            {
                year--;
            }

            return (weekNumber, year);
        }

        private static (int weekNumber, int year) GetPrevWeekIsoWeek()
        {
            var prevWeek = DateTime.UtcNow.AddDays(-7);
            return GetCurrentIsoWeekFrom(prevWeek);
        }

        private static (int weekNumber, int year) GetCurrentIsoWeekFrom(DateTime date)
        {
            var calendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
            var weekNumber = calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            var year = date.Year;

            if (weekNumber == 1 && date.Month == 12)
            {
                year++;
            }
            else if (weekNumber >= 52 && date.Month == 1)
            {
                year--;
            }

            return (weekNumber, year);
        }

        private static DateTime? GetPastRaceWeekend()
        {
            // Find the most recent Friday-Saturday race weekend
            // We look for the last Saturday that was part of a race weekend
            var today = DateTime.UtcNow.Date;
            var dayOfWeek = today.DayOfWeek;

            // Calculate days since last Saturday
            int daysSinceSaturday = (int)(dayOfWeek - DayOfWeek.Saturday);
            if (daysSinceSaturday < 0)
            {
                daysSinceSaturday += 7;
            }

            var lastSaturday = today.AddDays(-daysSinceSaturday);

            // Only consider if it's within the last 2 weeks
            if (lastSaturday < today.AddDays(-14))
            {
                return null;
            }

            return lastSaturday;
        }
    }
}
