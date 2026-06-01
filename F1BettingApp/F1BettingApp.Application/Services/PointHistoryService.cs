using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.Application.Services
{
    /// <summary>
    /// Service implementation for point history tracking.
    /// Records all point changes and provides query capabilities.
    /// </summary>
    public class PointHistoryService : IPointHistoryService
    {
        private readonly IPointHistoryRepository _pointHistoryRepository;

        public PointHistoryService(IPointHistoryRepository pointHistoryRepository)
        {
            _pointHistoryRepository = pointHistoryRepository;
        }

        /// <inheritdoc />
        public async Task RecordPointChangeAsync(int userId, int points, string category, string description, string source, int? referenceId = null)
        {
            var history = new PointHistory
            {
                UserId = userId,
                Points = points,
                Category = category,
                Description = description,
                ReferenceId = referenceId,
                Source = source,
                CreatedAt = DateTime.UtcNow
            };

            await _pointHistoryRepository.AddAsync(history);
        }

        /// <inheritdoc />
        public async Task<PointHistoryResponseDto> GetUserPointHistoryAsync(int userId, int page, int pageSize)
        {
            var totalCount = await _pointHistoryRepository.GetCountByUserIdAsync(userId);
            var items = await _pointHistoryRepository.GetByUserIdAsync(userId, page, pageSize);

            return new PointHistoryResponseDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc />
        public async Task<WeeklyPointSummaryDto> GetWeeklyPointSummaryAsync(int userId, int weekNumber, int year)
        {
            var (totalEarned, totalSpent) = await _pointHistoryRepository.GetWeeklySummaryAsync(userId, weekNumber, year);

            return new WeeklyPointSummaryDto
            {
                WeekNumber = weekNumber,
                Year = year,
                TotalEarned = totalEarned,
                TotalSpent = totalSpent
            };
        }

        private static PointHistoryDto MapToDto(PointHistory history)
        {
            return new PointHistoryDto
            {
                Id = history.Id,
                Points = history.Points,
                Category = history.Category,
                Description = history.Description,
                ReferenceId = history.ReferenceId,
                Source = history.Source,
                CreatedAt = history.CreatedAt
            };
        }
    }
}
