# Task 2: OpenF1 API Data Synchronization

## Objective
Implement periodic background jobs to fetch and synchronize race data from the OpenF1 API to the local database.

## Scope
- **Backend**:
  - Utilize the existing `OpenF1Client` and `IOpenF1ApiClient`.
  - Create background services (e.g., using `IHostedService` or Hangfire):
    - `RaceCalendarSyncJob`: Polls daily for schedule updates.
    - `StandingsSyncJob`: Polls for championship standings.
    - `DriverTeamSyncJob`: Polls weekly for driver and team details.
  - Implement the dynamic interval logic (1h during "race week", 12h otherwise) for race details syncing.
  - Map OpenF1 data to local Entity Framework Core models (`Race`, `Driver`, `Team`) and save to DB.
  - Ensure sync gracefully handles API rate limits (retries/backoff) and does not overwrite manually overridden data (admin lock flag).

## Testing (In Isolation)
- **Backend Tests**:
  - Mock `IOpenF1ApiClient` and verify the background jobs correctly map and save entities to the local database.
  - Test the dynamic interval calculation logic (simulate "race week" vs "non-race week").
  - Test idempotency (syncing the same data twice shouldn't create duplicates).

## Out of Scope (Do Not Modify)
- **Betting Logic**: Do not modify any code inside `BettingService` or bet-related controllers.
- **User Management/Auth**: Do not touch authentication middleware, user models, or token logic.
- **Frontend**: Do not modify any Angular code; this is purely a backend data ingestion task.

## Reviewability
This is reviewable as a standalone PR. It only impacts the data ingestion layer and can be run independently of any user-facing features or betting logic.
