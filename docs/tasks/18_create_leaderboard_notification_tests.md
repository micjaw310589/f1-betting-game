# Task: Create Leaderboard and Notification Service Tests

## Description
Create comprehensive unit tests for LeaderboardService and NotificationService covering all major functionality and edge cases.

## Requirements
- **LeaderboardService Tests**: UpdateLeaderboard_AfterRace_UpdatesRankings, GetCurrentLeaderboard_ReturnsTopPlayers, GetSeasonLeaderboard_ReturnsSeasonData, UpdateLeaderboard_WithTie_HandlesTieCorrectly
- **NotificationService Tests**: CreateNotification_WithValidData_Succeeds, MarkNotificationAsRead_UpdatesStatus, GetUnreadNotifications_ReturnsOnlyUnread, CreateNotification_ForMultipleUsers_Succeeds
- Test leaderboard calculation scenarios
- Test notification creation and management
- Test tie handling in leaderboards
- Use mock repositories

## Acceptance Criteria
- [ ] All LeaderboardService test cases implemented
- [ ] All NotificationService test cases implemented
- [ ] Leaderboard calculation properly tested
- [ ] Notification scenarios covered
- [ ] Tie handling validated
- [ ] Tests use proper mocking and assertions

## Deliverables
- LeaderboardServiceTests.cs with comprehensive test coverage
- NotificationServiceTests.cs with comprehensive test coverage

## Testing
- Verify all tests pass
- Check leaderboard calculation logic
- Validate notification management
- Ensure tie handling works correctly

## Dependencies
- LeaderboardService and NotificationService implementations
- Test infrastructure
- Test data builders

## Notes
- Each service test suite can be implemented and tested independently
- Reviewable as separate test suites for each service