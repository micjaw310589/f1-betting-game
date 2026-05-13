# Task 6: Implement On-Screen Notifications (Bet Conclusion & Race Updates)

**Objective:** Complete the backend-to-frontend pipeline for user notifications regarding bet resolutions and race status updates, as requested in Phase 3 Task 04.

**Scope:**
- **Backend Controller (`NotificationsController.cs`):** 
  - Create a new API controller to expose `NotificationService`.
  - Add `GET /api/notifications/unread` returning unread notifications for the currently authenticated user (`GetUnreadNotificationsAsync`).
  - Add `PUT /api/notifications/{id}/read` to mark a specific notification as read (`MarkNotificationAsReadAsync`).
- **Frontend Notification Flow (`bet-result-notification.component.ts`):**
  - Implement an interval timer inside `ngOnInit` to periodically execute `fetchAndDisplayNotifications()` (e.g., every 15 seconds).
  - Crucially, modify `fetchAndDisplayNotifications()` to send a request to `PUT /api/notifications/{id}/read` immediately after calling `showNotificationInternal()`. This clears the notification from the unread queue and prevents the same notification from appearing repeatedly in subsequent polls.
  - Ensure the polling interval is cleared in `ngOnDestroy` to prevent memory leaks.

**Verification:**
- Simulate a race finishing and triggering the background worker (or manual admin override).
- Observe the notification pop-up automatically appearing on the frontend within 15 seconds.
- Refresh the page and ensure the notification does *not* reappear (verifying the idempotency/read-flagging).
