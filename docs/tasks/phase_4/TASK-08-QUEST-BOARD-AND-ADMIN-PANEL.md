# Task 8: Quest Board & Admin Quest Panel

## Objective
Add a user-facing quest board page (catalog of all active quests with progress if logged in) and an admin quest management tab within the existing `AdminSystemManagementComponent`.

---

## 1. Quest Board (User-Facing)

### 1.1 Backend — New API Endpoint

**Endpoint:** `GET /api/quests` (public, no auth required)

Returns a list of all active quest definitions. If the user is authenticated, includes their current progress for each quest.

**DTO:** `QuestBoardDto` (new, in `F1BettingApp.Application/DTOs/`)

```csharp
public class QuestBoardDto
{
    public string QuestId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;     // "Betting" | "Engagement" | "Achievement"
    public bool IsOneTime { get; set; }
    public int Target { get; set; }
    public int PointsReward { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    
    // Optional progress (only populated if authenticated)
    public int? Progress { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsClaimed { get; set; }
}
```

**Controller:** `QuestsController` (new, in `F1BettingApp.API/Controllers/`)

```
[Route("api/quests")]
[ApiController]
public class QuestsController : ControllerBase
{
    private readonly IQuestService _questService;
    private readonly IQuestDefinitionService _questDefinitionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // GET /api/quests
    // Returns all active quest definitions.
    // If user is authenticated, includes their current progress.
    // Order by Order field.
}
```

**Service changes:** `IQuestService` needs a new method:
```csharp
Task<QuestBoardDto?> GetQuestBoardProgressAsync(string questId, int? userId);
```
This returns progress for a single quest for a single user (or null if not logged in). Called per-quest from the controller.

**Or simpler approach:** Add a method to `IQuestDefinitionService`:
```csharp
Task<List<QuestBoardDto>> GetActiveQuestBoardAsync(int? userId);
```
This returns all active quests, and if userId is provided, merges in progress for each.

### 1.2 Frontend — Quest Board Page

**Route:** `/quests` (added to `app.routes.ts`)

**Component:** `quest-board/quest-board.component.ts` (standalone)

**Layout:** Card grid, grouped by category. Each card shows:
- Quest name (with emoji/icon based on category)
- Description
- Category badge (color-coded)
- One-time vs Recurring badge
- Target value and points reward
- Progress bar (if logged in) with current progress / target
- "Completed" / "Claimed" indicator
- "Not started" state if progress is 0

**Categories with colors:**
- 🏎️ **Betting** — blue/cyan
- ⚡ **Engagement** — green
- 🏆 **Achievement** — gold/amber

**Navigation:** Add "Quests" link to the main navbar alongside Races, Profile, Admin.

**File structure:**
```
src/app/quest-board/
├── quest-board.component.ts
├── quest-board.component.html
├── quest-board.component.css
└── quest-board.models.ts
```

**Models (`quest-board.models.ts`):**
```typescript
export interface QuestBoardDto {
  questId: string;
  name: string;
  description: string;
  category: string;
  isOneTime: boolean;
  target: number;
  pointsReward: number;
  isActive: boolean;
  order: number;
  progress?: number | null;
  isCompleted?: boolean | null;
  isClaimed?: boolean | null;
}
```

**Service (`quest-board.service.ts`):**
```typescript
@Injectable({ providedIn: 'root' })
export class QuestBoardService {
  private readonly API_URL = `${environment.apiUrl}/quests`;
  
  getQuestBoard(): Observable<QuestBoardDto[]> {
    return this.http.get<QuestBoardDto[]>(this.API_URL);
  }
}
```

---

## 2. Admin Quest Panel

### 2.1 Implementation

Add a **"Quests" tab** to the existing `AdminSystemManagementComponent` (same component, new tab).

**Tab type:** Extend `activeTab` union type to include `'quests'`:
```typescript
activeTab: 'sync' | 'results' | 'metadata' | 'races' | 'bets' | 'quests' = 'bets';
```

**No new component needed** — quest management UI lives inside `AdminSystemManagementComponent`.

### 2.2 Quest Management UI

**Table columns:**
| ID | Quest ID | Name | Category | Type | Target | Reward | Active | Order | Completed By | Actions |

**Features:**
- **List all quests** (active + inactive) with pagination (20 per page)
- **Filter** by active/inactive status
- **Search** by name or quest ID
- **Sort** by name, category, order
- **Actions per row:**
  - ✏️ Edit (opens inline edit form or modal)
  - 🗑️ Delete (with confirmation modal, checks for active progress)
  - 🔄 Toggle active/inactive (instant toggle button)
  - 👁️ View progress (collapsible row or modal showing how many users completed it)
- **Create new quest** button (opens a form/modal)
- **Reset weekly quests** button (with confirmation)

**Create/Edit form fields:**
- Quest ID (lowercase + underscore, validated on submit)
- Name (required)
- Description (required)
- Category dropdown (Betting / Engagement / Achievement)
- One-time toggle
- Target (number, > 0)
- Points Reward (number, >= 0)
- Order (number)
- Active toggle

**Completion count:** Add a field to the admin response that shows how many users have completed each quest (lifetime count). This requires a new service method:
```csharp
Task<int> GetCompletedCountByQuestIdAsync(string questId);
```

### 2.3 Service Changes

**AdminService** — add methods:
```typescript
// Quest Definition Management
getAllQuestDefinitions(isActive?: boolean, page?: number, pageSize?: number, searchTerm?: string): Observable<PagedResult<QuestDefinitionDto>>;
createQuestDefinition(dto: CreateQuestDefinitionDto): Observable<QuestDefinitionDto>;
updateQuestDefinition(id: number, dto: UpdateQuestDefinitionDto): Observable<QuestDefinitionDto>;
deleteQuestDefinition(id: number): Observable<void>;
toggleQuestActive(id: number, isActive: boolean): Observable<QuestDefinitionDto>;
resetWeeklyQuests(): Observable<{ resetCount: number; message: string }>;
getQuestCompletedCount(questId: string): Observable<{ completedCount: number }>;
```

**Admin models** — add:
```typescript
export interface QuestDefinitionDto {
  id: number;
  questId: string;
  name: string;
  description: string;
  category: string;
  isOneTime: boolean;
  target: number;
  pointsReward: number;
  isActive: boolean;
  order: number;
  createdAt: Date;
  updatedAt: Date;
  completedCount?: number;
}

export interface CreateQuestDefinitionDto {
  questId: string;
  name: string;
  description: string;
  category: string;
  isOneTime: boolean;
  target: number;
  pointsReward: number;
  order: number;
  isActive: boolean;
}

export interface UpdateQuestDefinitionDto {
  name?: string;
  description?: string;
  category?: string;
  isOneTime?: boolean;
  target?: number;
  pointsReward?: number;
  order?: number;
  isActive?: boolean;
}

export const QUEST_CATEGORIES: { value: string; label: string }[] = [
  { value: 'Betting', label: '🏎️ Betting' },
  { value: 'Engagement', label: '⚡ Engagement' },
  { value: 'Achievement', label: '🏆 Achievement' },
];
```

### 2.4 Backend Changes for Admin

**QuestDefinitionsController** — The existing admin endpoints already support CRUD. Need to add:

1. **Pagination & search** to `GET /api/admin/quest-definitions`:
   - `?page=1&pageSize=20&isActive=true&searchTerm=first`
   - Return `PagedResult<QuestDto>` instead of `List<QuestDto>`

2. **Completed count** — Include `CompletedCount` in the admin quest DTO response. Add to `QuestDto`:
   ```csharp
   public int CompletedCount { get; set; }
   ```
   Set in `MapToDto` by counting `WeeklyQuestProgress` records where `IsClaimed = true` and `QuestId` matches.

3. **Service method:**
   ```csharp
   Task<int> GetCompletedCountByQuestIdAsync(string questId);
   ```

---

## 3. File Changes Summary

### Backend (C#)
| File | Change |
|------|--------|
| `F1BettingApp.Application/DTOs/QuestDto.cs` | Add `CompletedCount` property |
| `F1BettingApp.Application/DTOs/QuestBoardDto.cs` | **New file** — DTO for quest board |
| `F1BettingApp.Application/Interfaces/IQuestService.cs` | Add `GetQuestBoardProgressAsync` or update `IQuestDefinitionService` |
| `F1BettingApp.Application/Interfaces/IQuestDefinitionService.cs` | Add pagination + search + completed count methods |
| `F1BettingApp.Application/Services/QuestDefinitionService.cs` | Implement new methods |
| `F1BettingApp.Application/Services/QuestService.cs` | Implement quest board progress method |
| `F1BettingApp.API/Controllers/QuestsController.cs` | **New file** — public quest board endpoint |
| `F1BettingApp.API/Controllers/QuestDefinitionsController.cs` | Add pagination, search, completed count |
| `F1BettingApp.Infrastructure/Persistence/Repositories/IQuestDefinitionRepository.cs` | Add pagination + search |
| `F1BettingApp.Infrastructure/Persistence/Repositories/QuestDefinitionRepository.cs` | Implement pagination + search |
| `F1BettingApp.Infrastructure/Persistence/Repositories/IWeeklyQuestProgressRepository.cs` | Add `GetCompletedCountByQuestIdAsync` |
| `F1BettingApp.Infrastructure/Persistence/Repositories/WeeklyQuestProgressRepository.cs` | Implement completed count |
| Migration | New migration for any DTO changes (if needed) |

### Frontend (Angular)
| File | Change |
|------|--------|
| `src/app/app.routes.ts` | Add `/quests` route |
| `src/app/shared/nav-bar/nav-bar.html` | Add "Quests" nav link |
| `src/app/quest-board/quest-board.component.ts` | **New file** |
| `src/app/quest-board/quest-board.component.html` | **New file** |
| `src/app/quest-board/quest-board.component.css` | **New file** |
| `src/app/quest-board/quest-board.models.ts` | **New file** |
| `src/app/quest-board/quest-board.service.ts` | **New file** |
| `src/app/admin/models/admin.models.ts` | Add quest DTOs, categories constant |
| `src/app/admin/services/admin.service.ts` | Add quest management methods |
| `src/app/admin/admin-system-management/admin-system-management.component.ts` | Add 'quests' tab + quest management logic |
| `src/app/admin/admin-system-management/admin-system-management.component.html` | Add quests tab content |
| `src/app/admin/admin-system-management/admin-system-management.component.css` | Add quest table/modal styles |

---

## 4. Quest Board UI Wireframe

```
┌─────────────────────────────────────────────────────────────┐
│  🏁 Quest Board                                             │
│  Complete challenges to earn points!                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  🏎️ BETTING                    ⚡ ENGAGEMENT                │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ 🏁 First Check...│         │ 🏎️ Pole Position │         │
│  │ Place your firs...│         │ Log in 5 out of...│         │
│  │ 🏆 200 pts       │         │ 🏆 100 pts       │         │
│  │ ⏳ One-time      │         │ 🔄 Weekly        │         │
│  │                  │         │                  │         │
│  │ ████████████░░ 60%│        │ ░░░░░░░░░░░░░░░░ 0%  │         │
│  │ 0 / 1 completed  │         │ 0 / 5 progress   │         │
│  └──────────────────┘         └──────────────────┘         │
│                                                             │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ 🏎️ Race Day...  │         │ 🏎️ Race Day...  │         │
│  │ Place 1 bet du...│         │ Log in on Fri+S...│         │
│  │ 🏆 50 pts        │         │ 🏆 75 pts        │         │
│  │ 🔄 Weekly        │         │ 🔄 Weekly        │         │
│  │                  │         │                  │         │
│  │ ████████████░░ 50%│        │ ░░░░░░░░░░░░░░░░ 0%  │         │
│  │ 1 / 2 progress   │         │ 0 / 1 progress   │         │
│  └──────────────────┘         └──────────────────┘         │
│                                                             │
│  🏆 ACHIEVEMENT                                             │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │ 🏆 Winning Stre..│         │ 🏆 Comeback Ki..│         │
│  │ Win 3 bets in.. │         │ Win after 3 los..│         │
│  │ 🏆 300 pts       │         │ 🏆 150 pts       │         │
│  │ 🔄 Weekly        │         │ ⏳ One-time      │         │
│  │                  │         │                  │         │
│  │ ████████░░░░░░░░ 33%│       │ ░░░░░░░░░░░░░░░░ 0%  │         │
│  │ 1 / 3 progress   │         │ 0 / 1 completed  │         │
│  └──────────────────┘         └──────────────────┘         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Card states:**
- **Not started** (progress = 0): Gray progress bar, "Not started" label
- **In progress** (0 < progress < target): Colored progress bar with percentage
- **Completed but not claimed** (progress >= target, not claimed): Green bar, "Complete!" badge, claim button
- **Claimed** (claimed = true): Green bar with checkmark, "Completed ✓" label

---

## 5. Admin Quest Panel UI Wireframe

```
┌──────────────────────────────────────────────────────────────────────┐
│  Quests Management                                                   │
│  Create, edit, and manage quest definitions.                         │
├──────────────────────────────────────────────────────────────────────┤
│  [🔍 Search by name or quest ID...]  [Status: All ▼]  [+ Create Quest]│
│                                                                      │
│  ┌────┬─────────────────┬────────────┬──────────┬───────┬────────┬─┐│
│  │ ID │ Quest ID        │ Name       │ Category │ Type│ Target │ ││
│  ├────┼─────────────────┼────────────┼──────────┼───────┼────────┼─┤│
│  │ 1  │ first_bet       │ First Ch...│ 🏎️ Betting│One-time│ 1     │ ││
│  │    │                 │            │          │       │ 200 pts│ ││
│  │    │                 │            │          │       │ 0 done │ ││
│  │    │                 │            │          │       │        │ ││
│  │ 2  │ race_day_bet... │ Race Day..│ 🏎️ Betting│Weekly │ 1     │ ││
│  │    │                 │            │          │       │ 50 pts │ ││
│  │    │                 │            │          │       │ 12 done│ ││
│  └────┴─────────────────┴────────────┴──────────┴───────┴────────┴─┘│
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ Actions              │                                         │  │
│  ├────────────────────────────────────────────────────────────────┤  │
│  │ ✏️ Edit  🗑️ Delete  🔄 Toggle  👁️ Progress  │                 │  │
│  │ ✏️ Edit  🗑️ Delete  🔄 Toggle  👁️ Progress  │                 │  │
│  └────────────────────────────────────────────────────────────────┘  │
│                                                                      │
│  [First] [1] [2] [3] [Next]    Page 1 of 3 (42 quests)             │
│                                                                      │
│  [⚠️ Reset Weekly Quests]                                           │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 6. Testing (In Isolation)

### Quest Board
- **Unauthenticated:** `GET /api/quests` returns active quests without progress
- **Authenticated:** `GET /api/quests` returns active quests with progress for the logged-in user
- **Empty state:** When no quests are active, returns empty array
- **Ordering:** Quests are returned ordered by `Order` field, then by `QuestId`

### Admin Quest Panel
- **List all quests:** Returns paginated list with search/filter
- **Create quest:** Validates QuestId pattern, creates entry
- **Update quest:** Updates fields, validates uniqueness
- **Delete quest:** Fails if active progress exists (returns count)
- **Toggle active:** Flips `IsActive`
- **Reset weekly:** Resets all `WeeklyQuestProgress` for current week
- **Completion count:** Shows correct count of users who completed each quest
- **Admin-only:** Unauthenticated user gets 401

---

## 7. Out of Scope (Do Not Modify)
- **Quest logic/evaluation:** Already implemented in Task 2.
- **Profile page quest section:** Already exists; quest board is a separate page.
- **Daily login streak:** Not modified in this task.
- **Backend seed data:** Already seeded in Task 4.
- **Toasts/notifications:** Not modified in this task.

## 8. Reviewability
This task introduces two independent features:
1. **Quest Board** — A public/authenticated endpoint + Angular page showing quest catalog
2. **Admin Quest Panel** — A new tab in the existing admin system management component

Both can be tested independently. The quest board can be tested via the browser, and the admin panel via the admin dashboard after login as admin.
