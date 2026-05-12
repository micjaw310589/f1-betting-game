# Task 5: Fix Race Result Admin Override Persistence

**Objective:** Resolve the issue where saving manual race results updates the race status but fails to display or persist the results correctly.

**Scope:**
- **EF Core Batching Fix (`RaceService.cs`):** In `OverrideRaceResultAsync`, execute `await _dbContext.SaveChangesAsync();` immediately after `_dbContext.Results.RemoveRange(existingResults)` to flush deletions before inserting new results. This prevents EF Core from inserting before deleting, which avoids unique constraint violations on `(RaceId, DriverId)`.
- **JSON Serialization Fix (`RaceResultDto.cs`):** Add `[JsonPropertyName("positions")]` to the `Positions` property (and `FastestLapDriverId` / `PositionDto` properties) to strictly enforce camelCase serialization. Without this, the backend serialization falls back to PascalCase (because `JsonNamingPolicy` is not explicitly set in `Program.cs`), causing the frontend to read `resultDto.positions` as `undefined` and displaying an empty grid even when results exist.

**Verification:** 
- Open the Admin Panel and assign drivers to finishing positions for a Scheduled race.
- Click **Save**. Verify the success modal appears.
- Reload the page and click **Results** on the same race. The assigned drivers MUST be populated in the modal grid, proving successful database persistence and correct JSON mapping.
