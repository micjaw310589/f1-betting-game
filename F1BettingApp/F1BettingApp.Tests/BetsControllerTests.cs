// using F1BettingApp.Application.DTOs;
// using F1BettingApp.Application.Interfaces;
// using F1BettingApp.Domain.Enums;
// using F1BettingApp.Domain.Exceptions;
// using Moq;
// using F1BettingApp.API.Controllers;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text.Json;

// namespace F1BettingApp.Tests.BetsControllerTests;

// public class PlaceBetControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 UserId = 123,
//                 RaceId = 1,
//                 DriverId = 5,
//                 Amount = 100m,
//                 BetType = BetType.RaceWinner,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var invalidDto = new PlaceBetDto();

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, invalidDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetWithInsufficientFunds_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 200m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ThrowsAsync(new InsufficientFundsException("Insufficient funds"));

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetWithFinishedRace_ShouldReturnNotFound()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ThrowsAsync(new RaceCompletedException());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetWithZeroAmount_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 0m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class CancelBetControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task CancelBetWithValidId_ShouldReturnOk()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.CancelBetAsync(100, 123))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 Status = BetStatus.Canceled
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 100);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task CancelBetWithNonExistentId_ShouldReturnNotFound()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.CancelBetAsync(It.IsAny<int>(), It.IsAny<int>()))
//             .ThrowsAsync(new BetNotFoundException(999));

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 999);

//         Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
//     }

//     [Fact]
//     public async Task CancelBetWithRaceAlreadyStarted_ShouldReturnUnprocessableEntity()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.CancelBetAsync(It.IsAny<int>(), It.IsAny<int>()))
//             .ThrowsAsync(new RaceAlreadyStartedException());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 100);

//         Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
//     }

//     [Fact]
//     public async Task CancelBetWithUnauthorizedAttempt_ShouldReturnBadRequest()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.CancelBetAsync(It.IsAny<int>(), It.IsAny<int>()))
//             .ThrowsAsync(new UnauthorizedAccessException("You can only cancel your own bets"));

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 100);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class GetBetsControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task GetBetsWithValidUserId_ShouldReturnOk()
//     {
//         // Arrange
//         var mockBets = new List<BetResponseDto>
//         {
//             new BetResponseDto
//             {
//                 Id = 100,
//                 UserId = 123,
//                 RaceId = 1,
//                 DriverId = 5,
//                 Amount = 100m,
//                 BetType = BetType.RaceWinner,
//                 Status = BetStatus.Won,
//                 Winnings = 200m
//             }
//         };

//         _mockBettingService.Setup(b => b.GetUserBetsAsync(123))
//             .ReturnsAsync(mockBets);

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task GetBetsWithEmptyHistory_ShouldReturnOk()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.GetUserBetsAsync(It.IsAny<int>()))
//             .ReturnsAsync(new List<BetResponseDto>().AsEnumerable());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task GetBetByIdWithValidId_ShouldReturnOk()
//     {
//         // Arrange
//         var mockBet = new BetResponseDto
//         {
//             Id = 100,
//             UserId = 123,
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 100m,
//             BetType = BetType.RaceWinner,
//             Status = BetStatus.Pending
//         };

//         _mockBettingService.Setup(b => b.GetBetByIdAsync(100, 123))
//             .ReturnsAsync(mockBet);

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 100);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task GetBetByIdWithNonExistentId_ShouldReturnNotFound()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.GetBetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
//             .ReturnsAsync((BetResponseDto?)null);

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 999);

//         Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
//     }

//     [Fact]
//     public async Task GetBetByIdWithError_ShouldReturnInternalServerError()
//     {
//         // Arrange
//         _mockBettingService.Setup(b => b.GetBetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
//             .ThrowsAsync(new InvalidOperationException("Database error"));

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, 100);

//         Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
//     }
// }

// public class ValidateBetControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task ValidateBetWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.ValidateBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetValidationResult
//             {
//                 IsValid = true,
//                 Errors = new List<string>(),
//                 Odds = 5.5m,
//                 PotentialWinnings = 200m
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task ValidateBetWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.ValidateBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetValidationResult
//             {
//                 IsValid = false,
//                 Errors = new List<string> { "Race not found", "Insufficient funds" }
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnPositionControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnPositionWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             PlacePosition = 3,
//             Amount = 100m,
//             BetType = BetType.PodiumFinish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 PlacePosition = 3,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnPositionWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             PlacePosition = -1,
//             Amount = 100m,
//             BetType = BetType.PodiumFinish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnPositionWithMaximumAmount_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             PlacePosition = 2,
//             Amount = 500m,
//             BetType = BetType.PodiumFinish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnPositionWithAmountExceedingMaximum_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             PlacePosition = 2,
//             Amount = 1000m,
//             BetType = BetType.PodiumFinish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnQualifyingControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnQualifyingWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             QualifyingPosition = 3,
//             Amount = 100m,
//             BetType = BetType.FastestLap
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 QualifyingPosition = 3,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnQualifyingWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             QualifyingPosition = -1,
//             Amount = 100m,
//             BetType = BetType.FastestLap
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnQualifyingWithInvalidBetType_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             QualifyingPosition = 3,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnDriverStatisticsControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnDriverStatisticsWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             StatisticsPosition = 3,
//             Amount = 100m,
//             BetType = BetType.Top10Finish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 StatisticsPosition = 3,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnDriverStatisticsWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             StatisticsPosition = -1,
//             Amount = 100m,
//             BetType = BetType.Top10Finish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnDriverStatisticsWithInvalidBetType_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             StatisticsPosition = 3,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnDriverOddsControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnDriverOddsWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             OddsPosition = 3,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 OddsPosition = 3,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnDriverOddsWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = 5,
//             OddsPosition = -1,
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnDriverNationalityControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnDriverNationalityWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             Nationality = "GB",
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 Nationality = "GB",
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnDriverNationalityWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             Nationality = "",
//             Amount = 100m,
//             BetType = BetType.RaceWinner
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnDriverNationalityWithInvalidBetType_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             Nationality = "GB",
//             Amount = 100m,
//             BetType = BetType.FastestLap
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }

// public class PlaceBetOnConstructorPointsControllerTests
// {
//     private Mock<IBettingService> _mockBettingService = new();

//     [Fact]
//     public async Task PlaceBetOnConstructorPointsWithValidData_ShouldReturnOk()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             ConstructorPosition = 3,
//             Amount = 100m,
//             BetType = BetType.Top10Finish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(123, placeBetDto))
//             .ReturnsAsync(new BetResponseDto
//             {
//                 Id = 100,
//                 ConstructorPosition = 3,
//                 Status = BetStatus.Pending
//             });

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnConstructorPointsWithInvalidData_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             ConstructorPosition = -1,
//             Amount = 100m,
//             BetType = BetType.Top10Finish
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }

//     [Fact]
//     public async Task PlaceBetOnConstructorPointsWithInvalidBetType_ShouldReturnBadRequest()
//     {
//         // Arrange
//         var placeBetDto = new PlaceBetDto
//         {
//             RaceId = 1,
//             DriverId = null,
//             ConstructorPosition = 3,
//             Amount = 100m,
//             BetType = BetType.FastestLap
//         };

//         _mockBettingService.Setup(b => b.PlaceBetAsync(It.IsAny<int>(), It.IsAny<PlaceBetDto>()))
//             .ReturnsAsync(new BetResponseDto());

//         // Act & Assert
//         var controller = new BetsController(_mockBettingService.Object);
//         var result = await ControllerTestHelpers.GetActionResult(controller, placeBetDto);

//         Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//     }
// }