# Task 7: Integration Tests

## Objective
Create end-to-end integration tests covering the complete flow of the daily login streak and weekly quest systems, including point awards, history logging, and API responses.

## Scope

### Backend Integration Tests

#### Test Fixture: `PointsSystemTestFactory`
- A test factory that creates an in-memory or test database context
- Provides helper methods:
  - `CreateTestUser()` — creates a user with 10,000 starting points
  - `PlaceBetAsync(userId, raceId, amount, betType)` — creates a bet
  - `ResolveBetAsync(betId, status)` — resolves a bet as won/lost
  - `SimulateLoginAsync(userId, daysAgo?)` — simulates a login on a specific day
  - `AdvanceTimeByDays(days)` — advances the system clock for time-based tests

#### Test Suite: `DailyLoginStreakIntegrationTests`

| Test | Description |
|------|-------------|
| `FirstLogin_CreatesStreakAndAwardsPoints` | User logs in → streak created, 10 points awarded, history entry created |
| `ConsecutiveLogin_IncrementsStreak` | Day 1 + Day 2 login → streak = 2, 10 points awarded |
| `MissedDay_ResetsStreak` | Day 1 login, skip Day 2, Day 3 login → streak = 1, 10 points awarded |
| `SameDayLogin_NoDuplicatePoints` | User logs in twice on same day → only first login awards points |
| `StreakDay3_AppliesMultiplier` | Login streak reaches 3 → 15 points awarded (×1.5) |
| `StreakDay7_AppliesMaxMultiplier` | Login streak reaches 7 → 25 points awarded (×2.5) |
| `StreakHistoryEntryCreated` | Each login awards a `PointHistory` entry with category `DailyLogin` |

#### Test Suite: `WeeklyQuestsIntegrationTests`

| Test | Description |
|------|-------------|
| `PlaceBet_DuringRaceWeekend_IncrementsRaceDayBettor` | Place bet on Friday → `race_day_bettor` progress = 1 |
| `PlaceBet_5Times_CompletesBettingMarathon` | Place 5 bets in a week → quest completes, 150 points awarded |
| `NextWeek_QuestResets` | After weekly reset, `betting_marathon` progress = 0 again |
| `OneTimeQuest_NeverResets` | `first_bet` completes on first bet, stays completed across weeks |
| `Win3Bets_CompletesWinningStreak` | Win 3 bets in a week → `winning_streak` completes, 300 points awarded |
| `BoldMove_AwardsPointsForLargeStake` | Place 1500-point bet → `bold_move` completes, 75 points awarded |
| `QuestAlreadyClaimed_NoDoubleAward` | Completed quest doesn't award points again |
| `WeeklyReset_EvaluatesLoginStreakWeekly` | After reset, `login_streak_weekly` counts logins in past 7 days |

#### Test Suite: `PointHistoryIntegrationTests`

| Test | Description |
|------|-------------|
| `BetPlacement_CreatesNegativeHistoryEntry` | Placing a 500-point bet → history entry with -500, category `BetPlacement` |
| `BetWin_CreatesPositiveHistoryEntry` | Winning a bet → history entry with winnings, category `BetWin` |
| `DailyLogin_CreatesHistoryEntry` | Login streak points → history entry with category `DailyLogin` |
| `QuestCompletion_CreatesHistoryEntry` | Quest completion → history entry with category `Quest` |
| `HistoryPaginatedCorrectly` | Request page 2 of history → correct items returned |
| `WeeklySummary_CalculatesCorrectTotals` | Weekly summary → correct earned/spent totals |

#### Test Suite: `AdminQuestConfigIntegrationTests`

| Test | Description |
|------|-------------|
| `CreateQuest_ValidDto_CreatesEntry` | POST valid quest → 201, entry in DB |
| `CreateQuest_DuplicateQuestId_ReturnsConflict` | POST duplicate `QuestId` → 409 |
| `UpdateQuest_ChangesPointsReward` | PUT new points value → updated in DB |
| `ToggleQuestActive_DisablesQuest` | PATCH active=false → `IsActive` = false |
| `DeleteQuest_RemovesDefinition` | DELETE quest → removed from DB |
| `UnauthenticatedUser_CannotAccessAdminEndpoints` | GET/POST without admin role → 401 |

### Frontend Integration Tests

#### Test Suite: `UserProfileComponentIntegrationTests`

| Test | Description |
|------|-------------|
| `ProfileLoadsWithStreakData` | Mock API returns streak → streak card renders with correct values |
| `ProfileLoadsWithQuests` | Mock API returns quests → quests card renders with progress bars |
| `ProfileLoadsWithPointHistory` | Mock API returns history → history card renders paginated entries |
| `EmptyStatesShowWhenNoData` | Empty API responses → empty state messages displayed |
| `PointHistoryPaginationWorks` | Clicking "Next" → fetches page 2, updates list |

#### Test Suite: `ToastServiceIntegrationTests`

| Test | Description |
|------|-------------|
| `ShowPointsEarned_AddsToastWithPoints` | Call `showPointsEarned` → toast with points displayed |
| `Dismiss_RemovesToast` | Click close → toast removed from list |
| `Max3Toasts_OldestAutoDismissed` | Show 4 toasts → only 3 visible |
| `AutoDismiss_AfterDuration` | Wait 4s → toast auto-dismissed |

### Test Infrastructure

#### Database
- Use **PostgreSQL test container** (via `Testcontainers`) for integration tests
- Each test class gets a fresh database instance
- Use EF Core in-memory or test container with migration application

#### Mocking Strategy
- `QuestDefinitionRepository` — mock for admin tests (return predefined quests)
- `RaceRepository` — mock for quest trigger tests (return races with specific dates)
- `NotificationService` — mock to verify notifications are triggered

#### Running Tests
```bash
# Run all phase 4 integration tests
dotnet test F1BettingApp.Tests --filter "FullyQualifiedName~Phase4"

# Run specific test suite
dotnet test F1BettingApp.Tests --filter "FullyQualifiedName~DailyLoginStreak"
```

## Out of Scope (Do Not Modify)
- **Unit Tests**: Unit tests for individual services are covered in their respective task files. This task focuses on integration-level tests.
- **E2E Browser Tests**: No Playwright/Cypress tests; only API-level integration tests.
- **Performance Tests**: No load testing for concurrent logins or quest evaluations.

## Reviewability
This PR adds only test code — no production code changes. It depends on all previous tasks being implemented. Tests can be run independently using the test database setup.
