# Task: Extend Service Interfaces

## Description
Extend existing service interfaces and create new service interfaces for additional functionality in the business logic layer.

## Requirements
- **IBettingService** (Extend): PlaceBetAsync, CancelBetAsync, ProcessRaceResultsAsync, CalculateWinningsAsync
- **IRaceService** (Extend): SyncRaceDataFromOpenF1Async, GetUpcomingRacesWithOddsAsync, UpdateRaceStatusAsync
- **IUserService** (Extend): GetUserLeaderboardPositionAsync, GetUserStatisticsAsync, UpdateUserPointsAsync
- **INotificationService** (New): CreateNotificationAsync, MarkNotificationAsReadAsync, GetUnreadNotificationsAsync
- **ILeaderboardService** (New): UpdateLeaderboardAsync, GetCurrentLeaderboardAsync, GetSeasonLeaderboardAsync
- Add proper method signatures with async support
- Include XML documentation

## Acceptance Criteria
- [ ] IBettingService extended with new methods
- [ ] IRaceService extended with new methods
- [ ] IUserService extended with new methods
- [ ] INotificationService created with all methods
- [ ] ILeaderboardService created with all methods
- [ ] All methods have proper async signatures
- [ ] XML documentation added

## Deliverables
- Enhanced IBettingService.cs
- Enhanced IRaceService.cs
- Enhanced IUserService.cs
- INotificationService.cs
- ILeaderboardService.cs

## Testing
- Verify interfaces compile without errors
- Check method signatures match requirements
- Validate interfaces follow service layer patterns

## Dependencies
- Existing service interfaces

## Notes
- Each service interface can be extended/created and tested independently
- Reviewable as separate changes to business logic layer