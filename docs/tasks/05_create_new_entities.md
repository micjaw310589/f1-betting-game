# Task: Create New Domain Entities

## Description
Create new domain entities: Result, LeaderboardHistory, and Notification with their properties and methods.

## Requirements
- **Result Entity**: Properties (Id, RaceId, DriverId, Position, Points, FastestLap, PitStopTime), Methods (IsPodiumFinish(), IsPointsFinish())
- **LeaderboardHistory Entity**: Properties (Id, UserId, RaceId, Season, TotalPoints, Rank, CreatedAt), No methods required
- **Notification Entity**: Properties (Id, UserId, Title, Message, IsRead, CreatedAt), Method (MarkAsRead())
- Implement validation logic in entity constructors
- Add proper data annotations and relationships

## Acceptance Criteria
- [ ] Result entity created with all properties and methods
- [ ] LeaderboardHistory entity created with all properties
- [ ] Notification entity created with all properties and methods
- [ ] All entities have proper validation logic
- [ ] Entity relationships are correctly defined

## Deliverables
- Result.cs
- LeaderboardHistory.cs
- Notification.cs

## Testing
- Create unit tests for each entity's validation logic
- Test all methods for correct behavior
- Verify entity relationships work as expected

## Dependencies
- None

## Notes
- Each entity can be created and tested independently
- Reviewable as separate additions to domain layer