# Task: Create Value Objects

## Description
Create value objects for domain concepts: Money, RaceDate, and Odds with validation logic.

## Requirements
- **Money Value Object**: For points/balance management with validation and arithmetic operations
- **RaceDate Value Object**: With validation logic for race dates
- **Odds Value Object**: For bet calculations with validation
- Implement proper equality comparison
- Add validation logic in constructors
- Make objects immutable

## Acceptance Criteria
- [ ] Money value object created with validation and operations
- [ ] RaceDate value object created with validation
- [ ] Odds value object created with validation
- [ ] All value objects are immutable
- [ ] Proper equality comparison implemented

## Deliverables
- Money.cs
- RaceDate.cs
- Odds.cs

## Testing
- Create unit tests for each value object's validation logic
- Test arithmetic operations for Money
- Verify immutability is maintained
- Test equality comparison

## Dependencies
- None

## Notes
- Each value object can be created and tested independently
- Reviewable as separate additions to domain layer