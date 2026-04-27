# Task 05: Implement UsersController

## Overview
Implement the UsersController with user profile management functionality including profile retrieval and updates.

## Objectives
- Provide user profile management through REST API
- Enable users to view and update their profiles
- Implement proper authorization and validation
- Support user statistics and history retrieval

## Scope
### In Scope
- UsersController implementation
- User profile endpoints
- Profile update functionality
- User statistics retrieval
- Authorization and validation
- Integration with existing services

### Out of Scope
- User registration (handled by AuthController)
- Password management
- User deletion functionality
- Admin user management features

## Implementation Steps

### 1. Complete UsersController Implementation
- [ ] Implement `GET /api/users/me` endpoint for current user profile
- [ ] Implement `PUT /api/users/me` endpoint for profile updates
- [ ] Implement `GET /api/users/me/statistics` endpoint for user stats
- [ ] Implement `GET /api/users/me/history` endpoint for bet history
- [ ] Add proper authorization checks
- [ ] Implement Swagger documentation
- [ ] Add consistent error handling

### 2. Enhance UserService for Profile Management
- [ ] Implement `GetUserProfileAsync(string userId)` method
- [ ] Implement `UpdateUserProfileAsync(string userId, UpdateProfileDto dto)` method
- [ ] Implement `GetUserStatisticsAsync(string userId)` method
- [ ] Implement `GetUserBetHistoryAsync(string userId)` method
- [ ] Add proper authorization checks

### 3. Add User Profile DTOs
- [ ] Create `UserProfileDto` for profile data
- [ ] Create `UpdateProfileDto` with validation
- [ ] Create `UserStatisticsDto` for user stats
- [ ] Create `UserBetHistoryDto` for bet history
- [ ] Add proper data annotations

### 4. Implement Authorization
- [ ] Add authorization attributes to all endpoints
- [ ] Implement user ownership validation
- [ ] Add role-based access control if needed
- [ ] Ensure proper error responses for unauthorized access

### 5. Integrate with Existing Services
- [ ] Add integration with BettingService for bet history
- [ ] Add integration with LeaderboardService for statistics
- [ ] Ensure proper error handling for service calls
- [ ] Implement data aggregation for statistics

## Testing
- [ ] Test profile retrieval for authenticated users
- [ ] Test profile updates with valid data
- [ ] Test profile updates with invalid data
- [ ] Test statistics retrieval
- [ ] Test bet history retrieval
- [ ] Test authorization and ownership validation
- [ ] Test error handling for all endpoints

## Deliverables
- Fully implemented `UsersController.cs`
- Enhanced `UserService.cs` with profile methods
- User profile DTOs in `DTOs/` directory
- Comprehensive test coverage for user endpoints
- Updated integration with other services

## Success Criteria
- All user profile endpoints functional
- Profile retrieval and updates working
- Statistics and history retrieval implemented
- Proper authorization and validation in place
- All endpoints properly documented
- Comprehensive error handling implemented

## Review Checklist
- [ ] All user endpoints implemented and tested
- [ ] Authorization properly configured
- [ ] Profile updates working correctly
- [ ] Statistics calculation accurate
- [ ] Error handling covers all edge cases
- [ ] Swagger documentation complete
- [ ] Test coverage meets requirements