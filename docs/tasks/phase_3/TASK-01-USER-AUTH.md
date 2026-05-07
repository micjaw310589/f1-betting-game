# Task 1: User Authorization & Authentication

## Objective
Implement secure user registration, login, and session management using JWT tokens.

## Scope
- **Backend**:
  - Configure ASP.NET Core Identity for user management.
  - Implement `AuthController` with endpoints for `POST /api/auth/register` and `POST /api/auth/login`.
  - Configure JWT authentication middleware and token generation logic.
  - Set up a refresh token mechanism to handle long user sessions safely.
- **Frontend**:
  - Create the Angular `AuthModule`.
  - Implement `LoginComponent` and `RegisterComponent` with proper form validation.
  - Create an HTTP Interceptor to attach the JWT token to all outbound API requests.
  - Create an `AuthService` to manage user session state (login, logout, token refresh).

## Testing (In Isolation)
- **Backend Tests**: 
  - Write unit tests for `AuthController` (valid/invalid credentials, duplicate emails).
  - Test JWT token generation and validation.
- **Frontend Tests**: 
  - Test login/register form validations.
  - Ensure the interceptor correctly attaches the Authorization header to requests.

## Out of Scope (Do Not Modify)
- **Race Data/OpenF1 Integration**: Do not modify any logic related to fetching or parsing race data.
- **Betting Logic**: Do not modify `BettingService` or `BetsController`.
- **Existing Frontend Modules**: Do not modify the `RaceList` or `RaceDetail` components beyond adding simple login checks if absolutely necessary.

## Reviewability
This can be reviewed as a single PR because it introduces the foundational auth layer without depending on betting or race data logic.
