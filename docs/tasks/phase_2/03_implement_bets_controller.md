# Task 03: Implement BetsController

## Overview
Implement the BetsController with complete betting functionality including placing bets, canceling bets, and retrieving bet history.

## Objectives
- Provide full betting functionality through REST API
- Enable users to place various types of bets
- Allow bet cancellation before race start
- Provide bet history and status tracking
- Implement proper validation and error handling

## Scope
### In Scope
- BetsController implementation
- Bet placement endpoints
- Bet cancellation functionality
- Bet history retrieval
- Bet validation and status management
- Integration with RaceService for race data

### Out of Scope
- Frontend integration for betting UI
- Real-time betting updates
- Advanced odds calculation algorithms
- Payment processing integration

## Implementation Steps

### 1. Complete BetsController Implementation
- [ ] Implement `POST /api/bets` endpoint for placing bets
- [ ] Implement `GET /api/bets` endpoint for getting user bet history
- [ ] Implement `GET /api/bets/{id}` endpoint for getting specific bet
- [ ] Implement `DELETE /api/bets/{id}` endpoint for canceling bets
- [ ] Add proper request validation
- [ ] Implement Swagger documentation
- [ ] Add consistent error handling

### 2. Enhance BettingService
- [ ] Implement `PlaceBetAsync(PlaceBetDto dto, string userId)` method
- [ ] Implement `CancelBetAsync(int betId, string userId)` method
- [ ] Implement `GetUserBetsAsync(string userId)` method
- [ ] Implement `GetBetByIdAsync(int betId, string userId)` method
- [ ] Add bet validation logic (race status, user balance, etc.)
- [ ] Implement bet status updates

### 3. Add Betting DTOs
- [ ] Create `PlaceBetDto` with validation
- [ ] Create `BetResponseDto` for bet responses
- [ ] Create `BetHistoryDto` for bet history
- [ ] Add proper data annotations
- [ ] Ensure DTOs match domain bet types

### 4. Implement Bet Validation
- [ ] Add validation for race existence and status
- [ ] Implement user balance validation
- [ ] Add bet type validation
- [ ] Implement bet timing validation (before race start)
- [ ] Add validation for bet amounts

### 5. Integrate with RaceService
- [ ] Add race data validation to betting service
- [ ] Implement race status checks
- [ ] Add race result processing for bet resolution
- [ ] Ensure proper error handling for race data issues

## Testing
- [ ] Test bet placement with valid data
- [ ] Test bet placement with invalid data
- [ ] Test bet cancellation before race start
- [ ] Test bet cancellation after race start (should fail)
- [ ] Test bet history retrieval
- [ ] Test bet validation logic
- [ ] Test integration with RaceService

## Deliverables
- Fully implemented `BetsController.cs`
- Enhanced `BettingService.cs` with all methods
- Betting DTOs in `DTOs/` directory
- Comprehensive test coverage for betting endpoints
- Updated integration with RaceService

## Success Criteria
- All betting endpoints functional
- Bet placement and cancellation working
- Bet history retrieval implemented
- Proper validation for all bet operations
- Integration with RaceService complete
- All endpoints properly documented
- Comprehensive error handling in place

## Review Checklist
- [ ] All betting endpoints implemented and tested
- [ ] Bet validation covers all requirements
- [ ] Integration with RaceService working
- [ ] Error handling covers all edge cases
- [ ] Swagger documentation complete
- [ ] Test coverage meets requirements
- [ ] Bet status management implemented