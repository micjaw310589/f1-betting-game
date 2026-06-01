# Task 5: Frontend Profile Page Updates

## Objective
Extend the existing profile page (`user-profile.component.html`) with three new sections:
1. **Login Streak Card** — displays current streak, daily points, next bonus milestone
2. **Weekly Quests Card** — list of active quests with progress bars and completion status
3. **Points History Card** — paginated list of point transactions (new section below bet history)

## Scope

### Frontend

#### New Service Methods in `profile.service.ts`
```typescript
export interface DailyStreakResponse {
  currentStreak: number;
  lastLoginDate: Date;
  pointsToday: number;
  nextBonusMilestone: number;
  pointsAtNextMilestone: number;
}

export interface QuestResponse {
  questId: string;
  name: string;
  description: string;
  category: string;
  isOneTime: boolean;
  target: number;
  progress: number;
  isCompleted: boolean;
  isClaimed: boolean;
  pointsReward: number;
  isActive: boolean;
}

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

// Add to ProfileService:
getDailyStreak(): Observable<DailyStreakResponse>;
getQuests(): Observable<QuestResponse[]>;
getPointHistory(page: number, pageSize: number): Observable<PointHistoryResponseDto>;
```

#### New Models in `profile.models.ts`
- Add the interfaces above (`DailyStreakResponse`, `QuestResponse`, `PointHistoryDto`, `PointHistoryResponseDto`)

#### Component Updates: `user-profile.component.ts`
- Add new state properties:
  ```typescript
  dailyStreak: DailyStreakResponse | null = null;
  quests: QuestResponse[] = [];
  pointHistory: PointHistoryResponseDto | null = null;
  pointHistoryPage = 1;
  pointHistoryPageSize = 10;
  ```
- Update `load()` method to also fetch streak, quests, and point history (using `forkJoin`)
- Add methods:
  - `getStreakMultiplier(streak: number): string` — returns the multiplier text (e.g., "×2.5")
  - `getCategoryColor(category: string): string` — returns CSS color for quest category
  - `getPointsClass(points: number): string` — returns 'text-green' or 'text-red' for point history
  - `formatCategory(category: string): string` — formats category for display (e.g., "DailyLogin" → "Daily Login")

#### Template Updates: `user-profile.component.html`
Add three new `<section>` cards:

**1. Login Streak Card** (after the profile summary card):
```html
<section class="streak-card" aria-label="Login streak">
  <h2>Login Streak</h2>
  @if (dailyStreak) {
    <div class="streak-display">
      <div class="streak-fire">🔥</div>
      <div class="streak-count">{{ dailyStreak.currentStreak }} days</div>
      <div class="streak-points">
        Today: +{{ dailyStreak.pointsToday }} pts
        @if (dailyStreak.nextBonusMilestone) {
          <span class="streak-next">
            Next bonus at {{ dailyStreak.nextBonusMilestone }} days: +{{ dailyStreak.pointsAtNextMilestone }} pts
          </span>
        }
      </div>
      <!-- Streak progress dots (7 dots, filled based on currentStreak) -->
      <div class="streak-dots">
        @for (day of [1,2,3,4,5,6,7]; track day) {
          <div class="streak-dot" [class.filled]="day <= dailyStreak.currentStreak">
            {{ day }}
          </div>
        }
      </div>
    </div>
  }
</section>
```

**2. Weekly Quests Card** (before bet history):
```html
<section class="quests-card" aria-label="Weekly quests">
  <h2>Weekly Quests</h2>
  @if (quests.length === 0) {
    <div class="empty-state">No quests available this week.</div>
  } @else {
    <div class="quests-list">
      @for (quest of quests; track quest.questId) {
        <div class="quest-row" [class.quest-completed]="quest.isCompleted">
          <div class="quest-info">
            <div class="quest-name">{{ quest.name }}</div>
            <div class="quest-description">{{ quest.description }}</div>
            <div class="quest-progress">
              <div class="progress-bar">
                <div class="progress-fill" [style.width.%]="quest.target > 0 ? (quest.progress / quest.target * 100) : 0"></div>
              </div>
              <span class="progress-text">{{ quest.progress }} / {{ quest.target }}</span>
            </div>
          </div>
          <div class="quest-reward">
            @if (quest.isClaimed) {
              <span class="claimed-badge">✓ Claimed</span>
            } @else if (quest.isCompleted) {
              <span class="ready-badge">Ready to claim</span>
            } @else {
              <span class="reward-amount">+{{ quest.pointsReward }} pts</span>
            }
          </div>
        </div>
      }
    </div>
  }
</section>
```

**3. Points History Card** (after bet history):
```html
<section class="point-history-card" aria-label="Points history">
  <h2>Points History</h2>

  @if (pointHistory?.items.length === 0) {
    <div class="empty-state">No point transactions yet.</div>
  } @else {
    <div class="point-history-list">
      @for (entry of pointHistory?.items; track entry.id) {
        <div class="point-history-row">
          <div class="history-info">
            <span class="history-category" [class.cat-daily-login]="entry.category === 'DailyLogin'"
                  [class.cat-quest]="entry.category === 'Quest'"
                  [class.cat-bet]="entry.category === 'BetWin' || entry.category === 'BetLoss' || entry.category === 'BetPlacement'">
              {{ formatCategory(entry.category) }}
            </span>
            <span class="history-description">{{ entry.description }}</span>
          </div>
          <div class="history-points" [class.points-positive]="entry.points > 0" [class.points-negative]="entry.points < 0">
            {{ entry.points > 0 ? '+' : '' }}{{ entry.points }}
          </div>
          <div class="history-date">{{ entry.createdAt | date: 'mediumDate' }}</div>
        </div>
      }
    </div>

    <!-- Pagination -->
    <div class="pagination-controls">
      <button (click)="pointHistoryPage = pointHistoryPage - 1" [disabled]="pointHistoryPage <= 1">Previous</button>
      <span>Page {{ pointHistoryPage }}</span>
      <button (click)="pointHistoryPage = pointHistoryPage + 1" [disabled]="!hasMorePointHistory">Next</button>
    </div>
  }
</section>
```

#### CSS Updates: `user-profile.component.css`
Add styles for:
- `.streak-card` — card with streak display, fire emoji, progress dots
- `.streak-dots` — row of 7 dots, filled/empty states
- `.quests-card` — card with quest list
- `.quest-row` — each quest row with name, description, progress bar, reward
- `.progress-bar` / `.progress-fill` — visual progress indicator
- `.point-history-card` — card with transaction list
- `.point-history-row` — each transaction row
- `.points-positive` / `.points-negative` — green/red text colors
- `.history-category` — colored badge per category

#### TypeScript Interface Updates
Update `UserProfileComponent` class:
- Add `hasMorePointHistory` getter
- Add `pointHistoryPage` and `pointHistoryPageSize` properties
- Add `loadPointHistory()` method (separate from `load()` for lazy loading)
- Call `loadPointHistory()` in `ngOnInit` after the main load completes

## Testing (In Isolation)
- **Component Tests**:
  - Streak card renders correctly with streak data
  - Quests card renders progress bars and completion states
  - Points history card renders paginated list
  - Empty states show when no data is available
- **E2E Tests** (if applicable):
  - Navigate to profile page → verify all three sections render
  - Verify streak dots fill based on current streak
  - Verify progress bars update based on quest progress

## Out of Scope (Do Not Modify)
- **Backend API**: All API endpoints must already exist (from Tasks 1–4).
- **Toast Notifications**: Handled in Task 6.
- **Admin UI**: No admin panel changes.
- **Auth Module**: No changes to login/register.

## Reviewability
This PR is frontend-only and depends on the backend API from Tasks 1–4. It can be reviewed by running the app with mock data or with the backend features already deployed.
