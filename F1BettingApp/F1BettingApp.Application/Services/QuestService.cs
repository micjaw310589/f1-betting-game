using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Events;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service implementation for weekly quest operations.
    /// Handles quest progress tracking, completion evaluation, and point awards.
    /// </summary>
    public class QuestService : IQuestService
    {
        private readonly IQuestDefinitionRepository _questDefinitionRepository;
        private readonly IWeeklyQuestProgressRepository _questProgressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IPointHistoryService _pointHistoryService;

        public QuestService(
            IQuestDefinitionRepository questDefinitionRepository,
            IWeeklyQuestProgressRepository questProgressRepository,
            IUserRepository userRepository,
            IDomainEventPublisher domainEventPublisher,
            IPointHistoryService pointHistoryService)
        {
            _questDefinitionRepository = questDefinitionRepository;
            _questProgressRepository = questProgressRepository;
            _userRepository = userRepository;
            _domainEventPublisher = domainEventPublisher;
            _pointHistoryService = pointHistoryService;
        }

        public bool IsRaceWeekendDay(DateTime date)
        {
            // Race weekends typically fall on Friday (practice), Saturday (qualifying), Sunday (race)
            return date.DayOfWeek == DayOfWeek.Friday ||
                   date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }

        public async Task<QuestResponseDto> GetActiveQuestsAsync(int userId)
        {
            var quests = await _questDefinitionRepository.GetAllAsync(isActive: true);
            var questList = await quests.ToListAsync();

            var result = new QuestResponseDto();

            foreach (var quest in questList)
            {
                var progress = await GetQuestProgressAsync(userId, quest);
                result.Quests.Add(MapToDto(quest, progress));
            }

            return result;
        }

        public async Task<QuestBoardDto?> GetQuestBoardProgressAsync(string questId, int? userId)
        {
            var quest = await _questDefinitionRepository.GetByQuestIdAsync(questId);
            if (quest == null || !quest.IsActive)
            {
                return null;
            }

            QuestBoardDto dto = new()
            {
                QuestId = quest.QuestId,
                Name = quest.Name,
                Description = quest.Description,
                Category = quest.Category.ToString(),
                IsOneTime = quest.IsOneTime,
                Target = quest.Target,
                PointsReward = quest.PointsReward,
                IsActive = quest.IsActive,
                Order = quest.Order,
            };

            if (userId.HasValue)
            {
                var progress = await GetQuestProgressAsync(userId.Value, quest);
                dto.Progress = progress?.Progress ?? 0;
                dto.IsCompleted = progress?.IsCompleted ?? false;
                dto.IsClaimed = progress?.IsClaimed ?? false;
            }

            return dto;
        }

        public async Task<QuestDto?> GetQuestDefinitionAsync(string questId)
        {
            var quest = await _questDefinitionRepository.GetByQuestIdAsync(questId);
            if (quest == null) return null;

            return new QuestDto
            {
                QuestId = quest.QuestId,
                Name = quest.Name,
                Description = quest.Description,
                Category = quest.Category.ToString(),
                IsOneTime = quest.IsOneTime,
                Target = quest.Target,
                PointsReward = quest.PointsReward,
                IsActive = quest.IsActive
            };
        }

        public async Task CheckAndCompleteQuestsAsync(int userId)
        {
            var quests = await _questDefinitionRepository.GetAllAsync(isActive: true);
            var questList = await quests.ToListAsync();

            foreach (var quest in questList)
            {
                var progress = await GetQuestProgressAsync(userId, quest);

                // If progress meets or exceeds target and quest is not yet completed
                if (progress != null && !progress.IsCompleted && progress.Progress >= progress.Target)
                {
                    progress.IsCompleted = true;
                    progress.PointsAwarded = quest.PointsReward;
                    progress.IsClaimed = true;
                    progress.UpdatedAt = DateTime.UtcNow;

                    await _questProgressRepository.UpsertAsync(progress);

                    // Award points to the user
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        user.AddPoints(quest.PointsReward);
                        await _userRepository.UpdateAsync(user);

                        // Record point history
                        await _pointHistoryService.RecordPointChangeAsync(userId, quest.PointsReward, "Quest", $"Quest: {quest.Name}", "System");

                        // Publish domain event
                        await _domainEventPublisher.PublishAsync(new PointsAwardedEvent(userId, quest.PointsReward, $"Quest: {quest.QuestId}"));
                    }
                }
            }
        }

        public async Task UpdateQuestProgressAsync(int userId, string questId, int amount, string? additionalContext = null)
        {
            var quest = await _questDefinitionRepository.GetByQuestIdAsync(questId);
            if (quest == null || !quest.IsActive) return;

            var progress = await GetQuestProgressAsync(userId, quest, additionalContext);
            if (progress == null)
            {
                // Should not happen, but handle gracefully
                return;
            }

            // Special handling for consistent_bettor quest: check if the date has already been counted
            if (questId == "consistent_bettor" && additionalContext != null)
            {
                // The additionalContext contains a date string (yyyy-MM-dd)
                // Check if this date was already counted using the ReferenceId field
                if (progress.ReferenceId == additionalContext)
                {
                    // This date has already been counted, skip
                    return;
                }

                // Update the ReferenceId to track this date
                progress.ReferenceId = additionalContext;
            }

            // Increment progress
            progress.Progress += amount;

            // Check if target is met
            if (progress.Progress >= progress.Target && !progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.PointsAwarded = quest.PointsReward;
                progress.IsClaimed = true;
                progress.UpdatedAt = DateTime.UtcNow;

                await _questProgressRepository.UpsertAsync(progress);

                // Award points to the user
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.AddPoints(quest.PointsReward);
                    await _userRepository.UpdateAsync(user);

                    // Record point history
                    await _pointHistoryService.RecordPointChangeAsync(userId, quest.PointsReward, "Quest", $"Quest: {quest.Name}", "System");

                    // Publish domain event
                    await _domainEventPublisher.PublishAsync(new PointsAwardedEvent(userId, quest.PointsReward, $"Quest: {quest.QuestId}"));
                }
            }
            else
            {
                // Just update progress without completing
                progress.UpdatedAt = DateTime.UtcNow;
                await _questProgressRepository.UpsertAsync(progress);
            }
        }

        private async Task<WeeklyQuestProgress?> GetQuestProgressAsync(int userId, QuestDefinition quest, string? additionalContext = null)
        {
            if (quest.IsOneTime)
            {
                // For one-time quests, use week 0, year 0 as sentinel
                WeeklyQuestProgress? oneTimeProgress = await _questProgressRepository.GetAsync(userId, quest.QuestId, 0, 0);
                if (oneTimeProgress == null)
                {
                    oneTimeProgress = new WeeklyQuestProgress
                    {
                        UserId = userId,
                        QuestId = quest.QuestId,
                        WeekNumber = 0,
                        Year = 0,
                        Progress = 0,
                        Target = quest.Target,
                        IsCompleted = false,
                        PointsAwarded = 0,
                        IsClaimed = false,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _questProgressRepository.UpsertAsync(oneTimeProgress);
                }
                return oneTimeProgress;
            }

            // For recurring quests, use current ISO week
            var (weekNumber, year) = GetCurrentIsoWeek();

            var progress = await _questProgressRepository.GetAsync(userId, quest.QuestId, weekNumber, year);

            if (progress == null)
            {
                // Create new progress record for this week
                progress = new WeeklyQuestProgress
                {
                    UserId = userId,
                    QuestId = quest.QuestId,
                    WeekNumber = weekNumber,
                    Year = year,
                    Progress = 0,
                    Target = quest.Target,
                    IsCompleted = false,
                    PointsAwarded = 0,
                    IsClaimed = false,
                    UpdatedAt = DateTime.UtcNow
                };

                await _questProgressRepository.UpsertAsync(progress);
            }

            // Special handling for consistent_bettor quest
            // Uses additionalContext to track the date for unique day counting
            if (quest.QuestId == "consistent_bettor" && additionalContext != null)
            {
                // The additionalContext contains the date string to check for uniqueness
                // This is handled in the caller (BettingService) which determines
                // if this is a new day's bet
            }

            return progress;
        }

        private static (int weekNumber, int year) GetCurrentIsoWeek()
        {
            // Use the localized calendar for ISO week calculation (Monday start, first 4-day week)
            var calendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.Localized);
            var weekNumber = calendar.GetWeekOfYear(
                DateTime.UtcNow,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
            var year = DateTime.UtcNow.Year;

            // Handle year boundary for ISO week
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

        private QuestDto MapToDto(QuestDefinition quest, WeeklyQuestProgress? progress)
        {
            return new QuestDto
            {
                QuestId = quest.QuestId,
                Name = quest.Name,
                Description = quest.Description,
                Category = quest.Category.ToString(),
                IsOneTime = quest.IsOneTime,
                Target = quest.Target,
                Progress = progress?.Progress ?? 0,
                IsCompleted = progress?.IsCompleted ?? false,
                IsClaimed = progress?.IsClaimed ?? false,
                PointsReward = quest.PointsReward,
                IsActive = quest.IsActive
            };
        }
    }
}
