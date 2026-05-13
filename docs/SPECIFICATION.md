# F1 Betting Game - Comprehensive Specification

## 1. Overview

### 1.1 Purpose
The `f1-betting-game` is a web-based database application that allows users to bet on Formula 1 race outcomes using virtual points. The application integrates with the OpenF1 public API to provide real-time race data, championship standings, and results.

### 1.2 Goals
- Create an engaging platform for Formula 1 fans to test their predictive skills
- Provide real-time race data and betting functionality
- Offer a competitive environment with leaderboards and virtual currency
- Ensure seamless integration with external Formula 1 data sources

## 2. User Stories and Stakeholders

### 2.1 Stakeholders
| Stakeholder | Description | Goals |
|-------------|-------------|-------|
| **Players** | Registered users who place bets | Win virtual points, compete on leaderboards, enjoy F1 betting experience |
| **Administrators** | System maintainers | Manage users, monitor system health, ensure data integrity |
| **System** | Automated processes | Process race results, update standings, manage virtual currency |
| **OpenF1 API** | External data provider | Provide accurate, up-to-date Formula 1 data |

### 2.2 User Stories

#### 2.2.1 Player Stories
1. **Registration & Authentication**
   - As a player, I want to register an account so I can participate in betting
   - As a player, I want to log in securely so I can access my account
   - As a player, I want to reset my password if I forget it

2. **Account Management**
   - As a player, I want to view my current virtual balance
   - As a player, I want to see my betting history
   - As a player, I want to update my profile information

3. **Betting Functionality**
   - As a player, I want to view upcoming races so I can plan my bets
   - As a player, I want to see available betting options for each race
   - As a player, I want to place bets on race outcomes using my virtual points
   - As a player, I want to see my active bets
   - As a player, I want to cancel pending bets before race starts

4. **Race Information**
   - As a player, I want to view the current Formula 1 calendar
   - As a player, I want to see race details (circuit, date, time)
   - As a player, I want to view current championship standings
   - As a player, I want to see historical race results

5. **Competition**
   - As a player, I want to see the leaderboard to compare my performance
   - As a player, I want to see my ranking in the competition
   - As a player, I want to receive notifications about race results and my winnings

#### 2.2.2 Administrator Stories
1. **User Management**
   - As an admin, I want to view all registered users
   - As an admin, I want to suspend or ban users who violate rules
   - As an admin, I want to adjust user balances if needed

2. **System Monitoring**
   - As an admin, I want to view system logs and error reports
   - As an admin, I want to monitor API integration health
   - As an admin, I want to see betting statistics and patterns

3. **Content Management**
   - As an admin, I want to manually trigger race data synchronization
   - As an admin, I want to override race results if needed
   - As an admin, I want to manage betting rules and odds

## 3. Measurable Success Criteria

### 3.1 Quantitative Metrics
| Metric | Target | Measurement Method |
|--------|--------|---------------------|
| Registered users | 1,000+ within 3 months | Database count |
| Active users (weekly) | 30% of registered users | Login analytics |
| Bets placed per race | 500+ | Database count |
| System uptime | 99.9% | Monitoring tools |
| API response time | < 500ms | Performance monitoring |
| Race data synchronization | 100% accuracy | Comparison with source data |
| User satisfaction | 4.5/5 average rating | User surveys |

### 3.2 Qualitative Metrics
- Positive user feedback on betting experience
- Smooth integration with OpenF1 API
- Low number of support requests related to betting functionality
- High engagement during race weekends
- Clear and intuitive user interface

## 4. Functional Requirements

### 4.1 Core Features

#### 4.1.1 User Management
- User registration with email verification
- Secure authentication with JWT tokens
- Password reset functionality
- Profile management (username, avatar, preferences)
- Session management with token expiration

#### 4.1.2 Betting System
- Virtual currency system with initial balance (10,000 points)
- Multiple bet types:
  - TOP 3 drivers in race
  - Race winner
  - Podium finishers (1st, 2nd, 3rd)
  - Top 10 finishers
  - Fastest lap
  - Team with fastest pit stop
  - Number of drivers who won't finish (DNF)
  - Driver vs driver (head-to-head)
  - Team vs team
- Bet placement with point allocation
- Bet cancellation before race start
- Bet history with filtering and sorting
- Real-time odds calculation based on betting patterns

#### 4.1.3 Race Data Integration
- Automatic synchronization with OpenF1 API:
  - Race calendar
  - Race details (circuit, date, time)
  - Championship standings
  - Race results
  - Driver and team information
- Manual override capability for administrators
- Data validation and error handling

#### 4.1.4 Result Processing
- Automatic result processing after race completion via background jobs
- Point calculation based on bet outcomes (exact and partial matches)
- Leaderboard updates with historical tracking
- Notification system for bet results
- Historical data storage and retrieval

#### 4.1.5 Competition Features
- Global leaderboard showing top players
- Season-long competition with reset option
- User-specific statistics and performance metrics
- Achievement system for milestones

### 4.2 User Interface Requirements

#### 4.2.1 Web Application
- Responsive design for desktop and mobile devices
- Race calendar view with upcoming and past races
- Race detail page with betting options
- User dashboard showing:
  - Current balance
  - Active bets
  - Betting history
  - Performance statistics
- Leaderboard page with global and time-based rankings (e.g., last month)
- Profile management page
- Notification center

#### 4.2.2 Real-time Updates
- Live race status updates
- Real-time bet placement confirmation
- Immediate result processing notifications
- Leaderboard updates

## 5. Non-Functional Requirements

### 5.1 Performance
- Page load time < 2 seconds for all major views
- API response time < 500ms for 95% of requests
- Support for 1,000+ concurrent users
- Database queries optimized for performance

### 5.2 Security
- Secure password storage with hashing
- JWT token-based authentication with HTTP interceptors
- HTTPS for all communications
- Input validation to prevent SQL injection and XSS
- Rate limiting to prevent abuse
- Secure API key management for OpenF1 integration

### 5.3 Reliability
- 99.9% system uptime
- Automatic retry mechanism for failed API calls
- Database backup and recovery procedures
- Error logging and monitoring
- Background job monitoring

### 5.4 Scalability
- Horizontal scaling capability for backend services
- Database optimization for high read/write loads
- Caching strategy for frequently accessed data
- Load balancing for API requests

### 5.5 Usability
- Intuitive and consistent user interface
- Mobile-responsive design
- Accessibility compliance (WCAG 2.1 AA)
- Clear error messages and user feedback
- Help documentation and tooltips

### 5.6 Maintainability
- Modular code architecture with clear separation of concerns
- Comprehensive documentation
- Automated testing (unit, integration, e2e)
- CI/CD pipeline for automated deployments
- Monitoring and alerting system
- Background job monitoring and management

## 6. Explicit Constraints (What NOT to Build)

### 6.1 Functional Limitations
- **No real money betting**: The application uses virtual points only
- **No gambling features**: Not designed for real-money gambling or wagering
- **No live in-race betting**: Bets can only be placed before race start
- **No social features**: No chat, friend systems, or social media integration
- **No multi-language support**: English only (for initial version)
- **No mobile app**: Web application only (responsive design)
- **No advanced analytics**: Basic statistics only, no predictive modeling
- **No custom bet types**: Only predefined bet types as specified
- **No user-to-user transactions**: No point transfers between users

### 6.2 Technical Limitations
- **No offline functionality**: Requires internet connection
- **No browser plugins**: Pure web application, no extensions
- **No legacy browser support**: Modern browsers only (last 2 versions)
- **No custom database**: Uses standard relational database (Microsoft SQL Server)
- **No blockchain**: No cryptocurrency or blockchain integration
- **No AI/ML**: No machine learning for odds calculation or predictions
- **No voice interface**: Text-based UI only
- **No VR/AR**: Traditional 2D interface only

## 7. Technical Context and Integration Points

### 7.1 System Architecture
```
┌───────────────────────────────────────────────────────────────┐
│                        Client (Web App)                        │
│ - Angular SPA with lazy-loaded modules                        │
│ - HTTP interceptors for JWT and error handling                │
└───────────────────┬───────────────────────────┬───────────────┘
                    │                           │
                    ▼                           ▼
┌─────────────────────────────┐ ┌───────────────────────────────┐
│        Frontend Service     │ │        Backend API           │
│ - Angular application       │ │ - ASP.NET Core Web API       │
│ - State management          │ │ - Business logic             │
│ - UI components             │ │ - Authentication             │
│ - Module-based architecture │ │ - RESTful endpoints          │
└───────────────────┬─────────┘ └───────────────┬───────────────┘
                    │                           │
                    └─────────────┬─────────────┘
                                  │
                                  ▼
┌───────────────────────────────────────────────────────────────┐
│                        Database Layer                         │
│ - Microsoft SQL Server                                        │
│ - Entity Framework Core ORM                                   │
│ - Cache for OpenF1 data                                       │
└───────────────┬───────────────────────────────┬───────────────┘
                │                               │
                ▼                               ▼
┌─────────────────────────────┐ ┌───────────────────────────────┐
│        OpenF1 API           │ │        Background Jobs       │
│ - Race data                 │ │ - Race status monitor        │
│ - Championship standings    │ │ - Result processing           │
│ - Driver/team information   │ │ - Data synchronization        │
└─────────────────────────────┘ └───────────────────────────────┘
┌───────────────────────────────────────────────────────────────┐
│                        External Services                     │
│ - Email service                                                │
│ - Authentication service                                       │
│ - Monitoring service                                           │
└───────────────────────────────────────────────────────────────┘
```

### 7.2 Integration Points

#### 7.2.1 OpenF1 API Integration
- **Endpoint**: `https://api.openf1.org`
- **Data Synchronization**:
  - Race calendar (GET `/v1/races`)
  - Race details (GET `/v1/races/{raceId}`)
  - Championship standings (GET `/v1/standings`)
  - Race results (GET `/v1/results`)
  - Driver information (GET `/v1/drivers`)
  - Team information (GET `/v1/teams`)
  - Pit stop data (for fastest pit stop bets)
- **Synchronization Frequency**:
  - Race calendar: Daily
  - Championship standings: After each race
  - Race results: Immediately after race completion
  - Driver/team info: Weekly
- **Error Handling**:
  - Retry mechanism for failed requests
  - Fallback to cached data when API unavailable
  - Notification to administrators on persistent failures

#### 7.2.2 Database Schema
Key entities and relationships:
- **Users**: UserId, Username, Email, PasswordHash, TotalPoints, CreatedAt, LastLogin
- **Races**: RaceId, Name, Circuit, Date, Status (Scheduled, Finished, ResultsProcessed), OpenF1RaceId
- **Bets**: BetId, UserId, RaceId, DriverId_Prediction, FastLap_Prediction, DNF_Prediction, TeamId_Prediction, PitStop_Prediction, PointsAwarded, Status, CreatedAt
- **Results**: ResultId, RaceId, DriverId, Position, Points, FastestLap, PitStopTime
- **Drivers**: DriverId, Name, TeamId, OpenF1DriverId
- **Teams**: TeamId, Name, OpenF1TeamId
- **LeaderboardHistory**: LeaderboardHistoryId, UserId, RaceId, Season, TotalPoints, Rank, CreatedAt

#### 7.2.3 Authentication
- JWT token-based authentication with HTTP interceptors
- Token expiration: 24 hours
- Refresh token mechanism
- Secure cookie storage for tokens

### 7.3 Technology Stack

#### 7.3.1 Frontend
- **Framework**: Angular 17+ (Single Page Application)
- **Language**: TypeScript
- **Styling**: CSS/SCSS
- **State Management**: NgRx (optional)
- **Build Tool**: Angular CLI
- **Testing**: Jasmine, Karma, Protractor
- **Module Structure**:
  - Auth Module (`auth`) - User registration, login, profile management
  - Betting Module (`betting`) - Bet placement, upcoming races
  - Dashboard/Leaderboard Module (`leaderboard`) - Rankings, bet history, statistics
  - F1 Data Module (`race-details`) - Race calendar, team/driver information

#### 7.3.2 Backend
- **Framework**: ASP.NET Core 8+ (Clean Architecture)
- **Language**: C#
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity with JWT
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq
- **Architecture Layers**:
  - Presentation Layer (API) - RESTful endpoints
  - Business Logic Layer (Application/Core) - Game rules, validation, scoring
  - Data/Infrastructure Layer - Database access, OpenF1 client
  - Domain Layer - Entities, enums, domain exceptions

#### 7.3.3 Infrastructure
- **Hosting**: (to be determined)
- **CI/CD**: GitHub Actions/Azure DevOps
- **Monitoring**: Application Insights/Prometheus
- **Logging**: Serilog/ELK Stack
- **Caching**: Redis
- **Background Processing**: BackgroundService or Hangfire

### 7.4 Background Processing

The system implements background jobs to handle time-consuming operations and ensure smooth user experience:

#### 7.4.1 Race Status Monitor (Cyclic Job)
- **Purpose**: Monitor race status and detect when races are completed
- **Frequency**: Runs at regular intervals (e.g., every 5 minutes during race weekends)
- **Functionality**:
  - Checks race status in OpenF1 API
  - Identifies races that have finished but not yet processed
  - Triggers result processing for completed races
  - Updates race status in database (Scheduled → Finished → ResultsProcessed)

#### 7.4.2 Result Processing Job
- **Purpose**: Process race results and update user bets and leaderboards
- **Trigger**: Activated by Race Status Monitor when race completion is detected
- **Functionality**:
  - Retrieves official results from OpenF1 API
  - Processes all pending bets for the race
  - Calculates points for winning bets (including partial wins)
  - Updates user balances and total points
  - Updates leaderboard with current rankings
  - Creates historical records in LeaderboardHistory
  - Sends notifications to users about bet outcomes
  - Updates race status to ResultsProcessed

#### 7.4.3 Data Synchronization Jobs
- **Race Calendar Sync**: Daily synchronization of upcoming races
- **Championship Standings Sync**: Updates after each race completion
- **Driver/Team Info Sync**: Weekly synchronization of driver and team data
- **Error Handling**: Automatic retries, fallback to cached data, admin notifications

## 8. Acceptance Tests

### 8.1 User Management Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| User Registration | New user registers with valid email and password | User account created, verification email sent, initial balance (10,000 points) set |
| User Registration - Invalid Email | User attempts to register with invalid email format | Registration rejected, appropriate error message displayed |
| User Registration - Weak Password | User attempts to register with weak password | Registration rejected, password requirements displayed |
| User Login | Registered user logs in with correct credentials | User authenticated, JWT token issued, dashboard displayed |
| User Login - Invalid Credentials | User attempts to log in with incorrect credentials | Login rejected, appropriate error message displayed |
| Password Reset | User requests password reset | Password reset email sent with secure link |
| Password Reset - Invalid Email | User requests password reset with unregistered email | No email sent, generic message displayed (security) |
| Profile Update | User updates profile information | Profile updated, changes reflected in UI |

### 8.2 Betting System Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Place Bet - Valid | User places bet with sufficient balance | Bet recorded, balance deducted, confirmation shown |
| Place Bet - Insufficient Balance | User attempts to bet more than available balance | Bet rejected, error message displayed |
| Place Bet - After Race Start | User attempts to bet after race has started | Bet rejected, appropriate error message displayed |
| Cancel Bet - Before Race | User cancels bet before race start | Bet cancelled, points refunded to balance |
| Cancel Bet - After Race Start | User attempts to cancel bet after race start | Cancellation rejected, error message displayed |
| Bet History | User views betting history | List of all bets displayed with status and details |
| Bet Types | User views available bet types for a race | All predefined bet types (TOP 3, winner, podium, fastest lap, pit stop, DNF, etc.) displayed with descriptions |

### 8.3 Race Data Integration Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Race Calendar Sync | System synchronizes race calendar with OpenF1 API | All upcoming races imported with correct details |
| Championship Standings Sync | System synchronizes standings after race | Standings updated with latest results |
| Race Results Sync | System processes race results after completion | Results imported, bets processed, leaderboard updated |
| API Failure Handling | OpenF1 API becomes unavailable | System uses cached data, notifies admin, retries later |
| Data Validation | OpenF1 API returns invalid data | System rejects invalid data, logs error, notifies admin |

### 8.4 Result Processing Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Winning Bet Processing | User has winning bet on race winner | Points awarded based on odds, balance updated, notification sent |
| Losing Bet Processing | User has losing bet on race winner | Bet marked as lost, no points awarded |
| Partial Win Processing | User has bet on podium finishers (gets 2/3 correct) | Partial points awarded based on correct predictions |
| Fastest Lap Processing | User has bet on fastest lap winner | Points awarded if prediction correct |
| Fastest Pit Stop Processing | User has bet on team with fastest pit stop | Points awarded if prediction correct |
| DNF Processing | User has bet on number of DNFs | Points awarded if prediction correct |
| Head-to-Head Processing | User has bet on driver A finishing ahead of driver B | Points awarded if prediction correct |
| Leaderboard Update | Multiple users have winning/losing bets | Leaderboard updated with new rankings, historical records created |
| Notification System | User has winning bet | Notification sent to user with results |

### 8.5 Competition Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Leaderboard Display | User views leaderboard | Top players displayed with points and rankings, time-based filters available |
| User Statistics | User views personal statistics | Performance metrics displayed (win rate, ROI, etc.) |
| Season Reset | Administrator triggers season reset | All user balances reset to initial amount (10,000 points), leaderboard cleared |
| Achievement Unlock | User reaches betting milestone | Achievement awarded, notification displayed |

### 8.6 Non-Functional Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Performance - High Load | 1,000 concurrent users accessing system | All requests processed within 2 seconds, no errors |
| Security - SQL Injection | Attempt SQL injection in login form | Request rejected, error logged, no data compromised |
| Security - XSS | Attempt XSS attack in user profile | Malicious script sanitized, no execution |
| Security - API Abuse | Rapid successive API requests | Rate limiting applied, requests rejected |
| Accessibility - Screen Reader | Navigate application using screen reader | All elements properly announced, navigation possible |
| Mobile Responsiveness | View application on mobile device | All elements properly displayed, touch targets appropriate size |
| Browser Compatibility | Test on latest 2 versions of Chrome, Firefox, Edge | Application functions correctly on all browsers |
| Background Job Monitoring | Verify background jobs are running | Jobs execute as scheduled, errors logged and notified |

## 9. Glossary

| Term | Definition |
|------|------------|
| **Virtual Points** | In-game currency used for betting, not convertible to real money |
| **Bet Type** | Category of bet (e.g., race winner, podium, fastest lap, fastest pit stop, DNF) |
| **Odds** | Multiplier applied to bet amount to determine winnings |
| **OpenF1 API** | Public API providing Formula 1 race data and statistics |
| **Race Status** | Current state of a race (Scheduled, Finished, ResultsProcessed) |
| **Bet Status** | Current state of a bet (Pending, Won, Lost, Cancelled) |
| **Leaderboard** | Ranking of users based on their total virtual points |
| **Season** | Competitive period (e.g., calendar year or F1 season) |
| **JWT** | JSON Web Token - secure authentication mechanism |
| **ROI** | Return on Investment - measure of betting profitability |
| **DNF** | Did Not Finish - drivers who didn't complete the race |
| **Pit Stop** | A stop in the pits during which the team will change tires and make adjustments |
| **Partial Win** | Bet where user correctly predicts some but not all outcomes (e.g., 2 out of 3 podium finishers) |