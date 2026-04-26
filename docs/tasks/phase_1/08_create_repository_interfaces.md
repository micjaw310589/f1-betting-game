# Task: Create Repository Interfaces

## Description
Create specific repository interfaces extending IRepository with domain-specific query methods.

## Requirements
- **IUserRepository**: GetByEmailAsync, GetByUsernameAsync, GetLeaderboardAsync
- **IBetRepository**: GetUserBetsAsync, GetPendingBetsForRaceAsync, GetBetStatisticsAsync
- **IRaceRepository**: GetUpcomingRacesAsync, GetRaceWithResultsAsync, GetCurrentSeasonRacesAsync
- **IResultRepository**: GetRaceResultsAsync, GetDriverResultsAsync
- **INotificationRepository**: GetUnreadNotificationsAsync, MarkAsReadAsync
- Extend existing IRepository interface
- Define proper method signatures with async support

## Acceptance Criteria
- [ ] IUserRepository interface created with required methods
- [ ] IBetRepository interface created with required methods
- [ ] IRaceRepository interface created with required methods
- [ ] IResultRepository interface created with required methods
- [ ] INotificationRepository interface created with required methods
- [ ] All interfaces extend IRepository
- [ ] Proper async method signatures

## Deliverables
- IUserRepository.cs
- IBetRepository.cs
- IRaceRepository.cs
- IResultRepository.cs
- INotificationRepository.cs

## Testing
- Verify interfaces compile without errors
- Check method signatures match requirements
- Validate interfaces follow repository pattern

## Dependencies
- Existing IRepository interface

## Notes
- Each repository interface can be created and tested independently
- Reviewable as separate additions to data access layer