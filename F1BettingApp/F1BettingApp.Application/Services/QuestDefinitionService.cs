using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service implementation for quest definition admin operations.
    /// </summary>
    public class QuestDefinitionService : IQuestDefinitionService
    {
        private readonly IQuestDefinitionRepository _questDefinitionRepository;

        public QuestDefinitionService(IQuestDefinitionRepository questDefinitionRepository)
        {
            _questDefinitionRepository = questDefinitionRepository;
        }

        public async Task<List<QuestDto>> GetAllQuestDefinitionsAsync(bool? isActive = null)
        {
            var quests = await _questDefinitionRepository.GetAllAsync(isActive);
            var questList = await quests.ToListAsync();

            return questList.Select(MapToDto).ToList();
        }

        public async Task<QuestDto> CreateQuestDefinitionAsync(CreateQuestDto dto)
        {
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
            if (dto.Category != null) quest.Category = Enum.Parse<QuestCategory>(dto.Category, true);
            if (dto.IsOneTime != null) quest.IsOneTime = dto.IsOneTime.Value;
            if (dto.Target != null) quest.Target = dto.Target.Value;
            if (dto.PointsReward != null) quest.PointsReward = dto.PointsReward.Value;
            if (dto.IsActive != null) quest.IsActive = dto.IsActive.Value;
            if (dto.Order != null) quest.Order = dto.Order.Value;

            quest.UpdatedAt = DateTime.UtcNow;

            await _questDefinitionRepository.UpdateAsync(quest);
            return MapToDto(quest);
        }

        public async Task DeleteQuestDefinitionAsync(int id)
        {
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
