using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service implementation for quest definition admin operations.
    /// Includes validation, CRUD operations, and weekly reset functionality.
    /// </summary>
    public class QuestDefinitionService : IQuestDefinitionService
    {
        private readonly IQuestDefinitionRepository _questDefinitionRepository;
        private readonly IWeeklyQuestProgressRepository _weeklyQuestProgressRepository;

        private static readonly string[] ValidCategories = { "Betting", "Engagement", "Achievement" };
        private static readonly Regex QuestIdPattern = new(@"^[a-z_]+$", RegexOptions.Compiled);

        public QuestDefinitionService(
            IQuestDefinitionRepository questDefinitionRepository,
            IWeeklyQuestProgressRepository weeklyQuestProgressRepository)
        {
            _questDefinitionRepository = questDefinitionRepository;
            _weeklyQuestProgressRepository = weeklyQuestProgressRepository;
        }

        public async Task<List<QuestDto>> GetAllQuestDefinitionsAsync(bool? isActive = null)
        {
            var quests = await _questDefinitionRepository.GetAllAsync(isActive);
            var questList = quests.ToList(); // Materialize to List to avoid IAsyncEnumerable issues in tests

            return questList.Select(MapToDto).ToList();
        }

        public async Task<QuestDto> CreateQuestDefinitionAsync(CreateQuestDto dto)
        {
            ValidateQuestDto(dto, isUpdate: false);

            // Check QuestId uniqueness
            var existing = await _questDefinitionRepository.GetByQuestIdAsync(dto.QuestId);
            if (existing != null)
            {
                throw new InvalidOperationException($"Quest with QuestId '{dto.QuestId}' already exists.");
            }

            var quest = new QuestDefinition
            {
                QuestId = dto.QuestId,
                Name = dto.Name,
                Description = dto.Description,
                Category = Enum.Parse<QuestCategory>(dto.Category, true),
                IsOneTime = dto.IsOneTime,
                Target = dto.Target,
                PointsReward = dto.PointsReward,
                IsActive = dto.IsActive,
                Order = dto.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _questDefinitionRepository.CreateAsync(quest);
            return MapToDto(quest);
        }

        public async Task<QuestDto> UpdateQuestDefinitionAsync(int id, UpdateQuestDto dto)
        {
            var quest = await _questDefinitionRepository.GetByIdAsync(id);
            if (quest == null)
            {
                throw new KeyNotFoundException($"Quest with ID {id} not found.");
            }

            if (dto.Name != null) quest.Name = dto.Name;
            if (dto.Description != null) quest.Description = dto.Description;
            if (dto.Category != null)
            {
                ValidateCategory(dto.Category);
                quest.Category = Enum.Parse<QuestCategory>(dto.Category, true);
            }
            if (dto.IsOneTime != null) quest.IsOneTime = dto.IsOneTime.Value;
            if (dto.Target != null)
            {
                ValidateTarget(dto.Target.Value);
                quest.Target = dto.Target.Value;
            }
            if (dto.PointsReward != null)
            {
                ValidatePointsReward(dto.PointsReward.Value);
                quest.PointsReward = dto.PointsReward.Value;
            }
            if (dto.IsActive != null) quest.IsActive = dto.IsActive.Value;
            if (dto.Order != null) quest.Order = dto.Order.Value;

            quest.UpdatedAt = DateTime.UtcNow;

            await _questDefinitionRepository.UpdateAsync(quest);
            return MapToDto(quest);
        }

        public async Task DeleteQuestDefinitionAsync(int id)
        {
            var quest = await _questDefinitionRepository.GetByIdAsync(id);
            if (quest == null)
            {
                throw new KeyNotFoundException($"Quest with ID {id} not found.");
            }

            // Check for active progress records
            var activeCount = await _weeklyQuestProgressRepository.GetActiveProgressCountByQuestIdAsync(quest.QuestId);
            if (activeCount > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete quest '{quest.QuestId}' — {activeCount} user(s) have active progress records. " +
                    $"Reset progress or deactivate the quest first.");
            }

            await _questDefinitionRepository.DeleteAsync(id);
        }

        public async Task<QuestDto> ToggleQuestActiveAsync(int id, bool isActive)
        {
            var quest = await _questDefinitionRepository.GetByIdAsync(id);
            if (quest == null)
            {
                throw new KeyNotFoundException($"Quest with ID {id} not found.");
            }

            quest.IsActive = isActive;
            quest.UpdatedAt = DateTime.UtcNow;

            await _questDefinitionRepository.UpdateAsync(quest);
            return MapToDto(quest);
        }

        public async Task<int> ResetWeeklyQuestsAsync()
        {
            // Get current ISO week number and year
            var (weekNumber, year) = GetCurrentWeek();

            // Reset all weekly quest progress for all users for the current week
            return await _weeklyQuestProgressRepository.ResetAllWeeksAsync(weekNumber, year);
        }

        private static void ValidateQuestDto(CreateQuestDto dto, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(dto.QuestId))
            {
                throw new ArgumentException("QuestId is required.");
            }

            ValidateQuestIdPattern(dto.QuestId);

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Category))
            {
                throw new ArgumentException("Category is required.");
            }

            if (!ValidCategories.Contains(dto.Category, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid category '{dto.Category}'. Must be one of: {string.Join(", ", ValidCategories)}");
            }

            ValidateTarget(dto.Target);
            ValidatePointsReward(dto.PointsReward);
        }

        private static void ValidateTarget(int target)
        {
            if (target <= 0)
            {
                throw new ArgumentException("Target must be greater than 0.");
            }
        }

        private static void ValidatePointsReward(int pointsReward)
        {
            if (pointsReward < 0)
            {
                throw new ArgumentException("PointsReward must be greater than or equal to 0.");
            }
        }

        private static void ValidateCategory(string category)
        {
            if (!ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid category '{category}'. Must be one of: {string.Join(", ", ValidCategories)}");
            }
        }

        private static void ValidateQuestIdPattern(string questId)
        {
            if (!QuestIdPattern.IsMatch(questId))
            {
                throw new ArgumentException(
                    $"Invalid QuestId '{questId}'. Must match pattern ^[a-z_]+$ (lowercase letters and underscores only).");
            }
        }

        private static (int WeekNumber, int Year) GetCurrentWeek()
        {
            // Use ISO 8601 week date calculation
            var now = DateTime.UtcNow;
            var dayOfWeek = now.DayOfWeek;
            var mondayOfWeek = now.AddDays(-(dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));

            var jan1 = new DateTime(now.Year, 1, 1);
            var jan1DayOfWeek = jan1.DayOfWeek;
            var jan1Monday = jan1.AddDays(-(jan1DayOfWeek == DayOfWeek.Sunday ? 6 : (int)jan1DayOfWeek - 1));

            var daysSinceJan1Monday = (mondayOfWeek - jan1Monday).Days;
            var weekNumber = daysSinceJan1Monday / 7 + 1;

            // Simple fallback for edge cases
            if (weekNumber < 1 || weekNumber > 53)
            {
                weekNumber = 1;
            }

            return (weekNumber, now.Year);
        }

        private QuestDto MapToDto(QuestDefinition quest)
        {
            return new QuestDto
            {
                QuestId = quest.QuestId,
                Name = quest.Name,
                Description = quest.Description,
                Category = quest.Category.ToString(),
                IsOneTime = quest.IsOneTime,
                Target = quest.Target,
                Progress = 0,
                IsCompleted = false,
                IsClaimed = false,
                PointsReward = quest.PointsReward,
                IsActive = quest.IsActive
            };
        }
    }
}
