# F1 Betting Game - Technical Implementation Plan

## 1. Introduction

This document outlines the technical implementation plan for the F1 Betting Game system based on the comprehensive specification. The plan covers design documentation, system model classes, data access layer, business logic layer, and unit testing.

## 2. Current State Analysis

### 2.1 Existing Project Structure

The project follows a Clean Architecture approach with the following layers:

- **Presentation Layer**: Angular SPA and ASP.NET Core Web API
- **Application Layer**: Business logic services and DTOs
- **Domain Layer**: Core entities and enums
- **Infrastructure Layer**: Data persistence and external integrations

### 2.2 Existing Components

- **Domain Entities**: User, Bet, Race, Driver, Team
- **Enums**: BetStatus, RaceStatus
- **Services**: BettingService, RaceService, UserService
- **Repositories**: Generic Repository pattern
- **API Controllers**: AuthController, BetsController, RacesController
- **External Integration**: OpenF1Client

## 3. Implementation Plan

### 3.1 Phase 1: Design Documentation with UML Diagrams

#### 3.1.1 Class Diagram Enhancement

**Tasks:**
- [ ] Extend existing class diagram to include all bet types and relationships
- [ ] Add missing entities: Result, LeaderboardHistory, Notification
- [ ] Document all attributes, methods, and relationships
- [ ] Include inheritance and composition relationships

**Expected Output:**
- Updated `docs/class_diagram.puml` with complete domain model
- PlantUML source code with proper styling and organization

#### 3.1.2 Sequence Diagrams

**Tasks:**
- [ ] Create sequence diagram for user registration and authentication
- [ ] Create sequence diagram for bet placement workflow
- [ ] Create sequence diagram for race result processing
- [ ] Create sequence diagram for leaderboard update process

**Expected Output:**
- `docs/sequence_diagram_user_registration.puml`
- `docs/sequence_diagram_bet_placement.puml`
- `docs/sequence_diagram_race_result_processing.puml`
- `docs/sequence_diagram_leaderboard_update.puml`

#### 3.1.3 Component and Deployment Diagrams

**Tasks:**
- [ ] Create component diagram showing major system components
- [ ] Create deployment diagram showing infrastructure layout
- [ ] Document integration points with external services

**Expected Output:**
- `docs/component_diagram.puml`
- `docs/deployment_diagram.puml`

### 3.2 Phase 2: System Model Classes

#### 3.2.1 Domain Entities

**Tasks:**
- [ ] Enhance existing entities with additional properties and methods
- [ ] Add new entities: Result, LeaderboardHistory, Notification
- [ ] Implement validation logic in entity constructors
- [ ] Add domain-specific business rules

**Entities to Implement/Enhance:**

1. **User** (Enhance)
   - Add: ProfileImageUrl, LastLogin, IsActive, IsAdmin
   - Methods: AddPoints(), DeductPoints(), HasSufficientBalance()

2. **Bet** (Enhance)
   - Add: BetType, Odds, PotentialWinnings
   - Methods: CalculatePotentialWinnings(), ValidateBet()

3. **Race** (Enhance)
   - Add: Circuit, Country, OpenF1RaceId, Season
   - Methods: CanPlaceBets(), IsRaceFinished()

4. **Driver** (Enhance)
   - Add: Number, Country, OpenF1DriverId
   - Methods: GetFullName()

5. **Team** (Enhance)
   - Add: Country, OpenF1TeamId, Base
   - Methods: GetDrivers()

6. **Result** (New)
   - Properties: Id, RaceId, DriverId, Position, Points, FastestLap, PitStopTime
   - Methods: IsPodiumFinish(), IsPointsFinish()

7. **LeaderboardHistory** (New)
   - Properties: Id, UserId, RaceId, Season, TotalPoints, Rank, CreatedAt
   - Methods: None

8. **Notification** (New)
   - Properties: Id, UserId, Title, Message, IsRead, CreatedAt
   - Methods: MarkAsRead()

#### 3.2.2 Enums

**Tasks:**
- [ ] Add missing enums for bet types and other domain concepts

**Enums to Add:**

1. **BetType**
   - RaceWinner
   - PodiumFinish
   - Top10Finish
   - FastestLap
   - FastestPitStop
   - DNFCount
   - DriverVsDriver
   - TeamVsTeam

2. **NotificationType**
   - BetPlaced
   - BetWon
   - BetLost
   - RaceResultProcessed
   - SystemMessage

#### 3.2.3 Value Objects

**Tasks:**
- [ ] Create value objects for domain concepts

**Value Objects to Create:**

1. **Money** (for points/balance management)
2. **RaceDate** (with validation logic)
3. **Odds** (for bet calculations)

### 3.3 Phase 3: Data Access Layer

#### 3.3.1 Repository Interfaces

**Tasks:**
- [ ] Create specific repository interfaces extending IRepository
- [ ] Define domain-specific query methods

**Interfaces to Create:**

1. **IUserRepository**
   - GetByEmailAsync(string email)
   - GetByUsernameAsync(string username)
   - GetLeaderboardAsync(int limit, int season)

2. **IBetRepository**
   - GetUserBetsAsync(int userId, BetStatus? status)
   - GetPendingBetsForRaceAsync(int raceId)
   - GetBetStatisticsAsync(int userId)

3. **IRaceRepository**
   - GetUpcomingRacesAsync()
   - GetRaceWithResultsAsync(int raceId)
   - GetCurrentSeasonRacesAsync()

4. **IResultRepository**
   - GetRaceResultsAsync(int raceId)
   - GetDriverResultsAsync(int driverId, int season)

5. **INotificationRepository**
   - GetUnreadNotificationsAsync(int userId)
   - MarkAsReadAsync(int notificationId)

#### 3.3.2 Repository Implementations

**Tasks:**
- [ ] Implement repository classes using Entity Framework Core
- [ ] Add proper error handling and logging
- [ ] Implement caching strategies for frequently accessed data

**Implementation Details:**

1. **UserRepository**
   - Implement leaderboard queries with proper indexing
   - Add email/username uniqueness validation

2. **BetRepository**
   - Implement complex queries for bet statistics
   - Add filtering by bet type and status

3. **RaceRepository**
   - Implement race status filtering
   - Add OpenF1 API integration for race data

4. **ResultRepository**
   - Implement result processing queries
   - Add performance optimization for leaderboard calculations

#### 3.3.3 Database Context Enhancement

**Tasks:**
- [ ] Extend AppDbContext with new DbSets
- [ ] Configure entity relationships and constraints
- [ ] Add database indexes for performance

**Enhancements:**

1. Add DbSets for new entities:
   ```csharp
   public DbSet<Result> Results { get; set; }
   public DbSet<LeaderboardHistory> LeaderboardHistories { get; set; }
   public DbSet<Notification> Notifications { get; set; }
   ```

2. Configure relationships in OnModelCreating:
   ```csharp
   modelBuilder.Entity<Result>()
       .HasOne(r => r.Race)
       .WithMany()
       .HasForeignKey(r => r.RaceId);

   modelBuilder.Entity<Result>()
       .HasOne(r => r.Driver)
       .WithMany()
       .HasForeignKey(r => r.DriverId);
   ```

3. Add indexes for performance:
   ```csharp
   modelBuilder.Entity<Bet>()
       .HasIndex(b => new { b.UserId, b.Status });

   modelBuilder.Entity<Race>()
       .HasIndex(r => r.Status);
   ```

### 3.4 Phase 4: Business Logic Layer

#### 3.4.1 Service Interfaces

**Tasks:**
- [ ] Extend existing service interfaces with new methods
- [ ] Create new service interfaces for additional functionality

**Interfaces to Extend/Create:**

1. **IBettingService** (Extend)
   - PlaceBetAsync(PlaceBetDto betDto)
   - CancelBetAsync(int betId)
   - ProcessRaceResultsAsync(int raceId)
   - CalculateWinningsAsync(Bet bet, RaceResult result)

2. **IRaceService** (Extend)
   - SyncRaceDataFromOpenF1Async()
   - GetUpcomingRacesWithOddsAsync()
   - UpdateRaceStatusAsync(int raceId, RaceStatus status)

3. **IUserService** (Extend)
   - GetUserLeaderboardPositionAsync(int userId)
   - GetUserStatisticsAsync(int userId)
   - UpdateUserPointsAsync(int userId, decimal amount)

4. **INotificationService** (New)
   - CreateNotificationAsync(int userId, string title, string message)
   - MarkNotificationAsReadAsync(int notificationId)
   - GetUnreadNotificationsAsync(int userId)

5. **ILeaderboardService** (New)
   - UpdateLeaderboardAsync(int raceId)
   - GetCurrentLeaderboardAsync(int limit)
   - GetSeasonLeaderboardAsync(int season, int limit)

#### 3.4.2 Service Implementations

**Tasks:**
- [ ] Implement service methods with proper business logic
- [ ] Add validation and error handling
- [ ] Implement transaction management

**Implementation Details:**

1. **BettingService**
   - Implement bet placement with validation
   - Add odds calculation logic
   - Implement result processing with different bet types

2. **RaceService**
   - Implement OpenF1 API synchronization
   - Add race status monitoring
   - Implement race result processing

3. **UserService**
   - Implement points management
   - Add leaderboard position calculation
   - Implement user statistics aggregation

4. **NotificationService**
   - Implement notification creation
   - Add notification delivery mechanisms
   - Implement read status management

5. **LeaderboardService**
   - Implement leaderboard calculation
   - Add historical tracking
   - Implement season management

#### 3.4.3 Business Logic Patterns

**Tasks:**
- [ ] Implement domain-driven design patterns
- [ ] Add validation and error handling
- [ ] Implement transaction management

**Patterns to Implement:**

1. **Domain Events**
   - BetPlacedEvent
   - RaceFinishedEvent
   - PointsAwardedEvent

2. **Specification Pattern**
   - For complex query filtering
   - For bet validation rules

3. **Unit of Work**
   - For transaction management
   - For atomic operations

### 3.5 Phase 5: Unit Testing

#### 3.5.1 Test Strategy

**Tasks:**
- [ ] Define test coverage goals (80%+ for business logic)
- [ ] Set up test infrastructure (xUnit, Moq)
- [ ] Create test data builders

**Test Coverage Goals:**
- 80%+ code coverage for business logic layer
- 100% coverage for critical paths (bet placement, result processing)
- Test all bet types and edge cases

#### 3.5.2 Test Cases by Component

**BettingService Tests:**
- [ ] PlaceBet_WithValidData_Succeeds
- [ ] PlaceBet_WithInsufficientBalance_Fails
- [ ] PlaceBet_AfterRaceStart_Fails
- [ ] CancelBet_BeforeRaceStart_Succeeds
- [ ] CancelBet_AfterRaceStart_Fails
- [ ] ProcessRaceResults_WithWinningBets_UpdatesPoints
- [ ] ProcessRaceResults_WithPartialWins_UpdatesPoints
- [ ] ProcessRaceResults_WithLosingBets_NoPointsUpdate

**RaceService Tests:**
- [ ] SyncRaceData_FromOpenF1_Succeeds
- [ ] SyncRaceData_WithApiFailure_UsesCache
- [ ] UpdateRaceStatus_ToFinished_TriggersProcessing
- [ ] GetUpcomingRaces_ReturnsOnlyFutureRaces
- [ ] GetRaceWithResults_ReturnsCompleteData

**UserService Tests:**
- [ ] GetUserLeaderboardPosition_ReturnsCorrectPosition
- [ ] UpdateUserPoints_WithPositiveAmount_Succeeds
- [ ] UpdateUserPoints_WithNegativeAmount_Succeeds
- [ ] GetUserStatistics_ReturnsAccurateData
- [ ] GetUserByEmail_ReturnsCorrectUser

**LeaderboardService Tests:**
- [ ] UpdateLeaderboard_AfterRace_UpdatesRankings
- [ ] GetCurrentLeaderboard_ReturnsTopPlayers
- [ ] GetSeasonLeaderboard_ReturnsSeasonData
- [ ] UpdateLeaderboard_WithTie_HandlesTieCorrectly

**NotificationService Tests:**
- [ ] CreateNotification_WithValidData_Succeeds
- [ ] MarkNotificationAsRead_UpdatesStatus
- [ ] GetUnreadNotifications_ReturnsOnlyUnread
- [ ] CreateNotification_ForMultipleUsers_Succeeds

#### 3.5.3 Test Data Setup

**Tasks:**
- [ ] Create test data builders for entities
- [ ] Set up in-memory database for integration tests
- [ ] Create mock implementations for external services

**Test Data Builders:**
- UserBuilder
- BetBuilder
- RaceBuilder
- ResultBuilder

#### 3.5.4 Test Execution and Reporting

**Tasks:**
- [ ] Set up CI/CD pipeline for test execution
- [ ] Configure test coverage reporting
- [ ] Implement test result notifications

**Test Infrastructure:**
- GitHub Actions/Azure DevOps pipeline
- Coverage reporting with Coverlet
- Test result visualization

## 4. Integration Points and Potential Conflicts

### 4.1 Integration Points

#### 4.1.1 OpenF1 API Integration
- **Integration Point**: Race data synchronization
- **Potential Conflicts**:
  - API rate limiting
  - Data format changes
  - Service unavailability
- **Mitigation**:
  - Implement retry logic with exponential backoff
  - Cache data locally
  - Implement circuit breaker pattern

#### 4.1.2 Database Integration
- **Integration Point**: Entity Framework Core with SQL Server
- **Potential Conflicts**:
  - Schema migration issues
  - Performance bottlenecks
  - Connection pooling problems
- **Mitigation**:
  - Implement proper indexing
  - Use connection resiliency
  - Monitor query performance

#### 4.1.3 Background Processing
- **Integration Point**: Race result processing jobs
- **Potential Conflicts**:
  - Job scheduling conflicts
  - Race condition in data processing
  - Job failure handling
- **Mitigation**:
  - Implement proper locking mechanisms
  - Add comprehensive error handling
  - Implement job monitoring and alerts

### 4.2 Potential Conflicts

#### 4.2.1 Concurrent Bet Processing
- **Conflict**: Multiple users placing bets simultaneously
- **Solution**: Implement optimistic concurrency control
- **Implementation**: Use EF Core concurrency tokens

#### 4.2.2 Race Status Transitions
- **Conflict**: Race status updates during bet placement
- **Solution**: Implement proper transaction isolation
- **Implementation**: Use serializable transactions for critical operations

#### 4.2.3 Points Calculation
- **Conflict**: Inconsistent points calculation across bet types
- **Solution**: Centralize points calculation logic
- **Implementation**: Create PointsCalculator service

## 5. Implementation Timeline

### 5.1 Phase 1: Design Documentation (2 weeks)
- Week 1: UML diagrams creation
- Week 2: Diagram review and refinement

### 5.2 Phase 2: System Model Classes (3 weeks)
- Week 1: Domain entities enhancement
- Week 2: New entities implementation
- Week 3: Value objects and enums

### 5.3 Phase 3: Data Access Layer (4 weeks)
- Week 1: Repository interfaces
- Week 2: Repository implementations
- Week 3: Database context enhancement
- Week 4: Performance optimization

### 5.4 Phase 4: Business Logic Layer (5 weeks)
- Week 1: Service interfaces
- Week 2: BettingService implementation
- Week 3: RaceService and UserService
- Week 4: Notification and Leaderboard services
- Week 5: Integration and testing

### 5.5 Phase 5: Unit Testing (3 weeks)
- Week 1: Test infrastructure setup
- Week 2: Service layer testing
- Week 3: Test coverage analysis and improvements

## 6. Success Criteria

### 6.1 Technical Success Criteria
- [ ] All UML diagrams completed and reviewed
- [ ] All domain entities implemented with proper validation
- [ ] Repository pattern fully implemented
- [ ] Service layer with 80%+ test coverage
- [ ] Integration points properly handled
- [ ] Performance requirements met

### 6.2 Quality Success Criteria
- [ ] Code follows SOLID principles
- [ ] Proper separation of concerns
- [ ] Comprehensive error handling
- [ ] Clean and maintainable code
- [ ] Complete documentation

## 7. Risks and Mitigation

### 7.1 Technical Risks

| Risk | Impact | Mitigation Strategy |
|------|--------|---------------------|
| OpenF1 API changes | High | Implement versioned API client, add comprehensive error handling |
| Database performance issues | Medium | Implement caching, optimize queries, add proper indexing |
| Concurrent data access conflicts | High | Implement proper transaction management and locking strategies |
| Integration complexity | Medium | Use dependency injection, implement clear interfaces |

### 7.2 Schedule Risks

| Risk | Impact | Mitigation Strategy |
|------|--------|---------------------|
| Underestimated complexity | High | Break work into smaller tasks, regular progress reviews |
| Team member availability | Medium | Cross-train team members, document processes |
| External dependencies delays | Low | Identify dependencies early, implement fallback solutions |

## 8. Resources and Tools

### 8.1 Development Tools
- Visual Studio 2022 / VS Code
- PlantUML for diagram creation
- Entity Framework Core
- xUnit / Moq for testing
- GitHub Actions for CI/CD

### 8.2 Documentation Tools
- Markdown for technical documentation
- Swagger for API documentation
- PlantUML for architecture diagrams

### 8.3 Team Resources
- Backend developers (C#, ASP.NET Core)
- Database specialists (SQL Server)
- QA engineers (test automation)
- DevOps engineers (CI/CD pipeline)

## 9. Next Steps

1. **Immediate Actions**:
   - Review and approve this implementation plan
   - Set up project repository and CI/CD pipeline
   - Assign team members to specific components

2. **First Sprint**:
   - Begin with design documentation (UML diagrams)
   - Implement core domain entities
   - Set up basic repository infrastructure

3. **Ongoing**:
   - Regular code reviews
   - Continuous integration and testing
   - Progress tracking against timeline

## 10. Appendix

### 10.1 Glossary
- **BLL**: Business Logic Layer
- **DAL**: Data Access Layer
- **DTO**: Data Transfer Object
- **ORM**: Object-Relational Mapping
- **SOLID**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion

### 10.2 References
- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- Microsoft Documentation for ASP.NET Core and Entity Framework Core