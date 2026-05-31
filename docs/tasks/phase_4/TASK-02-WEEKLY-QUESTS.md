# Task 2: Weekly Quests System

## Objective
Implement a weekly quest system with recurring and one-time quests that award points when users complete specific actions. Quests are tracked per user, evaluated at defined triggers, and points are awarded atomically.

## Scope

### Backend

#### Entities

**`QuestDefinition`** — Admin-configurable quest template
- `Id` (int, PK), `QuestId` (string, unique), `Name`, `Description`, `Category` (enum: Betting/Engagement/Achievement), `IsOneTime` (bool), `Target` (int), `PointsReward` (int), `IsActive` (bool), `Order` (int), `CreatedAt`, `UpdatedAt`

**`WeeklyQuestProgress`** — Per-user quest progress
- `Id` (int, PK), `UserId` (int, FK), `QuestId` (string), `WeekNumber` (int), `Year` (int), `Progress` (int), `Target` (int), `IsCompleted` (bool), `PointsAwarded` (int), `IsClaimed` (bool), `UpdatedAt`

#### Seed Data
On migration, populate `QuestDefinition` with all quests from the IMPLEMENTATION_PLAN.md quest catalog.

#### Service: `IQuestService` + `QuestService`

**Core Methods:**
- `GetActiveQuestsAsync(userId)` — returns all active quest definitions with the user's current progress and claim status. For recurring quests, uses the current week. For one-time quests, aggregates lifetime progress.
- `CheckAndCompleteQuestsAsync(userId)` — evaluates all active quests for the user, awards points for newly completed ones, records in `PointHistory`, triggers `PointsAwardedEvent`. Called at weekly reset and at specific action triggers.
- `UpdateQuestProgressAsync(userId, questId, amount, additionalContext?)` — increments progress for a quest. Handles:
  - Weekly quests: uses current ISO week number + year
  - One-time quests: uses `WeekNumber = 0, Year = 0` as a sentinel
  - Upserts the `WeeklyQuestProgress` record if it doesn't exist
  - If progress ≥ target and not already claimed → marks as completed, awards points immediately
- `GetQuestDefinitionAsync(questId)` — returns a single quest definition (for admin/UI)

**Quest Completion Triggers (called from existing services):**
- `BettingService.PlaceBetAsync()` → calls `UpdateQuestProgressAsync` for:
  - `race_day_bettor` (+1 if the race is on Fri/Sat/Sun)
  - `betting_marathon` (+1)
  - `bold_move` (+1 if stake ≥ 1000)
  - `consistent_bettor` (+1, tracks unique dates)
  - `first_bet` (+1, one-time)
- `BettingService.ProcessRaceResultsAsync()` → calls `UpdateQuestProgressAsync` for:
  - `winning_streak` (+1 if bet was won)
  - `comeback_king` (tracks consecutive losses, awards on first win after ≥3 losses)
- `UserService.AuthenticateUserAsync()` → calls `UpdateQuestProgressAsync` for:
  - `login_streak_weekly` (+1 for today's login)
  - `streak_master` (awards one-time when streak hits 7)
- `RacePageVisitService` (new lightweight endpoint) → calls `UpdateQuestProgressAsync` for:
  - `race_explorer` (+1 per unique race viewed)

**Weekly Reset Job** — `QuestResetBackgroundJob`:
- Runs every Monday at 00:00 UTC
- For each user with active recurring quests:
  - Resets `WeeklyQuestProgress` records for the new week
  - Evaluates `login_streak_weekly` (counts logins in the past 7 days)
  - Evaluates `race_weekend_ready` (checks if user logged in on both Fri+Sat of the past race weekend)
  - Evaluates `top_10` (checks leaderboard position)
  - Awards points for completed quests

#### Service: `IQuestDefinitionService` (Admin)
- `GetAllQuestDefinitionsAsync()` — returns all quest definitions
- `CreateQuestDefinitionAsync(dto)` — creates a new quest
- `UpdateQuestDefinitionAsync(id, dto)` — updates an existing quest
- `DeleteQuestDefinitionAsync(id)` — deletes a quest (does not affect existing progress)
- `ToggleQuestActiveAsync(id, isActive)` — enables/disables a quest

### Frontend (API only — UI in Task 5)
- `GET /api/users/profile/quests` — returns:
  ```json
  {
    "quests": [
      {
        "questId": "betting_marathon",
        "name": "Betting Marathon",
        "description": "Place 5 bets in a single week",
        "category": "Betting",
        "isOneTime": false,
        "target": 5,
        "progress": 3,
        "isCompleted": false,
        "isClaimed": false,
        "pointsReward": 150,
        "isActive": true
      }
    ]
  }
  ```
- `GET /api/admin/quest-definitions` — admin list (see Task 4)

## Testing (In Isolation)
- **Unit Tests** for `QuestService`:
  - Placing a bet during a race weekend → `race_day_bettor` progress increments
  - Placing 5 bets in a week → `betting_marathon` completes, points awarded
  - Placing 5 bets in the next week → `betting_marathon` resets and starts fresh
  - One-time quest (`first_bet`) → completes once, does not reset
  - User wins 3 bets in a week → `winning_streak` completes
  - Concurrent quest updates → no double-awards (idempotency)
  - Weekly reset → progress resets for new week
- **Integration Tests**:
  - Full flow: place bets → check quest progress → verify points awarded
  - Weekly reset job → verify progress reset and evaluation

## Out of Scope (Do Not Modify)
- **Daily Login Streak**: Implemented in Task 1 only.
- **Admin UI**: Admin endpoints only; UI in Task 4/5.
- **Toast Notifications**: Only backend point awarding; UI in Task 6.
- **Race Page Visit Tracking**: Only the API endpoint; UI integration in Task 5.

## Reviewability
This PR focuses on the quest engine core: entities, service, seed data, and the weekly reset job. It can be tested by manually placing bets and checking quest progress via the API.
