# Task: Create Enums

## Description
Create missing enums for bet types and notification types to support domain logic.

## Requirements
- **BetType Enum**: RaceWinner, PodiumFinish, Top10Finish, FastestLap, FastestPitStop, DNFCount, DriverVsDriver, TeamVsTeam
- **NotificationType Enum**: BetPlaced, BetWon, BetLost, RaceResultProcessed, SystemMessage
- Add proper XML documentation for each enum value
- Follow existing enum naming conventions

## Acceptance Criteria
- [ ] BetType enum created with all required values
- [ ] NotificationType enum created with all required values
- [ ] Proper XML documentation added
- [ ] Enums follow consistent naming conventions

## Deliverables
- BetType.cs
- NotificationType.cs

## Testing
- Verify enums compile without errors
- Check enum values are used correctly in domain logic
- Validate XML documentation is complete

## Dependencies
- None

## Notes
- Each enum can be created and tested independently
- Reviewable as separate additions to domain layer