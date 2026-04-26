# Task: Enhance Database Context

## Description
Extend AppDbContext with new DbSets, configure entity relationships and constraints, and add database indexes for performance.

## Requirements
- Add DbSets for new entities: Results, LeaderboardHistories, Notifications
- Configure relationships in OnModelCreating method
- Add database indexes for performance-critical queries
- Configure proper foreign key relationships
- Add data validation constraints

## Acceptance Criteria
- [ ] New DbSets added to AppDbContext
- [ ] Entity relationships properly configured
- [ ] Database indexes added for performance
- [ ] Foreign key relationships configured
- [ ] Data validation constraints implemented

## Deliverables
- Enhanced AppDbContext.cs

## Testing
- Verify database migrations work correctly
- Test entity relationships function properly
- Validate performance improvements from indexing
- Check data integrity constraints

## Dependencies
- New domain entities
- Entity Framework Core

## Notes
- Can be implemented and tested independently
- Reviewable as single change to data access layer