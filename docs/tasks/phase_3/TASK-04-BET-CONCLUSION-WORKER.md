# Task 4: Bet Conclusion & Background Worker

## Objective
Automatically evaluate pending bets when a race finishes, update user balances with winnings, and send notifications.

## Scope
- **Backend**:
  - Create a `RaceStatusMonitorJob` that checks for newly finished races.
  - Implement/re-enable `BettingService.ProcessRaceResultsAsync`:
    - Fetch race results from database.
    - Evaluate all pending bets for the race.
    - Calculate winnings and update the status of each bet (`Won`/`Lost`).
    - Atomically credit user point balances.
    - Mark the race as `ResultsProcessed`.
  - Integrate the `NotificationService` to push or save a notification for the user about the bet outcome.

- **Frontend**:
  - Create a notification pop-up about user's bet resolvement.

## Testing (In Isolation)
- **Backend Tests**:
  - Create an integration/unit test that mocks a finished race and a set of pending bets (some winning, some losing).
  - Trigger `ProcessRaceResultsAsync` and assert that the correct bets are marked as won/lost and that user balances reflect exact expected winnings.
  - Test idempotency: running the processor twice on the same race should not credit points twice.

## Out of Scope (Do Not Modify)
- **Bet Placement Controllers**: Do not modify how users place or cancel bets.
- **OpenF1 Client Models**: Do not alter how the raw API data is initially parsed, only consume the results.

## Reviewability
This PR encapsulates the core business logic of resolving bets. It can be isolated entirely to the backend and unit tests without needing frontend changes.
