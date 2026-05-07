# Task 5: User Profile & Bet History

## Objective
Provide users with a dashboard to view their account settings, balance, and historical bet outcomes.

## Scope
- **Backend**:
  - Implement `GET /api/users/profile` in `UsersController`.
  - Implement `GET /api/users/bets` to return a paginated list of the user's bet history.
  - Utilize `BettingService.GetUserBetHistoryAsync`.
- **Frontend**:
  - Create the `UserProfileComponent`.
  - Display the user's current virtual points balance.
  - Implement a paginated data table or list showing active and concluded bets (with win/loss status and amounts).
  - Add basic account settings (e.g., updating username/avatar if supported).

## Testing (In Isolation)
- **Backend Tests**:
  - Verify that users can only fetch their own bet history (AuthZ tests).
  - Verify pagination logic for bet history.
- **Frontend Tests**:
  - Test UI rendering of different bet statuses (color-coding wins vs losses).
  - Test pagination controls.

## Out of Scope (Do Not Modify)
- **Core Betting Logic**: Do not modify the placement, cancellation, or resolution of bets.
- **Auth/Registration Flow**: Do not modify the login/signup process.
- **Race Details Component**: Do not touch the public race viewing sections of the frontend application.

## Reviewability
This is a standard feature-slice PR. It relies on the data structures from previous tasks but operates independently as a read-only presentation layer.
