using Xunit;
using Moq;
using F1BettingApp.API.Controllers;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;

namespace F1BettingApp.Tests;

/// <summary>
/// Tests for the AdminController, focusing on sync trigger, race result override,
/// metadata update, and the IsManuallyOverridden flag behavior.
/// </summary>
public class AdminControllerTests
{
    private readonly Mock<IRaceService> _mockRaceService;
    private readonly Mock<IBettingService> _mockBettingService;
    private readonly Mock<ILogger<AdminController>> _mockLogger;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockRaceService = new Mock<IRaceService>();
        _mockBettingService = new Mock<IBettingService>();
        _mockLogger = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_mockRaceService.Object, _mockBettingService.Object, _mockLogger.Object);
    }

    // Helper to extract ObjectResult from ActionResult<T>
    private static ObjectResult GetObjectResult<T>(ActionResult<T> result) => 
        result.Result is ObjectResult obj ? obj : throw new InvalidOperationException($"Expected ObjectResult but got {result.Result?.GetType().Name ?? "null"}");

    // Helper to extract ObjectResult from ActionResult (non-generic)
    private static ObjectResult GetObjectResult(ActionResult result) => 
        result is ObjectResult obj ? obj : throw new InvalidOperationException($"Expected ObjectResult but got {result.GetType().Name}");

    // ========================================
    // TriggerSync - Success Tests
    // ========================================

    [Fact]
    public async Task TriggerSync_ShouldReturnOkWithSyncResult()
    {
        // Arrange
        var syncResult = new SyncResultDto
        {
            Success = true,
            RacesProcessed = 5,
            RacesCreated = 2,
            RacesUpdated = 3,
            SyncedAt = DateTime.UtcNow
        };

        _mockRaceService
            .Setup(s => s.SyncRaceDataFromOpenF1Async())
            .ReturnsAsync(syncResult);

        // Act
        var result = await _controller.TriggerSync();

        // Assert
        Assert.NotNull(result);
        var objResult = GetObjectResult(result);
        Assert.Equal(200, objResult.StatusCode);
        var returnedResult = Assert.IsType<SyncResultDto>(objResult.Value);
        Assert.True(returnedResult.Success);
        Assert.Equal(5, returnedResult.RacesProcessed);
        Assert.Equal(2, returnedResult.RacesCreated);
        Assert.Equal(3, returnedResult.RacesUpdated);
    }

    [Fact]
    public async Task TriggerSync_EmptySync_ShouldReturnOkWithZeroCounts()
    {
        // Arrange
        var syncResult = new SyncResultDto
        {
            Success = true,
            RacesProcessed = 0,
            RacesCreated = 0,
            RacesUpdated = 0,
            SyncedAt = DateTime.UtcNow
        };

        _mockRaceService
            .Setup(s => s.SyncRaceDataFromOpenF1Async())
            .ReturnsAsync(syncResult);

        // Act
        var response = await _controller.TriggerSync();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        var returnedResult = Assert.IsType<SyncResultDto>(objResult.Value);
        Assert.True(returnedResult.Success);
        Assert.Equal(0, returnedResult.RacesProcessed);
    }

    // ========================================
    // TriggerSync - Error Tests
    // ========================================

    [Fact]
    public async Task TriggerSync_EntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRaceService
            .Setup(s => s.SyncRaceDataFromOpenF1Async())
            .ThrowsAsync(new KeyNotFoundException("Driver not found in sync data"));

        // Act
        var response = await _controller.TriggerSync();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(404, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("SYNC_ENTITY_NOT_FOUND", errorResponse.Error);
        Assert.Contains("not found", errorResponse.Message);
    }

    [Fact]
    public async Task TriggerSync_InvalidOperationException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockRaceService
            .Setup(s => s.SyncRaceDataFromOpenF1Async())
            .ThrowsAsync(new InvalidOperationException("API rate limit exceeded"));

        // Act
        var response = await _controller.TriggerSync();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(500, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("SYNC_FAILED", errorResponse.Error);
    }

    [Fact]
    public async Task TriggerSync_GenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockRaceService
            .Setup(s => s.SyncRaceDataFromOpenF1Async())
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var response = await _controller.TriggerSync();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(500, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("SYNC_ERROR", errorResponse.Error);
    }

    // ========================================
    // GetRaceResults - Success Tests
    // ========================================

    [Fact]
    public async Task GetRaceResults_ShouldReturnRaceResultDto()
    {
        // Arrange
        var raceId = 1;
        var raceResult = new RaceResultDto
        {
            RaceId = raceId,
            RaceName = "Monaco Grand Prix",
            Circuit = "Circuit de Monaco",
            Country = "Monaco",
            RaceDate = new DateTime(2025, 5, 25),
            WinnerDriverName = "Max Verstappen",
            Positions = new List<PositionDto>
            {
                new PositionDto { Position = 1, DriverId = 1, DriverName = "Max Verstappen", TeamName = "Red Bull", Points = 25 },
                new PositionDto { Position = 2, DriverId = 4, DriverName = "Charles Leclerc", TeamName = "Ferrari", Points = 18 },
                new PositionDto { Position = 3, DriverId = 63, DriverName = "George Russell", TeamName = "Mercedes", Points = 15 },
            }
        };

        _mockRaceService
            .Setup(s => s.GetRaceResultDtoAsync(raceId))
            .ReturnsAsync(raceResult);

        // Act
        var response = await _controller.GetRaceResults(raceId);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        var returnedResult = Assert.IsType<RaceResultDto>(objResult.Value);
        Assert.Equal(raceId, returnedResult.RaceId);
        Assert.Equal("Monaco Grand Prix", returnedResult.RaceName);
        Assert.Equal(3, returnedResult.Positions.Count);
        Assert.Equal("Max Verstappen", returnedResult.WinnerDriverName);
    }

    [Fact]
    public async Task GetRaceResults_NotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockRaceService
            .Setup(s => s.GetRaceResultDtoAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Race with ID 999 not found."));

        // Act
        var response = await _controller.GetRaceResults(999);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(404, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("RACE_NOT_FOUND", errorResponse.Error);
    }

    // ========================================
    // OverrideRaceResults - Success Tests
    // ========================================

    [Fact]
    public async Task OverrideRaceResults_ShouldReturnOk()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 },
                new PositionEntryDto { Position = 2, DriverId = 4 },
                new PositionEntryDto { Position = 3, DriverId = 63 },
            },
            FastestLapDriverId = 1
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        Assert.NotNull(objResult.Value);
        _mockRaceService.Verify(s => s.OverrideRaceResultAsync(raceId, dto), Times.Once);
    }

    [Fact]
    public async Task OverrideRaceResults_EmptyPositions_ShouldReturnBadRequest()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>()
        };

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(400, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("INVALID_INPUT", errorResponse.Error);
        Assert.Contains("at least one position", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OverrideRaceResults_RaceNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var raceId = 999;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 }
            }
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .ThrowsAsync(new KeyNotFoundException("Race with ID 999 not found."));

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(404, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("RACE_NOT_FOUND", errorResponse.Error);
    }

    [Fact]
    public async Task OverrideRaceResults_InvalidPosition_ShouldReturnBadRequest()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 0, DriverId = 1 } // Position must be >= 1
            }
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .ThrowsAsync(new ArgumentException("Position must be at least 1."));

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(400, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("INVALID_INPUT", errorResponse.Error);
    }

    [Fact]
    public async Task OverrideRaceResults_DuplicateDriver_ShouldReturnBadRequest()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 },
                new PositionEntryDto { Position = 2, DriverId = 1 }, // Same driver twice
                new PositionEntryDto { Position = 3, DriverId = 63 }
            }
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .ThrowsAsync(new ArgumentException("The following drivers are assigned to multiple positions: 1. Each driver can only occupy one position."));

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(400, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("INVALID_INPUT", errorResponse.Error);
        Assert.Contains("multiple positions", errorResponse.Message);
    }

    [Fact]
    public async Task OverrideRaceResults_InvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 }
            }
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .ThrowsAsync(new InvalidOperationException("Race already finished and no positions provided."));

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(400, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("INVALID_OPERATION", errorResponse.Error);
    }

    // ========================================
    // GetAdminRaces - Success Tests
    // ========================================

    [Fact]
    public async Task GetAdminRaces_ShouldReturnAllRaces()
    {
        // Arrange
        var races = new List<RaceDto>
        {
            new RaceDto { Id = 1, Name = "Bahrain GP", Status = RaceStatus.Finished, Country = "Bahrain" },
            new RaceDto { Id = 2, Name = "Saudi Arabian GP", Status = RaceStatus.Finished, Country = "Saudi Arabia" },
            new RaceDto { Id = 3, Name = "Australian GP", Status = RaceStatus.Scheduled, Country = "Australia" },
        };

        _mockRaceService
            .Setup(s => s.GetAllRacesAsync())
            .ReturnsAsync(races);

        // Act
        var response = await _controller.GetAdminRaces();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        var returnedRaces = Assert.IsAssignableFrom<IEnumerable<RaceDto>>(objResult.Value);
        Assert.Equal(3, returnedRaces.Count());
    }

    [Fact]
    public async Task GetAdminRaces_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRaceService
            .Setup(s => s.GetAllRacesAsync())
            .ReturnsAsync(Enumerable.Empty<RaceDto>());

        // Act
        var response = await _controller.GetAdminRaces();

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        var returnedRaces = Assert.IsAssignableFrom<IEnumerable<RaceDto>>(objResult.Value);
        Assert.Empty(returnedRaces);
    }

    // ========================================
    // UpdateRaceMetadata - Success Tests
    // ========================================

    [Fact]
    public async Task UpdateRaceMetadata_ShouldReturnOk()
    {
        // Arrange
        var raceId = 1;
        var dto = new UpdateRaceMetadataDto
        {
            Name = "Modified Race Name",
            Circuit = "Modified Circuit",
            Country = "Modified Country",
            Status = RaceStatus.Finished
        };

        _mockRaceService
            .Setup(s => s.UpdateRaceMetadataAsync(raceId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _controller.UpdateRaceMetadata(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        Assert.NotNull(objResult.Value);
    }

    [Fact]
    public async Task UpdateRaceMetadata_RaceNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var raceId = 999;
        var dto = new UpdateRaceMetadataDto { Name = "New Name" };

        _mockRaceService
            .Setup(s => s.UpdateRaceMetadataAsync(raceId, dto))
            .ThrowsAsync(new KeyNotFoundException("Race with ID 999 not found."));

        // Act
        var response = await _controller.UpdateRaceMetadata(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(404, objResult.StatusCode);
        var errorResponse = Assert.IsType<ErrorResponse>(objResult.Value);
        Assert.Equal("RACE_NOT_FOUND", errorResponse.Error);
    }

    // ========================================
    // IsManuallyOverridden Flag Tests
    // ========================================

    [Fact]
    public async Task OverrideRaceResults_SetsIsManuallyOverriddenFlag()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 }
            }
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        Assert.NotNull(objResult.Value);
        _mockRaceService.Verify(s => s.OverrideRaceResultAsync(raceId, dto), Times.Once);
    }

    [Fact]
    public async Task UpdateRaceMetadata_SetsIsManuallyOverriddenFlag()
    {
        // Arrange
        var raceId = 1;
        var dto = new UpdateRaceMetadataDto { Name = "Updated Name" };

        _mockRaceService
            .Setup(s => s.UpdateRaceMetadataAsync(raceId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _controller.UpdateRaceMetadata(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        Assert.NotNull(objResult.Value);
        _mockRaceService.Verify(s => s.UpdateRaceMetadataAsync(raceId, dto), Times.Once);
    }

    // ========================================
    // OverrideRaceResults - Multiple Positions
    // ========================================

    [Fact]
    public async Task OverrideRaceResults_MultiplePositions_ShouldSucceed()
    {
        // Arrange
        var raceId = 1;
        var dto = new OverrideRaceResultDto
        {
            Positions = new List<PositionEntryDto>
            {
                new PositionEntryDto { Position = 1, DriverId = 1 },
                new PositionEntryDto { Position = 2, DriverId = 2 },
                new PositionEntryDto { Position = 3, DriverId = 3 },
                new PositionEntryDto { Position = 4, DriverId = 4 },
                new PositionEntryDto { Position = 5, DriverId = 5 },
            },
            FastestLapDriverId = 1
        };

        _mockRaceService
            .Setup(s => s.OverrideRaceResultAsync(raceId, dto))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _controller.OverrideRaceResults(raceId, dto);

        // Assert
        var objResult = GetObjectResult(response);
        Assert.Equal(200, objResult.StatusCode);
        Assert.NotNull(objResult.Value);
        _mockRaceService.Verify(s => s.OverrideRaceResultAsync(raceId, dto), Times.Once);
    }
}
