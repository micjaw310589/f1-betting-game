# Task 3: Bet Creation & Cancellation

## Objective
Allow authenticated users to place bets on upcoming scheduled races and cancel them before the race starts.

## Scope
- **Backend**:
  - Implement `BetsController` endpoints: `POST /api/bets/place` and `POST /api/bets/{id}/cancel`.
  - Implement/re-enable logic in `BettingService.PlaceBetAsync` and `CancelBetAsync`.
  - Add validation: User must have sufficient balance, Race must be `Scheduled`.
  - Ensure atomic transactions: Creating a bet and deducting points must succeed or fail together. Implement optimistic concurrency if necessary.
- **Frontend**:
  - Create the `BetPlacementComponent` within the race details view.
  - Implement forms for selecting bet type, driver/team, and stake amount.
  - Add UI for users to cancel an existing pending bet.

## Testing (In Isolation)
- **Backend Tests**:
  - Unit tests for `BettingService` confirming points deduction, race status validation, and successful bet creation.
  - Test concurrency by simulating simultaneous bet placements for the same user.
- **Frontend Tests**:
  - Test form validations (preventing negative bet amounts, disabled submit button when balance is too low).
  - Mock backend to test success/error toast notifications on bet placement.

## Out of Scope (Do Not Modify)
- **Bet Resolution Logic**: Do not implement or modify the logic that calculates winnings after a race finishes (`ProcessRaceResultsAsync`).
- **OpenF1 Sync Background Jobs**: Do not modify the way race data is fetched into the system.
- **Auth Core**: Do not modify the JWT token logic or registration flows.

## Reviewability
Can be reviewed as a single PR focused strictly on the bet placement/cancellation lifecycle. Depends on Task 1 (Auth) and Task 2 (Race Data).
