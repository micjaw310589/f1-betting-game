# Task 1: Migrate Shared Modal CSS Rules (Fixes Issue 4)

**Objective:** Ensure all confirmation dialogs within the system management module appear as proper overlay windows.

**Scope:** 
- Extract or port the modal CSS rules (`.modal-overlay`, `.modal-content`, `.modal-header`, etc.) into `admin-system-management.component.css`.

**Verification:** Trigger the race deletion dialog and verify it displays as a centralized overlay with a backdrop, rather than inline text.
