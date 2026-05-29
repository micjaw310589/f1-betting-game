using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.Services;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F1BettingApp.Tests
{
    /// <summary>
    /// Unit tests for QuestDefinitionService using fake repositories.
    /// Tests CRUD operations, validation, and weekly reset functionality.
    /// </summary>
    public class QuestDefinitionServiceTests
    {
        private readonly FakeQuestDefinitionRepository _questRepo;
        private readonly FakeWeeklyQuestProgressRepository _progressRepo;
        private readonly QuestDefinitionService _service;

        public QuestDefinitionServiceTests()
        {
            _questRepo = new FakeQuestDefinitionRepository();
            _progressRepo = new FakeWeeklyQuestProgressRepository();
            _service = new QuestDefinitionService(_questRepo, _progressRepo);
        }

        #region GetAllQuestDefinitionsAsync Tests

        [Fact]
        public async Task GetAllQuestDefinitionsAsync_ReturnsAllQuests()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, QuestId = "first_bet", Name = "First Bet", Category = QuestCategory.Betting, IsActive = true, Order = 1 });
            _questRepo.Add(new QuestDefinition { Id = 2, QuestId = "login_streak", Name = "Login Streak", Category = QuestCategory.Engagement, IsActive = false, Order = 2 });

            // Act
            var result = await _service.GetAllQuestDefinitionsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("first_bet", result[0].QuestId);
            Assert.Equal("login_streak", result[1].QuestId);
        }

        [Fact]
        public async Task GetAllQuestDefinitionsAsync_FilterActive_ReturnsOnlyActive()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, QuestId = "active_quest", Name = "Active", Category = QuestCategory.Betting, IsActive = true });
            _questRepo.Add(new QuestDefinition { Id = 2, QuestId = "inactive_quest", Name = "Inactive", Category = QuestCategory.Engagement, IsActive = false });

            // Act
            var result = await _service.GetAllQuestDefinitionsAsync(isActive: true);

            // Assert
            Assert.Single(result);
            Assert.True(result[0].IsActive);
        }

        #endregion

        #region CreateQuestDefinitionAsync Tests

        [Fact]
        public async Task CreateQuestDefinitionAsync_ValidDto_CreatesQuest()
        {
            // Arrange
            var dto = new CreateQuestDto
            {
                QuestId = "new_quest",
                Name = "New Quest",
                Description = "A new quest",
                Category = "Betting",
                IsOneTime = false,
                Target = 10,
                PointsReward = 500,
                IsActive = true,
                Order = 1
            };

            // Act
            var result = await _service.CreateQuestDefinitionAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new_quest", result.QuestId);
            Assert.Equal("New Quest", result.Name);
            Assert.Equal(500, result.PointsReward);
            Assert.Equal(1, _questRepo.Count);
        }

        [Fact]
        public async Task CreateQuestDefinitionAsync_DuplicateQuestId_ThrowsException()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, QuestId = "existing_quest" });
            var dto = new CreateQuestDto
            {
                QuestId = "existing_quest",
                Name = "New Quest",
                Category = "Betting",
                Target = 10,
                PointsReward = 500
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateQuestDefinitionAsync(dto));
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task CreateQuestDefinitionAsync_InvalidQuestIdPattern_ThrowsException()
        {
            // Arrange
            var dto = new CreateQuestDto
            {
                QuestId = "Invalid-Quest-Id", // Contains uppercase and hyphens
                Name = "New Quest",
                Category = "Betting",
                Target = 10,
                PointsReward = 500
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateQuestDefinitionAsync(dto));
            Assert.Contains("^[a-z_]+$", exception.Message);
        }

        [Fact]
        public async Task CreateQuestDefinitionAsync_ZeroTarget_ThrowsException()
        {
            // Arrange
            var dto = new CreateQuestDto
            {
                QuestId = "zero_target",
                Name = "Quest",
                Category = "Betting",
                Target = 0,
                PointsReward = 500
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateQuestDefinitionAsync(dto));
            Assert.Contains("greater than 0", exception.Message);
        }

        [Fact]
        public async Task CreateQuestDefinitionAsync_NegativePointsReward_ThrowsException()
        {
            // Arrange
            var dto = new CreateQuestDto
            {
                QuestId = "negative_reward",
                Name = "Quest",
                Category = "Betting",
                Target = 10,
                PointsReward = -100
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateQuestDefinitionAsync(dto));
            Assert.Contains("greater than or equal to 0", exception.Message);
        }

        [Fact]
        public async Task CreateQuestDefinitionAsync_InvalidCategory_ThrowsException()
        {
            // Arrange
            var dto = new CreateQuestDto
            {
                QuestId = "invalid_category",
                Name = "Quest",
                Category = "InvalidCategory",
                Target = 10,
                PointsReward = 500
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateQuestDefinitionAsync(dto));
            Assert.Contains("Must be one of", exception.Message);
        }

        #endregion

        #region UpdateQuestDefinitionAsync Tests

        [Fact]
        public async Task UpdateQuestDefinitionAsync_ValidUpdate_UpdatesFields()
        {
            // Arrange
            var existingQuest = new QuestDefinition
            {
                Id = 1,
                QuestId = "existing_quest",
                Name = "Old Name",
                Description = "Old description",
                Category = QuestCategory.Betting,
                IsOneTime = false,
                Target = 5,
                PointsReward = 100,
                IsActive = true,
                Order = 1
            };
            _questRepo.Add(existingQuest);

            var dto = new UpdateQuestDto
            {
                Name = "Updated Name",
                Target = 10,
                PointsReward = 500
            };

            // Act
            var result = await _service.UpdateQuestDefinitionAsync(1, dto);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(10, result.Target);
            Assert.Equal(500, result.PointsReward);
        }

        [Fact]
        public async Task UpdateQuestDefinitionAsync_InvalidTarget_ThrowsException()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, Name = "Quest" });
            var dto = new UpdateQuestDto { Target = 0 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateQuestDefinitionAsync(1, dto));
            Assert.Contains("greater than 0", exception.Message);
        }

        [Fact]
        public async Task UpdateQuestDefinitionAsync_InvalidCategory_ThrowsException()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, Name = "Quest" });
            var dto = new UpdateQuestDto { Category = "InvalidCategory" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateQuestDefinitionAsync(1, dto));
            Assert.Contains("Must be one of", exception.Message);
        }

        #endregion

        #region DeleteQuestDefinitionAsync Tests

        [Fact]
        public async Task DeleteQuestDefinitionAsync_NoActiveProgress_DeletesSuccessfully()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, QuestId = "test_quest" });

            // Act
            await _service.DeleteQuestDefinitionAsync(1);

            // Assert
            Assert.Equal(0, _questRepo.Count);
        }

        [Fact]
        public async Task DeleteQuestDefinitionAsync_HasActiveProgress_ThrowsException()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, QuestId = "test_quest" });
            _progressRepo.AddProgress(new WeeklyQuestProgress { QuestId = "test_quest", IsClaimed = false });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteQuestDefinitionAsync(1));
            Assert.Contains("active progress records", exception.Message);
        }

        [Fact]
        public async Task DeleteQuestDefinitionAsync_QuestNotFound_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteQuestDefinitionAsync(999));
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region ToggleQuestActiveAsync Tests

        [Fact]
        public async Task ToggleQuestActiveAsync_TogglesToInactive()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, IsActive = true });

            // Act
            var result = await _service.ToggleQuestActiveAsync(1, false);

            // Assert
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task ToggleQuestActiveAsync_TogglesToActive()
        {
            // Arrange
            _questRepo.Add(new QuestDefinition { Id = 1, IsActive = false });

            // Act
            var result = await _service.ToggleQuestActiveAsync(1, true);

            // Assert
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task ToggleQuestActiveAsync_QuestNotFound_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ToggleQuestActiveAsync(999, true));
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region ResetWeeklyQuestsAsync Tests

        [Fact]
        public async Task ResetWeeklyQuestsAsync_ResetsAllProgress()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var weekNumber = GetIsoWeekNumber(now);
            var year = now.Year;

            _progressRepo.AddProgress(new WeeklyQuestProgress
            {
                QuestId = "quest1",
                IsClaimed = true,
                Progress = 5,
                WeekNumber = weekNumber,
                Year = year
            });
            _progressRepo.AddProgress(new WeeklyQuestProgress
            {
                QuestId = "quest2",
                IsClaimed = false,
                Progress = 3,
                WeekNumber = weekNumber,
                Year = year
            });

            // Act
            var result = await _service.ResetWeeklyQuestsAsync();

            // Assert
            Assert.Equal(2, result);
            // Verify progress was reset
            foreach (var progress in _progressRepo.ProgressRecords)
            {
                Assert.Equal(0, progress.Progress);
                Assert.False(progress.IsClaimed);
            }
        }

        [Fact]
        public async Task ResetWeeklyQuestsAsync_NoProgress_ReturnsZero()
        {
            // Act
            var result = await _service.ResetWeeklyQuestsAsync();

            // Assert
            Assert.Equal(0, result);
        }

        #endregion

        private static int GetIsoWeekNumber(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            var mondayOfWeek = date.AddDays(-(dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1));

            var jan1 = new DateTime(date.Year, 1, 1);
            var jan1DayOfWeek = jan1.DayOfWeek;
            var jan1Monday = jan1.AddDays(-(jan1DayOfWeek == DayOfWeek.Sunday ? 6 : (int)jan1DayOfWeek - 1));

            var daysSinceJan1Monday = (mondayOfWeek - jan1Monday).Days;
            var weekNumber = daysSinceJan1Monday / 7 + 1;

            return weekNumber < 1 ? 1 : weekNumber;
        }

        /// <summary>
        /// Fake repository for QuestDefinition.
        /// </summary>
        private class FakeQuestDefinitionRepository : IQuestDefinitionRepository
        {
            private readonly List<QuestDefinition> _quests = new();
            private int _nextId = 1;

            public IReadOnlyList<QuestDefinition> All => _quests.AsReadOnly();
            public int Count => _quests.Count;

            public void Add(QuestDefinition quest)
            {
                quest.Id = _nextId++;
                _quests.Add(quest);
            }

            public Task<IQueryable<QuestDefinition>> GetAllAsync(bool? isActive = null)
            {
                var query = _quests.AsQueryable();
                if (isActive.HasValue)
                {
                    query = query.Where(q => q.IsActive == isActive.Value);
                }
                return Task.FromResult(query);
            }

            public Task<QuestDefinition?> GetByQuestIdAsync(string questId)
            {
                return Task.FromResult(_quests.FirstOrDefault(q => q.QuestId == questId));
            }

            public Task<QuestDefinition?> GetByIdAsync(int id)
            {
                return Task.FromResult(_quests.FirstOrDefault(q => q.Id == id));
            }

            public Task CreateAsync(QuestDefinition quest)
            {
                quest.Id = _nextId++;
                _quests.Add(quest);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(QuestDefinition quest)
            {
                var existing = _quests.FirstOrDefault(q => q.Id == quest.Id);
                if (existing != null)
                {
                    existing.QuestId = quest.QuestId;
                    existing.Name = quest.Name;
                    existing.Description = quest.Description;
                    existing.Category = quest.Category;
                    existing.IsOneTime = quest.IsOneTime;
                    existing.Target = quest.Target;
                    existing.PointsReward = quest.PointsReward;
                    existing.IsActive = quest.IsActive;
                    existing.Order = quest.Order;
                    existing.UpdatedAt = quest.UpdatedAt;
                }
                return Task.CompletedTask;
            }

            public Task DeleteAsync(int id)
            {
                var quest = _quests.FirstOrDefault(q => q.Id == id);
                if (quest != null)
                {
                    _quests.Remove(quest);
                }
                return Task.CompletedTask;
            }

            public Task SaveChangesAsync()
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Fake repository for WeeklyQuestProgress.
        /// </summary>
        private class FakeWeeklyQuestProgressRepository : IWeeklyQuestProgressRepository
        {
            private readonly List<WeeklyQuestProgress> _progressRecords = new();

            public IReadOnlyList<WeeklyQuestProgress> ProgressRecords => _progressRecords.AsReadOnly();

            public void AddProgress(WeeklyQuestProgress progress)
            {
                _progressRecords.Add(progress);
            }

            public Task<IQueryable<WeeklyQuestProgress>> GetAllAsync(int userId, int? weekNumber = null, int? year = null)
            {
                var query = _progressRecords.AsQueryable();
                if (weekNumber.HasValue)
                {
                    query = query.Where(p => p.WeekNumber == weekNumber.Value);
                }
                if (year.HasValue)
                {
                    query = query.Where(p => p.Year == year.Value);
                }
                return Task.FromResult(query);
            }

            public Task<WeeklyQuestProgress?> GetAsync(int userId, string questId, int weekNumber, int year)
            {
                return Task.FromResult(_progressRecords.FirstOrDefault(p =>
                    p.UserId == userId && p.QuestId == questId && p.WeekNumber == weekNumber && p.Year == year));
            }

            public Task<IQueryable<WeeklyQuestProgress>> GetActiveAsync(int userId)
            {
                return Task.FromResult(_progressRecords
                    .Where(p => p.UserId == userId && !p.IsClaimed)
                    .AsQueryable());
            }

            public Task UpsertAsync(WeeklyQuestProgress progress)
            {
                var existing = _progressRecords.FirstOrDefault(p =>
                    p.UserId == progress.UserId && p.QuestId == progress.QuestId &&
                    p.WeekNumber == progress.WeekNumber && p.Year == progress.Year);
                if (existing != null)
                {
                    existing.Progress = progress.Progress;
                    existing.IsClaimed = progress.IsClaimed;
                }
                else
                {
                    _progressRecords.Add(progress);
                }
                return Task.CompletedTask;
            }

            public Task ResetWeekAsync(int userId, int weekNumber, int year)
            {
                foreach (var record in _progressRecords.Where(p =>
                    p.UserId == userId && p.WeekNumber == weekNumber && p.Year == year))
                {
                    record.Progress = 0;
                    record.IsCompleted = false;
                    record.PointsAwarded = 0;
                    record.IsClaimed = false;
                }
                return Task.CompletedTask;
            }

            public Task<IQueryable<WeeklyQuestProgress>> GetAllLifetimeAsync(int userId)
            {
                return Task.FromResult(_progressRecords
                    .Where(p => p.UserId == userId && p.WeekNumber == 0 && p.Year == 0)
                    .AsQueryable());
            }

            public Task SaveChangesAsync()
            {
                return Task.CompletedTask;
            }

            public Task<int> GetActiveProgressCountByQuestIdAsync(string questId)
            {
                return Task.FromResult(_progressRecords.Count(p => p.QuestId == questId && !p.IsClaimed));
            }

            public Task<int> ResetAllWeeksAsync(int weekNumber, int year)
            {
                var records = _progressRecords.Where(p => p.WeekNumber == weekNumber && p.Year == year).ToList();
                foreach (var record in records)
                {
                    record.Progress = 0;
                    record.IsCompleted = false;
                    record.PointsAwarded = 0;
                    record.IsClaimed = false;
                }
                return Task.FromResult(records.Count);
            }
        }
    }
}
