# Task 3: Points History & Point Logging

## Objective
Create a `PointHistory` entity and service to track all point changes (earnings and spending) with category, description, and source. This provides an audit trail and a user-facing history view on the profile page.

## Scope

### Backend

#### Entity: `PointHistory`
```
Id              int (PK)
UserId          int (FK → User)
Points          int          (positive = earned, negative = spent)
Category        string       ("DailyLogin" | "Quest" | "BetWin" | "BetLoss" | "AdminAdjustment" | "BetPlacement" | "BetCancellation")
Description     string       (human-readable, e.g. "Login streak day 5", "Quest: Betting Marathon")
ReferenceId     int?         (optional: bet id, quest id, or other reference)
CreatedAt       datetime
Source          string       ("System" | "Admin" | "Bet")
```

#### Repository: `IPointHistoryRepository`
- `AddAsync(PointHistory)`
- `GetByUserIdAsync(userId, page, pageSize)` — paginated history
- `GetWeeklySummaryAsync(userId, weekNumber, year)` — total earned/spent for a week

#### Service: `IPointHistoryService` + `PointHistoryService`
- `RecordPointChangeAsync(userId, points, category, description, source, referenceId?)` — creates a history entry. Called whenever points are awarded or deducted.
- `GetUserPointHistoryAsync(userId, page, pageSize)` — returns paginated history with newest first
- `GetWeeklyPointSummaryAsync(userId, weekNumber, year)` — returns total earned and spent for a week

#### Integration Points
- **Daily Login**: `DailyLoginService.ProcessDailyLoginAsync()` calls `RecordPointChangeAsync(points, "DailyLogin", "Login streak day X", "System")`
- **Quest Completion**: `QuestService.CheckAndCompleteQuestsAsync()` calls `RecordPointChangeAsync(points, "Quest", "Quest: QuestName", "System")`
- **Bet Placement** (existing): `BettingService.PlaceBetAsync()` calls `RecordPointChangeAsync(-amount, "BetPlacement", "Bet on RaceName", "Bet")`
- **Bet Win** (existing): `BettingService.ProcessRaceResultsAsync()` calls `RecordPointChangeAsync(winnings, "BetWin", "Won bet on RaceName", "Bet")`
- **Bet Loss** (existing): `BettingService.ProcessRaceResultsAsync()` calls `RecordPointChangeAsync(-amount, "BetLoss", "Lost bet on RaceName", "Bet")`
- **Admin Adjustment** (existing): `UserService.AdjustUserPointsAsync()` calls `RecordPointChangeAsync(delta, "AdminAdjustment", reason, "Admin")`

#### API Endpoint
- `GET /api/users/profile/point-history?page={n}&pageSize={m}` — returns:
  ```json
  {
    "items": [
      {
        "id": 1,
        "points": 20,
        "category": "DailyLogin",
        "description": "Login streak day 5",
        "createdAt": "2026-05-28T10:30:00Z"
      },
      {
        "id": 2,
        "points": -500,
        "category": "BetPlacement",
        "description": "Bet on Monaco Grand Prix",
        "createdAt": "2026-05-28T11:00:00Z"
      }
    ],
    "totalCount": 42,
    "pageNumber": 1,
    "pageSize": 20
  }
  ```

### Frontend (API only — UI in Task 5)
- The existing `profile.models.ts` will be extended with:
  ```typescript
  export interface PointHistoryDto {
    id: number;
    points: number;
    category: string;
    description: string;
    createdAt: Date;
  }

  export interface PointHistoryResponseDto {
    items: PointHistoryDto[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }
  ```

## Testing (In Isolation)
- **Unit Tests** for `PointHistoryService`:
  - Recording a point change creates a history entry
  - Negative points are stored correctly (spent vs earned)
  - Pagination works correctly
  - Weekly summary calculates totals correctly
- **Integration Tests**:
  - Full flow: user places a bet → verifies two history entries (deduction + potential win/loss)
  - User earns daily login points → verifies history entry with correct category

## Out of Scope (Do Not Modify)
- **Quest System**: This task only records history; quest logic is in Task 2.
- **Admin UI**: Admin endpoints only; UI in Task 4/5.
- **Frontend UI**: No profile page changes; only the API endpoint.
- **Existing Point Logic**: Do not modify `User.AddPoints()` / `User.DeductPoints()` — they remain unchanged. This task adds a parallel history log.

## Reviewability
This PR is self-contained: it introduces the `PointHistory` entity, repository, service, and API endpoint. It can be tested by calling the history endpoint after placing a bet or earning points.
