# Task 6: Admin Panel - User Management

## Objective
Provide administrators with a secure interface to manage the platform's users.

## Scope
- **Backend**:
  - Introduce an `Admin` role in ASP.NET Core Identity.
  - Secure specific `UsersController` endpoints with `[Authorize(Roles = "Admin")]`.
  - Add endpoints to list all users, suspend/ban users, and manually adjust user points balances.
- **Frontend**:
  - Implement an `AdminGuard` to protect admin routes.
  - Create the `AdminUserManagementComponent` showing a data grid of all registered users.
  - Add modal forms for editing user balances and changing account status.

## Testing (In Isolation)
- **Backend Tests**:
  - Test Role-Based Access Control: ensure standard users get `403 Forbidden` when accessing admin endpoints.
  - Test the logic for adjusting points to ensure history/logs are kept if applicable.
- **Frontend Tests**:
  - Verify that the Admin menu item is hidden for regular users.
  - Test that the router guard redirects non-admins away from admin routes.

## Out of Scope (Do Not Modify)
- **System Settings**: Do not modify the background workers or API sync triggers.
- **Betting Service**: Do not modify core bet calculation or race results logic.
- **Auth Core**: Do not alter the JWT issuance, only the assignment and checking of the 'Admin' role.

## Reviewability
This introduces the RBAC foundation and the first set of admin tools. It's reviewable as a cohesive, isolated feature.
