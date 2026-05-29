# Task 1: Daily Login Points (Streak System)

## Objective
Implement a daily login streak system that awards points to users for logging in consecutively, with increasing bonus multipliers at streak milestones. The streak resets if a user misses a day.

## Scope

### Backend
- **Entity**: Create `DailyLoginStreak` entity with fields:
  - `Id`, `UserId` (FK), `CurrentStreak`, `LastLoginDate` (UTC date), `ClaimedToday` (bool), `UpdatedAt`
- **Repository**: Create `IDailyLoginStreakRepository` with methods for upsert, get by user, and update.
- **Service**: Create `IDailyLoginService` interface and `DailyLoginService` implementation:
  - `ProcessDailyLoginAsync(userId)` — called after successful authentication:
    1. Check if the user already has a `DailyLoginStreak` record
    2. If not, create one with `CurrentStreak = 1`, `LastLoginDate = today`, `ClaimedToday = true`
    3. If yes and `LastLoginDate == today`, return early (already claimed today)
    4. If yes and `LastLoginDate < today`:
       - If `LastLoginDate` is exactly yesterday → increment `CurrentStreak`
       - Otherwise → reset `CurrentStreak` to 1
       - Set `ClaimedToday = true`
       - Update `LastLoginDate = today`
    5. Calculate points based on streak (see table below)
    6. Award points to user via `user.AddPoints()`
    7. Record the point change in `PointHistory` with category `DailyLogin`
    8. Trigger `PointsAwardedEvent` domain event
    9. Save all changes atomically
  - `GetStreakInfoAsync(userId)` — returns streak count, current day points, next bonus milestone
- **Integration**: Hook `ProcessDailyLoginAsync` into the `UserService.AuthenticateUserAsync()` method, called after successful login.

### Point Calculation Table

| Streak Days | Daily Base Points | Bonus Multiplier | Effective Points |
|-------------|-------------------|------------------|------------------|
| 1 | 10 | ×1 | 10 |
| 2 | 10 | ×1 | 10 |
| 3 | 10 | ×1.5 | 15 |
| 4 | 10 | ×1.5 | 15 |
| 5 | 10 | ×2 | 20 |
| 6 | 10 | ×2 | 20 |
| 7+ | 10 | ×2.5 | 25 |

### Frontend (API only — UI in Task 5)
- `GET /api/users/profile/daily-streak` — returns:
  ```json
  {
    "currentStreak": 5,
    "lastLoginDate": "2026-05-28",
    "pointsToday": 20,
    "nextBonusMilestone": 7,
    "pointsAtNextMilestone": 25
  }
  ```

## Testing (In Isolation)
- **Unit Tests** for `DailyLoginService`:
  - New user logs in → creates streak, awards 10 points
  - User logs in next day → increments streak, awards points
  - User logs in same day again → no points awarded, returns early
  - User misses a day → streak resets to 1, awards 10 points
  - User hits streak day 3 → multiplier ×1.5 applied
  - User hits streak day 7 → multiplier ×2.5 applied
  - Concurrent login from two devices → only one awards points (test concurrency)
- **Integration Tests**:
  - End-to-end login flow that verifies streak is created and points are credited

## Out of Scope (Do Not Modify)
- **Quest System**: Do not implement weekly quests in this task.
- **Admin Controls**: Do not implement admin endpoints for streak configuration.
- **Frontend UI**: Do not modify profile page; only add the API endpoint.
- **Notifications**: Do not implement toast notifications yet.

## Reviewability
This PR is self-contained: it introduces the streak entity, service, repository, and API endpoint. It can be tested by logging in as a user and verifying the streak/points response.
