# Task 4: Admin Quest Configuration

## Objective
Provide admin API endpoints for managing quest definitions — creating, updating, deleting, and toggling quests. This allows administrators to adjust point values, enable/disable quests, and add new quests without code changes.

## Scope

### Backend

#### Controller: `QuestDefinitionsController` (Admin)
- `[Authorize(Roles = "Admin")]`
- `[Route("api/admin/quest-definitions")]`

**Endpoints:**

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/admin/quest-definitions` | List all quest definitions (with optional `?isActive=true` filter) |
| `POST` | `/api/admin/quest-definitions` | Create a new quest definition |
| `PUT` | `/api/admin/quest-definitions/{id}` | Update an existing quest definition |
| `DELETE` | `/api/admin/quest-definitions/{id}` | Delete a quest definition |
| `PATCH` | `/api/admin/quest-definitions/{id}/active` | Toggle quest active/inactive |
| `POST` | `/api/admin/quests/reset-week` | Force reset all weekly quest progress (for testing/debugging) |

#### DTOs

```csharp
// Create/Update quest
public class CreateQuestDefinitionDto
{
    [Required]
    public string QuestId { get; set; }       // e.g. "betting_marathon"
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    [Required]
    public string Category { get; set; }      // "Betting" | "Engagement" | "Achievement"
    public bool IsOneTime { get; set; }
    [Required]
    public int Target { get; set; }
    [Required]
    public int PointsReward { get; set; }
    public int Order { get; set; }
}

public class ToggleQuestActiveDto
{
    [Required]
    public bool IsActive { get; set; }
}

// Response
public class QuestDefinitionDto
{
    public int Id { get; set; }
    public string QuestId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool IsOneTime { get; set; }
    public int Target { get; set; }
    public int PointsReward { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Service: `IQuestDefinitionService` + `QuestDefinitionService`
- `GetAllQuestDefinitionsAsync(filterActive?)` — returns all definitions
- `CreateQuestDefinitionAsync(dto)` — validates uniqueness of `QuestId`, creates entry
- `UpdateQuestDefinitionAsync(id, dto)` — updates fields; validates `QuestId` uniqueness
- `DeleteQuestDefinitionAsync(id)` — deletes the definition; existing `WeeklyQuestProgress` records are orphaned but remain (they just won't show in active quests)
- `ToggleQuestActiveAsync(id, isActive)` — toggles `IsActive`
- `ResetWeeklyQuestsAsync()` — resets all `WeeklyQuestProgress` records for the current week to `IsClaimed = false, Progress = 0`

#### Validation Rules
- `QuestId` must be unique and match pattern `^[a-z_]+$` (lowercase + underscore)
- `Target` must be > 0
- `PointsReward` must be ≥ 0
- `Category` must be one of: "Betting", "Engagement", "Achievement"
- Cannot delete a quest that has active `WeeklyQuestProgress` records (return error with count)

### Frontend (API only — UI in Task 5)
- The admin panel (existing `AdminController` or a new `QuestsController`) will call these endpoints.
- No frontend admin UI changes in this task.

## Testing (In Isolation)
- **Unit Tests** for `QuestDefinitionService`:
  - Creating a quest with duplicate `QuestId` → throws validation error
  - Updating a quest → fields are correctly updated
  - Toggling active/inactive → `IsActive` flips
  - Deleting a quest → removed from database
  - Deleting a quest with active progress → returns error
- **Integration Tests**:
  - Full CRUD cycle: create → read → update → toggle → delete
  - Admin-only access: unauthenticated user gets 401

## Out of Scope (Do Not Modify)
- **Quest Logic**: Quest evaluation and progress tracking is in Task 2.
- **Admin UI**: No Angular admin panel changes; only API endpoints.
- **Daily Login Streak**: Streak configuration is hardcoded in this task.
- **Frontend Profile**: No profile page changes.

## Reviewability
This PR is self-contained: it introduces the admin API for quest management. It can be tested by calling the endpoints with admin credentials and verifying the database state.
