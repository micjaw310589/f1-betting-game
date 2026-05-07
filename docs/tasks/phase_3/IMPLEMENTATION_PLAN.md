# Technical Implementation Plan

## 1. Bet Creation

### Description
The user selects an upcoming race, fills out a bet form, and confirms the bet. Their virtual points balance is updated immediately after creation. Users can also cancel bets before the race starts.

### Implementation Details
- **Frontend**: Create a bet placement form component in the Angular Betting Module. The form will handle bet type selection, driver/team selection, and amount input.
- **Backend**: Utilize the existing BetsController and BettingService. 
  - The PlaceBet service method will validate the user's balance, race status (must be scheduled), and driver validity. It will deduct the bet amount from the user's virtual points and save the bet.
  - The CancelBet service method will allow users to cancel bets and refund points as long as the race has not started.
- **Database**: Ensure atomic transactions using Entity Framework Core when creating the bet record and updating the user's points balance.

### Integration Points
- Frontend forms integrating with the Bets API endpoints.
- BettingService integrates with User and Race repositories.

### Potential Conflicts
- **Concurrency**: High traffic right before a race starts could lead to race conditions when deducting points. Requires optimistic concurrency control on the user's points balance.
- **Race Status Changes**: A race might switch to 'InProgress' exactly when a user hits confirm. The transaction must strictly verify race status within the transaction scope.

---

## 2. Bet Conclusion & Background Worker

### Description
A background worker periodically checks for race completion status. Once a race concludes, the system processes all bets placed on that race, updates user balances, and sends notifications.

### Implementation Details
- **Background Worker**: Implement a Hosted Service or utilize Hangfire to periodically query race statuses.
- **Result Processing**: Once a race is marked as finished, the worker calls the race results processing logic in BettingService. This will fetch the final standings, evaluate all pending bets for the race, calculate winnings, and update user balances.
- **Notifications**: Integrate the NotificationService to alert users of their bet outcomes (won/lost and amount).

### Integration Points
- Background worker integrates with OpenF1 API to check status.
- Background worker integrates with BettingService to trigger evaluation.
- BettingService integrates with NotificationService.

### Potential Conflicts
- **Provisional vs. Official Results**: Formula 1 results can change hours after a race due to penalties. The worker should either wait for official confirmation or support recalculating and adjusting balances if results change.
- **Idempotency**: The worker must ensure it does not process the same race twice, preventing double payouts. A status flag like 'ResultsProcessed' must be strictly enforced.

---

## 3. OpenF1 API Integration

### Description
The backend periodically downloads and synchronizes race data, standings, and driver/team info from the OpenF1 API to the local database. Sync intervals vary based on whether it is a "race week".

### Implementation Details
- **Scheduling Logic**: The worker will dynamically adjust its polling frequency. If the current date is within a 7-day window of an upcoming scheduled race ("race week"), it polls at maximum 1-hour intervals. Otherwise, it polls every 12 hours.
- **Data Synchronization**: Implement synchronization services using the existing OpenF1Client. These services will map OpenF1 data structures to local domain entities and update the database.

### Integration Points
- OpenF1Client fetching from the external OpenF1 API.
- Synchronization background jobs updating the local SQL database.

### Potential Conflicts
- **API Rate Limits**: Frequent polling could trigger rate limits. Proper error handling and backoff strategies are required.
- **Data Overwrites**: If an admin manually overrides a race result (see Admin Panel), the automatic sync should not overwrite the admin's manual changes. A flag indicating manual override is necessary.

---

## 4. User Authorization & Authentication + Profile Page

### Description
Secure user registration, login, session management, and a user profile page displaying basic settings and bet history.

### Implementation Details
- **Backend Authentication**: Use ASP.NET Core Identity with JWT tokens. The AuthController handles registration, login, and token issuance. 
- **User Dashboard**: The UsersController provides endpoints for fetching profile data and user-specific bet history.
- **Frontend**: Implement an Auth Module in Angular with login/register views, and a Profile component displaying user details and paginated bet history.

### Integration Points
- Frontend HTTP Interceptors attaching JWT tokens to protected API requests.
- AuthController integrating with ASP.NET Core Identity.

### Potential Conflicts
- **Token Expiration**: Long user sessions on the web app might experience sudden token expirations during critical actions (like placing a bet). Implementing a refresh token mechanism is highly recommended.

---

## 5. Admin Panel

### Description
A dedicated area for administrators to manage users (suspend/ban, adjust balances) and manage bets (override results, manually trigger syncs).

### Implementation Details
- **Role-Based Access Control**: Introduce an 'Admin' role. Secure admin endpoints using authorization policies requiring this role.
- **User Management**: Add endpoints for admins to list all users, adjust point balances, and change user statuses.
- **Bet/System Management**: Add endpoints to allow admins to force-trigger OpenF1 data synchronization and manually input or override race results.

### Integration Points
- Angular Admin Module routing guarded by role checks.
- Backend Admin endpoints integrating directly with User and Race repositories.

### Potential Conflicts
- **Data Integrity**: An admin adjusting a user's balance concurrently with a bet placement could cause data anomalies. 
- **Result Overrides**: As mentioned in the OpenF1 section, admin overrides must lock the record from future automatic OpenF1 sync updates to prevent conflicts.
