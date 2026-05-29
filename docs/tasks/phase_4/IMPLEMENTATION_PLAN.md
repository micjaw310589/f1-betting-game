# Technical Implementation Plan — Phase 4: Daily Login Streaks & Weekly Quests

## 1. Overview

This phase introduces a **daily login streak** system and **weekly quests** that award points from the same virtual currency pool used for betting. Points are earned automatically and displayed on the user's profile page with toast notifications.

### Goals
- Reward daily engagement through a login streak with increasing bonus multipliers
- Provide weekly quests (recurring + one-time) that encourage betting and platform usage
- Track all point awards in a history entity for transparency and auditability
- Allow admins to configure quest definitions and point values via the admin panel
- Display streak progress, quest progress, and points history on the profile page
- Show toast notifications when points are earned

### Non-Goals
- No new currency — all points go into the existing `User.Points` pool
- No social features (sharing, leaderboards per-quest, etc.)
- No cosmetic rewards or power-ups

---

## 2. Quest Catalog (Default Configuration)

### Recurring Weekly Quests (reset every week)

| Quest ID | Name | Description | Points | Type |
|----------|------|-------------|--------|------|
| `login_streak_weekly` | **Pole Position** | Log in 5 out of 7 days in a week | 100 | Recurring |
| `race_day_bettor` | **Race Day Bettor** | Place at least 1 bet during a race weekend (Fri–Sun) | 50 | Recurring |
| `betting_marathon` | **Betting Marathon** | Place 5 bets in a single week | 150 | Recurring |
| `bold_move` | **Bold Move** | Place a bet with 1000+ points stake in one go | 75 | Recurring |
| `consistent_bettor` | **Consistent Bettor** | Place at least 1 bet on 5 different days within a week | 200 | Recurring |
| `winning_streak` | **Winning Streak** | Win 3 bets in a single week | 300 | Recurring |
| `race_weekend_ready` | **Race Weekend Ready** | Log in on both Friday and Saturday of a race weekend | 75 | Recurring |
| `race_explorer` | **Race Explorer** | Visit race detail pages for 3 different races in a week | 50 | Recurring |

### One-Time Quests (never reset, tracked per user lifetime)

| Quest ID | Name | Description | Points | Type |
|----------|------|-------------|--------|------|
| `first_bet` | **First Checkered Flag** | Place your first bet ever | 200 | One-time |
| `comeback_king` | **Comeback King** | Win a bet after having 3 consecutive losing bets | 150 | One-time |
| `streak_master` | **Streak Master** | Maintain a 7-day login streak | 500 | One-time |
| `top_10` | **Top 10** | Finish in the top 10 of the leaderboard at any point during the week | 250 | Recurring |

### Daily Login Streak

| Streak Days | Daily Base Points | Bonus Multiplier |
|-------------|-------------------|------------------|
| 1 | 10 | ×1 |
| 2 | 10 | ×1 |
| 3 | 10 | ×1.5 |
| 4 | 10 | ×1.5 |
| 5 | 10 | ×2 |
| 6 | 10 | ×2 |
| 7+ | 10 | ×2.5 |

**Rules:**
- Points are awarded on each login if the user hasn't already claimed daily points for that day
- If a user misses a day, the streak resets to 0
- Streak is tracked in UTC

---

## 3. Architecture

### 3.1 Domain Entities

```
DailyLoginStreak
├── Id (int, PK)
├── UserId (int, FK → User)
├── CurrentStreak (int)          — consecutive days logged in
├── LastLoginDate (date)         — last day they logged in (UTC date)
├── ClaimedToday (bool)          — whether daily points were claimed today
└── UpdatedAt (datetime)

WeeklyQuestProgress
├── Id (int, PK)
├── UserId (int, FK → User)
├── QuestId (string)             — e.g. "betting_marathon"
├── WeekNumber (int)             — ISO week number
├── Year (int)                   — year for disambiguation
├── Progress (int)               — current progress value
├── Target (int)                 — target value (from QuestDefinition)
├── IsCompleted (bool)
├── PointsAwarded (int)          — points already awarded (for partial claims)
├── IsClaimed (bool)             — whether points have been awarded
└── UpdatedAt (datetime)

PointHistory
├── Id (int, PK)
├── UserId (int, FK → User)
├── Points (int)                 — positive = earned, negative = spent
├── Category (string)            — "DailyLogin", "Quest", "BetWin", "BetLoss", "AdminAdjustment"
├── Description (string)         — human-readable, e.g. "Login streak day 5"
├── ReferenceId (int?)           — optional reference (e.g. bet id, quest id)
├── CreatedAt (datetime)
└── Source (string)              — "System" | "Admin" | "Bet"

QuestDefinition
├── Id (int, PK)
├── QuestId (string, unique)     — e.g. "betting_marathon"
├── Name (string)                — display name
├── Description (string)         — tooltip/description
├── Category (string)            — "Betting" | "Engagement" | "Achievement"
├── IsOneTime (bool)             — one-time vs recurring weekly
├── Target (int)                 — target value to complete
├── PointsReward (int)           — points awarded on completion
├── IsActive (bool)              — admin toggle
├── Order (int)                  — display order
├── CreatedAt (datetime)
└── UpdatedAt (datetime)
```

### 3.2 Services

```
IDailyLoginService
├── ProcessDailyLoginAsync(userId)        — called on login; updates streak, awards points
├── GetStreakInfoAsync(userId)            — returns streak info for UI
└── ResetStreakAsync(userId)              — admin utility

IQuestService
├── GetActiveQuestsAsync(userId)          — returns all quests with progress
├── CheckAndCompleteQuestsAsync(userId)   — evaluates quest conditions, awards points
├── GetQuestDefinitionAsync(questId)      — returns quest config
└── UpdateQuestProgressAsync(...)         — increments progress for a quest

IQuestDefinitionService (Admin)
├── GetAllQuestDefinitionsAsync()
├── CreateQuestDefinitionAsync(dto)
├── UpdateQuestDefinitionAsync(id, dto)
├── DeleteQuestDefinitionAsync(id)
└── ToggleQuestActiveAsync(id, isActive)

IPointHistoryService
├── RecordPointChangeAsync(userId, points, category, description, source)
├── GetUserPointHistoryAsync(userId, page, pageSize)
└── GetWeeklyPointSummaryAsync(userId, weekNumber, year)
```

### 3.3 Integration Points

- **On Login**: `UserService` calls `IDailyLoginService.ProcessDailyLoginAsync()` after authentication
- **On Bet Placement**: `BettingService` calls `IQuestService.UpdateQuestProgressAsync("race_day_bettor", ...)` and `UpdateQuestProgressAsync("betting_marathon", ...)`
- **On Bet Resolution**: `BettingService` calls `IQuestService.UpdateQuestProgressAsync("winning_streak", ...)` and `UpdateQuestProgressAsync("bold_move", ...)`
- **Weekly Reset Job**: Background job runs every Monday at 00:00 UTC to:
  - Reset weekly quest progress for the new week
  - Award "Race Weekend Ready" quest if applicable
  - Award "Race Explorer" quest if applicable
- **On Race Page Visit**: Frontend calls an API that triggers `IQuestService.UpdateQuestProgressAsync("race_explorer", ...)`
- **On Leaderboard Check**: `IQuestService.CheckAndCompleteQuestsAsync()` is called to evaluate "Top 10" quest

### 3.4 API Endpoints

```
GET    /api/users/profile/daily-streak          — Get current login streak info
POST   /api/users/profile/quests                — Get all quests with progress (called on profile load)
GET    /api/users/profile/point-history          — Get paginated point history
GET    /api/admin/quest-definitions              — List all quest definitions (admin)
POST   /api/admin/quest-definitions              — Create quest definition (admin)
PUT    /api/admin/quest-definitions/{id}         — Update quest definition (admin)
DELETE /api/admin/quest-definitions/{id}         — Delete quest definition (admin)
PATCH  /api/admin/quest-definitions/{id}/active  — Toggle active/inactive (admin)
POST   /api/admin/quests/reset-week              — Force reset weekly quests (admin)
```

---

## 4. Tasks

| Task | File | Description | Layer |
|------|------|-------------|-------|
| 1 | `TASK-01-DAILY-LOGIN-POINTS.md` | Daily login streak system | Backend |
| 2 | `TASK-02-WEEKLY-QUESTS.md` | Weekly quest system (definitions, progress, completion) | Backend |
| 3 | `TASK-03-POINTS-HISTORY.md` | Point history entity, logging, and API | Backend |
| 4 | `TASK-04-ADMIN-QUEST-CONFIG.md` | Admin API for quest management | Backend |
| 5 | `TASK-05-FRONTEND-PROFILE-UPDATES.md` | Profile page: streak, quests, points history UI | Frontend |
| 6 | `TASK-06-TOAST-NOTIFICATIONS.md` | Toast notifications for points earned | Frontend |
| 7 | `TASK-07-INTEGRATION-TESTS.md` | End-to-end tests for the full system | Both |

---

## 5. Database Migration

A single EF Core migration will be created that:
1. Adds `DailyLoginStreak` table
2. Adds `WeeklyQuestProgress` table
3. Adds `PointHistory` table
4. Adds `QuestDefinition` table with seed data (all default quests)
5. Adds foreign key indexes for performance

---

## 6. Weekly Quest Completion Evaluation

Quests are checked for completion at the following triggers:

| Quest | Trigger |
|-------|---------|
| `login_streak_weekly` | Weekly reset job (Monday 00:00 UTC) |
| `race_day_bettor` | On bet placement |
| `betting_marathon` | On bet placement |
| `bold_move` | On bet placement |
| `consistent_bettor` | On bet placement (track unique days) |
| `winning_streak` | On bet resolution (won/lost) |
| `race_weekend_ready` | Weekly reset job (checks Fri+Sat logins) |
| `race_explorer` | On race page visit API call |
| `first_bet` | On bet placement |
| `comeback_king` | On bet resolution |
| `streak_master` | On daily login (when streak hits 7) |
| `top_10` | On leaderboard fetch or weekly reset |

Points are awarded **once per completion** — if a quest is already claimed, subsequent triggers skip the award.

---

## 7. Frontend Profile Page Changes

The existing profile page (`user-profile.component.html`) will be extended with:

1. **Login Streak Card** — displays current streak, daily points, next bonus milestone
2. **Weekly Quests Card** — list of active quests with progress bars, completion status
3. **Points History Card** — paginated list of point transactions (new section below bet history)

A new `profile.models.ts` interface will be added for streak/quest/point-history DTOs.

A new `profile.service.ts` method will be added for fetching streak, quests, and history.

---

## 8. Toast Notifications

When a user earns points (daily login or quest completion):
- A toast notification appears at the bottom-right of the screen
- Shows the quest/streak name, points earned, and a brief message
- Auto-dismisses after 4 seconds
- Can be manually dismissed

Implementation: Angular toast component (e.g., using a simple custom service with `@angular/cdk/overlay` or a lightweight notification library).

---

## 9. Risk & Considerations

| Risk | Mitigation |
|------|-----------|
| Race conditions on daily login (user logs in from multiple devices) | Use optimistic concurrency or `INSERT ... ON CONFLICT UPDATE` on `DailyLoginStreak` |
| Quest progress counted multiple times | Quest completion is idempotent — points awarded only once per `QuestId` per `WeekNumber` (or lifetime for one-time quests) |
| Weekly reset race condition (user logs in during reset) | Weekly reset job runs during low-traffic hours (00:00 UTC); use database transactions |
| Admin changes quest mid-week | Quest changes apply to the current week; progress already earned is preserved |
| Large point injections from quests breaking betting economy | Default quest values capped at ~1,500 pts/week max; admin can adjust values |
| Race weekend detection depends on race calendar | Use the `Race` entity's `Date` field to determine if a given day is a race weekend |
