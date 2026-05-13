# Task 2: Fix Modal State Management on Tab Navigation (Fixes Issue 3)

**Objective:** Prevent orphaned modals from persisting across different tabs.

**Scope:**
- Modify the `switchTab` method in `admin-system-management.component.ts`.
- Explicitly set `showDeleteConfirm`, `showCreateRaceForm`, `showMetadataConfirmModal`, and `showResultsConfirmModal` to `false` upon navigation.

**Verification:** Open the race deletion dialog, navigate to another tab without confirming/cancelling, and verify the dialogue is closed.
