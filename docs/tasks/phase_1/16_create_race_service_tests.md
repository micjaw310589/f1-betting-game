# Task: Create RaceService Tests

## Description
Create comprehensive unit tests for RaceService covering all major functionality and edge cases.

## Requirements
- Test cases: SyncRaceData_FromOpenF1_Succeeds, SyncRaceData_WithApiFailure_UsesCache, UpdateRaceStatus_ToFinished_TriggersProcessing, GetUpcomingRaces_ReturnsOnlyFutureRaces, GetRaceWithResults_ReturnsCompleteData
- Test OpenF1 API integration scenarios
- Test race status transitions
- Use mock repositories and API clients

## Acceptance Criteria
- [ ] All specified test cases implemented
- [ ] OpenF1 API integration properly tested
- [ ] Race status transitions covered
- [ ] Error handling scenarios tested
- [ ] Tests use proper mocking and assertions

## Deliverables
- RaceServiceTests.cs with comprehensive test coverage

## Testing
- Verify all tests pass
- Check API integration mocking works correctly
- Validate race status logic
- Ensure error handling is proper

## Dependencies
- RaceService implementation
- Test infrastructure
- Test data builders

## Notes
- Can be implemented and tested independently
- Reviewable as single test suite for RaceService