# Task 7: Admin Panel - System & Bet Management

## Objective
Provide administrators with controls to manage race data synchronization and override race results when necessary.

## Scope
- **Backend**:
  - Add endpoints in an `AdminController` (or secure `RacesController` endpoints) to manually trigger OpenF1 sync jobs.
  - Add an endpoint to allow manual input or override of a race result.
  - Ensure that if a result is manually overridden, a flag (`IsManuallyOverridden`) is set on the race record so that subsequent automatic OpenF1 syncs do not revert the admin's changes.
- **Frontend**:
  - Create the `AdminSystemManagementComponent`.
  - Implement buttons/actions to manually trigger syncs (with visual feedback/spinners).
  - Add an interface to edit the results of a specific race manually.

## Testing (In Isolation)
- **Backend Tests**:
  - Test the manual sync trigger to ensure it successfully invokes the background job immediately.
  - Verify that the `IsManuallyOverridden` flag successfully blocks the background worker from overwriting the data during its next cycle.
- **Frontend Tests**:
  - Test the manual override form.
  - Ensure success/error feedback is provided when triggering system tasks.

## Out of Scope (Do Not Modify)
- **User Management**: Do not modify how user points or account statuses are managed.
- **OpenF1 Fetching Logic**: Do not modify the way `OpenF1Client` retrieves and parses the raw data.
- **Public Views**: Do not modify the public-facing race calendar or bet placement views.

## Reviewability
This builds upon the Admin UI from Task 6 and the OpenF1 sync logic from Task 2. It's a well-defined slice for managing system edge cases.
