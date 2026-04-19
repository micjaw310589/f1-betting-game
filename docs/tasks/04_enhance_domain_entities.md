# Task: Enhance Domain Entities

## Description
Enhance existing domain entities (User, Bet, Race, Driver, Team) with additional properties and methods, and implement validation logic in entity constructors.

## Requirements
- **User Entity**: Add ProfileImageUrl, LastLogin, IsActive, IsAdmin properties and AddPoints(), DeductPoints(), HasSufficientBalance() methods
- **Bet Entity**: Add BetType, Odds, PotentialWinnings properties and CalculatePotentialWinnings(), ValidateBet() methods
- **Race Entity**: Add Circuit, Country, OpenF1RaceId, Season properties and CanPlaceBets(), IsRaceFinished() methods
- **Driver Entity**: Add Number, Country, OpenF1DriverId properties and GetFullName() method
- **Team Entity**: Add Country, OpenF1TeamId, Base properties and GetDrivers() method
- Implement validation logic in all entity constructors
- Add domain-specific business rules

## Acceptance Criteria
- [ ] User entity enhanced with new properties and methods
- [ ] Bet entity enhanced with new properties and methods
- [ ] Race entity enhanced with new properties and methods
- [ ] Driver entity enhanced with new properties and methods
- [ ] Team entity enhanced with new properties and methods
- [ ] All entities have proper validation logic
- [ ] Domain-specific business rules implemented

## Deliverables
- Enhanced User.cs
- Enhanced Bet.cs
- Enhanced Race.cs
- Enhanced Driver.cs
- Enhanced Team.cs

## Testing
- Create unit tests for each entity's validation logic
- Test all new methods for correct behavior
- Verify business rules are properly enforced

## Dependencies
- None

## Notes
- Each entity can be enhanced and tested independently
- Reviewable as separate changes to domain layer