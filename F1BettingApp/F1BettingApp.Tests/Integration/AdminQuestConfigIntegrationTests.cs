using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using F1BettingApp.Infrastructure.Persistence;
using F1BettingApp.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace F1BettingApp.Tests.Integration;

/// <summary>
/// Integration tests for the admin quest configuration system.
/// </summary>
public class AdminQuestConfigIntegrationTests
{
    private PointsSystemTestFactory CreateFactory() => new();

    [Fact]
    public async Task CreateQuest_ValidDto_CreatesEntry()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();
        var dto = new CreateQuestDto
        {
            QuestId = "test_quest_create",
            Name = "Test Quest",
            Description = "A test quest for integration testing",
            Category = "Betting",
            Target = 5,
            PointsReward = 100,
            IsOneTime = false,
            IsActive = true
        };

        var created = await questService.CreateQuestDefinitionAsync(dto);
        Assert.NotNull(created);
        Assert.Equal("test_quest_create", created.QuestId);
        Assert.Equal("Test Quest", created.Name);
        Assert.Equal(5, created.Target);
        Assert.Equal(100, created.PointsReward);
        Assert.True(created.IsActive);

        var dbQuest = await factory.CreateDbContext().QuestDefinitions
            .FirstOrDefaultAsync(q => q.QuestId == "test_quest_create");
        Assert.NotNull(dbQuest);
        Assert.Equal("Test Quest", dbQuest.Name);
    }

    [Fact]
    public async Task CreateQuest_DuplicateQuestId_ReturnsConflict()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();
        var dto = new CreateQuestDto
        {
            QuestId = "test_quest_dup",
            Name = "Test Quest",
            Description = "First creation",
            Category = "Betting",
            Target = 3,
            PointsReward = 50,
            IsOneTime = false
        };

        await questService.CreateQuestDefinitionAsync(dto);

        var secondDto = new CreateQuestDto
        {
            QuestId = "test_quest_dup",
            Name = "Test Quest 2",
            Description = "Second creation - should fail",
            Category = "Betting",
            Target = 3,
            PointsReward = 50,
            IsOneTime = false
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questService.CreateQuestDefinitionAsync(secondDto));
    }

    [Fact]
    public async Task UpdateQuest_ChangesPointsReward()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_update",
            Name = "Original Quest",
            Description = "Original description",
            Category = "Achievement",
            Target = 10,
            PointsReward = 100,
            IsOneTime = false
        });

        var dbQuest = await factory.CreateDbContext().QuestDefinitions
            .FirstOrDefaultAsync(q => q.QuestId == "test_quest_update");
        Assert.NotNull(dbQuest);
        var questId = dbQuest.Id;

        var updated = await questService.UpdateQuestDefinitionAsync(questId, new UpdateQuestDto
        {
            Name = "Updated Quest",
            Description = "Updated description",
            Category = "Engagement",
            Target = 20,
            PointsReward = 200,
            IsOneTime = true
        });

        Assert.Equal("Updated Quest", updated.Name);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal("Engagement", updated.Category);
        Assert.Equal(20, updated.Target);
        Assert.Equal(200, updated.PointsReward);
        Assert.True(updated.IsOneTime);
    }

    [Fact]
    public async Task ToggleQuestActive_DisablesQuest()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_toggle",
            Name = "Toggle Quest",
            Description = "Test quest for toggling",
            Category = "Betting",
            Target = 5,
            PointsReward = 75,
            IsActive = true
        });

        var dbQuest = await factory.CreateDbContext().QuestDefinitions
            .FirstOrDefaultAsync(q => q.QuestId == "test_quest_toggle");
        Assert.NotNull(dbQuest);
        var questId = dbQuest.Id;

        var toggled = await questService.ToggleQuestActiveAsync(questId, false);
        Assert.False(toggled.IsActive);

        var updatedDbQuest = await factory.CreateDbContext().QuestDefinitions.FindAsync(questId);
        Assert.NotNull(updatedDbQuest);
        Assert.False(updatedDbQuest.IsActive);

        var reactivated = await questService.ToggleQuestActiveAsync(questId, true);
        Assert.True(reactivated.IsActive);
    }

    [Fact]
    public async Task DeleteQuest_RemovesDefinition()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_delete",
            Name = "Delete Me",
            Description = "This quest will be deleted",
            Category = "Betting",
            Target = 5,
            PointsReward = 75
        });

        var dbQuest = await factory.CreateDbContext().QuestDefinitions
            .FirstOrDefaultAsync(q => q.QuestId == "test_quest_delete");
        Assert.NotNull(dbQuest);
        var questId = dbQuest.Id;

        await questService.DeleteQuestDefinitionAsync(questId);

        var existing = await factory.CreateDbContext().QuestDefinitions.FindAsync(questId);
        Assert.Null(existing);
    }

    [Fact]
    public async Task DeleteQuest_WithActiveProgress_ThrowsException()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();
        var user = await factory.CreateTestUserAsync("quest_delete_user");

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_delete_blocked",
            Name = "Blocked Delete",
            Description = "Cannot delete because of active progress",
            Category = "Betting",
            Target = 5,
            PointsReward = 75
        });

        var dbQuest = await factory.CreateDbContext().QuestDefinitions
            .FirstOrDefaultAsync(q => q.QuestId == "test_quest_delete_blocked");
        Assert.NotNull(dbQuest);
        var questId = dbQuest.Id;

        var progress = new WeeklyQuestProgress
        {
            UserId = user.Id,
            QuestId = "test_quest_delete_blocked",
            WeekNumber = 1,
            Year = 2024,
            Progress = 2,
            Target = 5,
            IsCompleted = false,
            PointsAwarded = 0,
            IsClaimed = false,
            UpdatedAt = DateTime.UtcNow
        };
        factory.CreateDbContext().WeeklyQuestProgresses.Add(progress);
        await factory.CreateDbContext().SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questService.DeleteQuestDefinitionAsync(questId));
    }

    [Fact]
    public async Task GetAllQuestDefinitions_ReturnsAllQuests()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_list_a",
            Name = "Quest 1",
            Description = "First quest",
            Category = "Betting",
            Target = 1,
            PointsReward = 50,
            IsActive = true
        });

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "test_quest_list_b",
            Name = "Quest 2",
            Description = "Second quest",
            Category = "Engagement",
            Target = 3,
            PointsReward = 100,
            IsActive = false
        });

        var allQuests = await questService.GetAllQuestDefinitionsAsync();
        Assert.Equal(2, allQuests.Count);

        var activeQuests = await questService.GetAllQuestDefinitionsAsync(isActive: true);
        Assert.Single(activeQuests);
        Assert.Equal("test_quest_list_a", activeQuests[0].QuestId);
    }

    [Fact]
    public async Task CreateQuest_InvalidQuestId_ReturnsConflict()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        await questService.CreateQuestDefinitionAsync(new CreateQuestDto
        {
            QuestId = "valid_quest_id",
            Name = "Valid Quest",
            Description = "Valid ID",
            Category = "Betting",
            Target = 1,
            PointsReward = 50
        });

        var invalidDto = new CreateQuestDto
        {
            QuestId = "Invalid_Quest_Id",
            Name = "Invalid Quest",
            Description = "Invalid ID",
            Category = "Betting",
            Target = 1,
            PointsReward = 50
        };

        await Assert.ThrowsAnyAsync<Exception>(() =>
            questService.CreateQuestDefinitionAsync(invalidDto));
    }

    [Fact]
    public async Task CreateQuest_TargetMustBePositive_ReturnsConflict()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        var dto = new CreateQuestDto
        {
            QuestId = "zero_target_quest",
            Name = "Zero Target",
            Description = "Target must be > 0",
            Category = "Betting",
            Target = 0,
            PointsReward = 50
        };

        await Assert.ThrowsAnyAsync<Exception>(() =>
            questService.CreateQuestDefinitionAsync(dto));
    }

    [Fact]
    public async Task CreateQuest_InvalidCategory_ReturnsConflict()
    {
        using var factory = CreateFactory();
        var questService = factory.CreateQuestDefinitionService();

        var dto = new CreateQuestDto
        {
            QuestId = "invalid_category_quest",
            Name = "Invalid Category",
            Description = "Invalid category",
            Category = "InvalidCategory",
            Target = 5,
            PointsReward = 50
        };

        await Assert.ThrowsAnyAsync<Exception>(() =>
            questService.CreateQuestDefinitionAsync(dto));
    }
}
