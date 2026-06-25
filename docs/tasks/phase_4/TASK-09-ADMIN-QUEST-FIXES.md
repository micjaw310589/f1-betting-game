# Task 9: Admin Quest Fixes — Reset Bug & Quest Creation Bug

## Objective
Fix two critical bugs in the admin quest management system:
1. **Quest manual reset makes quests unresponsive** — The reset endpoint clears ALL weekly quest progress indiscriminately, including already-completed/claimed quests, which breaks the completion/points-awarded flow.
2. **Quest addition doesn't work** — Quest creation via the admin panel is failing (to be diagnosed and fixed).

---

## Part 1: Quest Manual Reset Bug

### Problem Description

When an admin clicks "⚠️ Reset Weekly Quests", the `POST /api/admin/quest-definitions/reset-week` endpoint is called. The current implementation in `QuestDefinitionService.ResetWeeklyQuestsAsync()` calls `ResetAllWeeksAsync(weekNumber, year)` which resets **ALL** weekly quest progress records for all users in the current week — **including quests that were already completed and claimed**.

### Root Cause Analysis

**File:** `F1BettingApp/F1BettingApp.Infrastructure/Persistence/Repositories/WeeklyQuestProgressRepository.cs`

```csharp
// Current buggy implementation
public async Task<int> ResetAllWeeksAsync(int weekNumber, int year)
{
    var records = await _dbSet
        .Where(p => p.WeekNumber == weekNumber && p.Year == year)
        .ToListAsync();

    foreach (var record in records)
    {
        record.Progress = 0;
        record.IsCompleted = false;      // ❌ BUG: Resets completed quests too
        record.PointsAwarded = 0;         // ❌ BUG: Removes awarded points
        record.IsClaimed = false;         // ❌ BUG: Un-claims completed quests
        record.UpdatedAt = DateTime.UtcNow;
    }

    await SaveChangesAsync();
    return records.Count;
}
```

**Impact on the completion flow:**

In `QuestService.CheckAndCompleteQuestsAsync()`:
```csharp
if (progress != null && !progress.IsCompleted && progress.Progress >= progress.Target)
{
    // ... award points, mark as claimed
}
```

After reset:
1. `IsCompleted` is set to `false` for ALL records (including completed ones)
2. `PointsAwarded` is set to `0` — users lose their earned points
3. `IsClaimed` is set to `false` — quests appear unclaimed
4. The completion check `!progress.IsCompleted` is now true, but `progress.Progress` is also `0`, so `progress.Progress >= progress.Target` is **false**
5. The quest becomes **unresponsive** — it can never be re-completed because progress was wiped

### Expected Behavior

The reset should only reset **incomplete** weekly quest progress records:
- Records where `IsCompleted == false` → Reset to `Progress = 0, IsCompleted = false, PointsAwarded = 0, IsClaimed = false`
- Records where `IsCompleted == true` → **Leave unchanged** (preserve the completion and points)

### Proposed Fix

**File:** `F1BettingApp/F1BettingApp.Infrastructure/Persistence/Repositories/WeeklyQuestProgressRepository.cs`

```csharp
public async Task<int> ResetAllWeeksAsync(int weekNumber, int year)
{
    // Only reset INCOMPLETE progress records
    var records = await _dbSet
        .Where(p => p.WeekNumber == weekNumber && p.Year == year && !p.IsCompleted)
        .ToListAsync();

    foreach (var record in records)
    {
        record.Progress = 0;
        record.IsCompleted = false;
        record.PointsAwarded = 0;
        record.IsClaimed = false;
        record.UpdatedAt = DateTime.UtcNow;
    }

    await SaveChangesAsync();
    return records.Count;
}
```

### Related: Background Job QuestResetBackgroundJob

The background job (`QuestResetBackgroundJob.PerformWeeklyReset`) already handles reset correctly:
1. It resets the **previous** week's progress (not current week)
2. It re-evaluates quests via `CheckAndCompleteQuestsAsync` after reset
3. It handles individual user progress via `ResetWeekAsync` (which has the same bug)

**Additional fix needed:** `ResetWeekAsync` in `WeeklyQuestProgressRepository.cs` should also filter by `!IsCompleted`:

```csharp
public async Task ResetWeekAsync(int userId, int weekNumber, int year)
{
    var records = await _dbSet
        .Where(p => p.UserId == userId && p.WeekNumber == weekNumber && p.Year == year && !p.IsCompleted)
        .ToListAsync();

    foreach (var record in records)
    {
        record.Progress = 0;
        record.IsCompleted = false;
        record.PointsAwarded = 0;
        record.IsClaimed = false;
        record.UpdatedAt = DateTime.UtcNow;
    }

    await SaveChangesAsync();
}
```

### Testing

- **Unit test:** Verify that `ResetAllWeeksAsync` only resets records where `IsCompleted == false`
- **Unit test:** Verify that completed records (where `IsCompleted == true`) are preserved after reset
- **Integration test:** Create a quest, complete it, then call reset — verify the completed quest is preserved
- **Manual test:** Admin resets weekly quests, verify users can complete quests again in the new week

---

## Part 2: Quest Addition Bug

### Problem Description

The user reports that quest addition through the admin panel "doesn't work." The exact failure mode needs to be diagnosed.

### Investigation Areas

#### 2.1 QuestId Validation Pattern

**File:** `F1BettingApp/F1BettingApp.Application/Services/QuestDefinitionService.cs`

```csharp
private static readonly Regex QuestIdPattern = new(@"^[a-z_]+$", RegexOptions.Compiled);
```

The `QuestId` must match `^[a-z_]+$` — **only lowercase letters and underscores**. No numbers, no uppercase, no hyphens, no spaces.

**Frontend validation** (`admin-system-management.component.ts`):
```typescript
if (!/^[a-z_]+$/.test(this.questForm.questId)) {
    this.questFormError = 'Quest ID must contain only lowercase letters and underscores.';
    return;
}
```

Frontend and backend validation are **consistent**. If the user enters an invalid questId, they should see an error message.

**Question for the user:** What exact `questId` are you entering when creating a quest? Does the form show a validation error message?

#### 2.2 Required Fields

The `CreateQuestDto` requires:
- `QuestId` (non-empty, pattern validated)
- `Name` (non-empty)
- `Description` (non-empty)
- `Category` (must be "Betting", "Engagement", or "Achievement")
- `Target` (> 0)
- `PointsReward` (>= 0)

The frontend form has defaults for all fields:
```typescript
questForm: CreateQuestDefinitionDto = {
    questId: '',           // Must be filled by user
    name: '',              // Must be filled by user
    description: '',       // Must be filled by user
    category: 'Betting',   // Default
    isOneTime: true,       // Default
    target: 1,             // Default
    pointsReward: 100,     // Default
    order: 1,              // Default
    isActive: true,        // Default
};
```

#### 2.3 API Endpoint Routing

The frontend calls `POST /api/admin/quest-definitions` which maps to:
- **Controller:** `QuestDefinitionsController` (route: `/api/admin/quest-definitions`)
- **Method:** `Create([FromBody] CreateQuestDto dto)`
- **Authorization:** `[Authorize(Roles = "Admin")]`

**Question for the user:** Are you logged in as an admin when trying to create a quest?

#### 2.4 Backend Validation Errors

The backend returns specific error messages:
- Empty `QuestId` → `400 Bad Request` — "QuestId is required."
- Invalid pattern → `400 Bad Request` — "Invalid QuestId 'X'. Must match pattern ^[a-z_]+$"
- Empty `Name` → `400 Bad Request` — "Name is required."
- Invalid `Category` → `400 Bad Request` — "Invalid category 'X'. Must be one of: Betting, Engagement, Achievement"
- `Target <= 0` → `400 Bad Request` — "Target must be greater than 0."
- Negative `PointsReward` → `400 Bad Request` — "PointsReward must be greater than or equal to 0."
- Duplicate `QuestId` → `409 Conflict` — "Quest with QuestId 'X' already exists."

The frontend `handleError` extracts the message and displays it in `questFormError`.

**Question for the user:** Do you see any error messages in the UI or in the browser console when trying to create a quest?

#### 2.5 Frontend Response Handling

After successful creation, the frontend:
1. Sets `questFormSuccess = true`
2. Closes the form
3. Reloads the quest list

```typescript
this.adminService.createQuestDefinition(this.questForm).subscribe({
    next: () => {
        this.questFormSuccess = true;
        this.isSavingQuest = false;
        this.showQuestForm = false;
        this.loadQuestDefinitions();
        setTimeout(() => { this.questFormSuccess = false; }, 5000);
    },
    error: (error) => {
        this.questFormError = error.message || 'Failed to create quest';
        this.isSavingQuest = false;
    },
});
```

This flow looks correct.

### Potential Issues to Investigate

| # | Area | Potential Issue | Likelihood |
|---|------|----------------|------------|
| P1 | Validation | User enters questId with uppercase/numbers/hyphens | High |
| P2 | Auth | User is not logged in as admin | Medium |
| P3 | API | Backend not running or unreachable | Low |
| P4 | CORS | CORS blocking the request | Low |
| P5 | Form | Form validation passes but backend rejects | Low |
| P6 | Response | Backend returns unexpected format | Low |

### Diagnostic Steps

1. **Check browser DevTools Network tab** when submitting the quest creation form:
   - Is the POST request sent?
   - What is the status code?
   - What is the request payload?
   - What is the response body?

2. **Check browser console** for errors:
   - Any CORS errors?
   - Any Angular errors?
   - Any service errors?

3. **Check backend logs** for:
   - Any 400/401/500 errors on `POST /api/admin/quest-definitions`
   - Validation error messages

4. **Test with curl/Postman** directly against the API:
   ```bash
   curl -X POST https://f1-betting-game-api.onrender.com/api/admin/quest-definitions \
     -H "Authorization: Bearer <admin-token>" \
     -H "Content-Type: application/json" \
     -d '{
       "questId": "test_quest",
       "name": "Test Quest",
       "description": "A test quest",
       "category": "Betting",
       "isOneTime": true,
       "target": 1,
       "pointsReward": 100,
       "order": 1,
       "isActive": true
     }'
   ```

---

## Questions for the User

Before implementing, please answer the following:

### For the Reset Bug:
1. **Q1:** When you say quests become "unresponsive" after reset, do you mean:
   - a) Users can no longer complete quests (progress doesn't increment)?
   - b) Completed quests show as incomplete after reset (IsClaimed is false)?
   - c) Points are lost after reset?
   - d) Something else?

2. **Q2:** Is the reset being used during active gameplay (while users are completing quests), or is it only used for testing/debugging?

### For the Quest Addition Bug:
3. **Q3:** What exact steps do you take when trying to create a quest? (e.g., "I click '+ Create Quest', fill in the form, click 'Create Quest'")

4. **Q4:** What happens after you click "Create Quest"?
   - a) Nothing happens (button doesn't respond)?
   - b) Loading spinner shows, then an error appears?
   - c) Success message appears, but the quest doesn't show in the list?
   - d) Error message appears (what does it say)?
   - e) Page reloads or redirects?

5. **Q5:** What `questId` are you entering? (e.g., "first_bet", "FirstBet", "first-bet")

6. **Q6:** Are you logged in as an admin user when trying to create quests?

7. **Q7:** Can you check the browser's Network tab and tell me:
   - What status code does the POST `/api/admin/quest-definitions` request return?
   - What is the request payload?
   - What is the response body?

---

## Implementation Plan (Pending User Answers)

### Phase 1: Fix Reset Bug (Confirmed)
- [ ] Fix `ResetAllWeeksAsync` in `WeeklyQuestProgressRepository.cs` to only reset incomplete records
- [ ] Fix `ResetWeekAsync` in `WeeklyQuestProgressRepository.cs` to only reset incomplete records
- [ ] Update unit tests for `QuestDefinitionServiceTests.ResetWeeklyQuestsAsync`
- [ ] Add integration test for reset preserving completed quests
- [ ] Update `WeeklyQuestsIntegrationTests.cs`

### Phase 2: Fix Quest Addition Bug (After User Answers)
- [ ] Diagnose root cause based on user answers
- [ ] Implement fix
- [ ] Add/update tests
- [ ] Verify end-to-end

---

## Files to Modify

| File | Change |
|------|--------|
| `F1BettingApp/F1BettingApp.Infrastructure/Persistence/Repositories/WeeklyQuestProgressRepository.cs` | Fix `ResetAllWeeksAsync` and `ResetWeekAsync` to filter by `!IsCompleted` |
| `F1BettingApp/F1BettingApp.Tests/QuestDefinitionServiceTests.cs` | Update `ResetWeeklyQuestsAsync` tests to verify completed quests are preserved |
| `F1BettingApp/F1BettingApp.Tests/Integration/WeeklyQuestsIntegrationTests.cs` | Add test for reset preserving completed quests |
| *(TBD)* | Quest addition fix (depends on diagnosis) |

---

## Risk Assessment

| Risk | Level | Mitigation |
|------|-------|------------|
| Reset fix breaks existing behavior for users who relied on full reset | Low | The current behavior is clearly buggy (loses earned points). The fix restores correct behavior. |
| Background job behavior change | Low | The background job already handles re-evaluation; the fix makes it consistent with manual reset. |
| Quest addition fix unknown scope | Medium | Will be determined after user provides diagnostic information. |

---

## Acceptance Criteria

### Reset Bug
- [ ] `POST /api/admin/quest-definitions/reset-week` only resets incomplete quest progress
- [ ] Completed quests (IsCompleted=true, IsClaimed=true) retain their completion status and points after reset
- [ ] After reset, users can still complete weekly quests in the new week
- [ ] `QuestResetBackgroundJob` uses the same logic (resets previous week's incomplete progress)
- [ ] All existing tests pass
- [ ] New tests added for the fix

### Quest Addition Bug
- [ ] Quest creation works end-to-end (form submit → API call → quest appears in list)
- [ ] Validation errors are displayed correctly in the UI
- [ ] Admin-only access is enforced
- [ ] Tests cover quest creation flow
