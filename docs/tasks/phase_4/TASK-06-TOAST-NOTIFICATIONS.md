# Task 6: Toast Notifications for Points Earned

## Objective
Implement a toast notification system that displays a brief notification when a user earns points from daily login or quest completion. The toast appears at the bottom-right of the screen and auto-dismisses after 4 seconds.

## Scope

### Frontend

#### Toast Service: `toast.service.ts`
```typescript
export interface ToastMessage {
  id: string;
  type: 'success' | 'info' | 'warning';
  title: string;
  message: string;
  points?: number;       // optional: points earned
  duration?: number;     // default 4000ms
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toasts: ToastMessage[] = [];
  private readonly TOAST_ID_PREFIX = 'toast_';

  // Show a toast notification
  show(toast: Omit<ToastMessage, 'id'>): string;

  // Dismiss a specific toast
  dismiss(id: string): void;

  // Get current toasts (for template binding)
  getToasts(): ToastMessage[];

  // Convenience methods
  showPointsEarned(questName: string, points: number): void;
  showDailyLogin(streakDays: number, points: number): void;
}
```

#### Toast Component: `toast.component.ts`
- Standalone component
- Renders a list of active toasts from `ToastService`
- Each toast has:
  - A close button (✕)
  - Auto-dismiss timer (4s default)
  - Visual indicator for points earned (green icon + points amount)
  - Fade-in/fade-out animation

```html
<div class="toast-container">
  @for (toast of toasts; track toast.id) {
    <div class="toast toast-{{ toast.type }}" [class.toast-points]="toast.points !== undefined">
      <div class="toast-icon">
        @if (toast.points !== undefined) {
          <span class="toast-points-icon">🏆</span>
        } @else {
          <span class="toast-default-icon">ℹ️</span>
        }
      </div>
      <div class="toast-content">
        <div class="toast-title">{{ toast.title }}</div>
        <div class="toast-message">{{ toast.message }}</div>
        @if (toast.points !== undefined) {
          <div class="toast-points">+{{ toast.points }} points</div>
        }
      </div>
      <button class="toast-close" (click)="dismiss(toast.id)">✕</button>
    </div>
  }
</div>
```

#### Toast Container in App Layout: `app.html`
- Add `<app-toast></app-toast>` at the root level (below `<router-outlet>`)
- Position: fixed bottom-right, z-index high enough to overlay everything
- Max 3 toasts visible at once; older toasts auto-dismiss

#### Integration with Profile Page
- In `user-profile.component.ts`, after loading streak/quests:
  ```typescript
  // After loading streak
  if (this.dailyStreak?.pointsToday && this.dailyStreak.currentStreak > 0) {
    this.toastService.showDailyLogin(this.dailyStreak.currentStreak, this.dailyStreak.pointsToday);
  }

  // After loading quests, check for newly completed quests
  const newQuests = this.quests.filter(q => q.isCompleted && !q.isClaimed);
  for (const quest of newQuests) {
    this.toastService.showPointsEarned(quest.name, quest.pointsReward);
  }
  ```

#### CSS: `toast.component.css`
```css
.toast-container {
  position: fixed;
  bottom: 24px;
  right: 24px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 380px;
}

.toast {
  background: #1e1e2e;
  border: 1px solid #3a3a4a;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.4);
  animation: slideIn 0.3s ease-out;
  transition: opacity 0.3s, transform 0.3s;
}

.toast.toast-points {
  border-color: #ffd700;
  background: linear-gradient(135deg, #1e1e2e 0%, #2a2a1e 100%);
}

.toast-points-icon {
  font-size: 24px;
}

.toast-points {
  color: #ffd700;
  font-weight: bold;
  font-size: 14px;
  margin-top: 4px;
}

.toast-close {
  background: none;
  border: none;
  color: #888;
  cursor: pointer;
  font-size: 16px;
  padding: 0 4px;
}

@keyframes slideIn {
  from { transform: translateX(100%); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}

@keyframes fadeOut {
  from { opacity: 1; transform: translateX(0); }
  to { opacity: 0; transform: translateX(100%); }
}
```

## Testing (In Isolation)
- **Service Tests**:
  - `show()` adds a toast to the list
  - `dismiss()` removes a toast
  - `showPointsEarned()` creates a toast with points display
  - Max 3 toasts enforced (oldest auto-dismissed)
- **Component Tests**:
  - Toast renders with correct content
  - Close button dismisses the toast
  - Toast disappears after duration (use `fakeAsync`/`tick`)
  - Multiple toasts stack correctly
  - `toast-points` class applied when points are present

## Out of Scope (Do Not Modify)
- **Backend**: No backend changes; the frontend proactively shows toasts after loading data.
- **Real-time Push**: No WebSocket/SSE implementation; toasts are shown on page load/data refresh.
- **Toast Settings**: No user preferences for toast behavior.

## Reviewability
This PR is frontend-only and self-contained. It can be tested by loading the profile page and verifying toasts appear for streak/quest data.
