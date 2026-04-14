-- ============================================================
-- F1 Betting Application - Sample Data Script
-- Microsoft SQL Server
-- ============================================================
-- This script inserts sample/test data into the database
-- for development and testing purposes
-- ============================================================

USE F1BettingAppDb;
GO

-- ============================================================
-- SAMPLE DATA - USERS
-- ============================================================
INSERT INTO dbo.Users (username, email, password_hash, points, created_at)
VALUES
    ('testuser1', 'testuser1@example.com', 'hash_password_123', 1000, GETDATE()),
    ('testuser2', 'testuser2@example.com', 'hash_password_456', 750, GETDATE()),
    ('testuser3', 'testuser3@example.com', 'hash_password_789', 500, GETDATE()),
    ('jsmith', 'john.smith@example.com', 'hash_password_abc', 2000, GETDATE()),
    ('mjones', 'mary.jones@example.com', 'hash_password_def', 1500, GETDATE());

PRINT 'Inserted 5 sample users';
GO

-- ============================================================
-- SAMPLE DATA - TEAMS
-- ============================================================
INSERT INTO dbo.Teams (name)
VALUES
    ('Red Bull Racing'),
    ('Mercedes'),
    ('Ferrari'),
    ('McLaren'),
    ('Alpine'),
    ('Aston Martin'),
    ('Williams');

PRINT 'Inserted 7 sample teams';
GO

-- ============================================================
-- SAMPLE DATA - DRIVERS
-- ============================================================
INSERT INTO dbo.Drivers (name, team_id)
VALUES
    ('Max Verstappen', 1),
    ('Sergio Perez', 1),
    ('Lewis Hamilton', 2),
    ('George Russell', 2),
    ('Charles Leclerc', 3),
    ('Carlos Sainz', 3),
    ('Lando Norris', 4),
    ('Oscar Piastri', 4),
    ('Esteban Ocon', 5),
    ('Pierre Gasly', 5),
    ('Fernando Alonso', 6),
    ('Lance Stroll', 6),
    ('Alex Albon', 7),
    ('Logan Sargeant', 7);

PRINT 'Inserted 14 sample drivers';
GO

-- ============================================================
-- SAMPLE DATA - RACES
-- ============================================================
INSERT INTO dbo.Races (name, date, status)
VALUES
    ('Bahrain Grand Prix', '2024-03-02 15:00:00', 'Finished'),
    ('Saudi Arabian Grand Prix', '2024-03-09 20:00:00', 'Finished'),
    ('Australian Grand Prix', '2024-03-24 04:00:00', 'Finished'),
    ('Monaco Grand Prix', '2024-05-26 15:00:00', 'Scheduled'),
    ('Canadian Grand Prix', '2024-06-09 19:00:00', 'Scheduled'),
    ('Silverstone Grand Prix', '2024-07-07 15:00:00', 'Scheduled'),
    ('Hungarian Grand Prix', '2024-07-21 15:00:00', 'Scheduled'),
    ('Belgian Grand Prix', '2024-08-04 15:00:00', 'Scheduled');

PRINT 'Inserted 8 sample races';
GO

-- ============================================================
-- SAMPLE DATA - BETS
-- ============================================================
INSERT INTO dbo.Bets (user_id, race_id, driver_id, amount, status, created_at)
VALUES
    (1, 1, 1, 50.00, 'Won', DATEADD(DAY, -2, GETDATE())),
    (1, 1, 2, 30.00, 'Lost', DATEADD(DAY, -2, GETDATE())),
    (1, 2, 3, 100.00, 'Won', DATEADD(DAY, -1, GETDATE())),
    (2, 1, 5, 40.00, 'Lost', DATEADD(DAY, -2, GETDATE())),
    (2, 2, 7, 75.00, 'Pending', DATEADD(DAY, -1, GETDATE())),
    (2, 3, 1, 60.00, 'Pending', DATEADD(DAY, -1, GETDATE())),
    (3, 1, 3, 25.00, 'Won', DATEADD(DAY, -2, GETDATE())),
    (3, 2, 5, 50.00, 'Won', DATEADD(DAY, -1, GETDATE())),
    (4, 1, 1, 200.00, 'Won', DATEADD(DAY, -2, GETDATE())),
    (4, 3, 7, 150.00, 'Pending', DATEADD(DAY, -1, GETDATE())),
    (5, 2, 3, 80.00, 'Won', DATEADD(DAY, -1, GETDATE())),
    (5, 3, 1, 120.00, 'Pending', DATEADD(DAY, -1, GETDATE()));

PRINT 'Inserted 12 sample bets';
GO

-- ============================================================
-- VERIFY SAMPLE DATA
-- ============================================================
PRINT '';
PRINT '=== Sample Data Inserted Successfully ===';
PRINT '';

PRINT 'Users:';
SELECT * FROM dbo.Users;
PRINT '';

PRINT 'Teams:';
SELECT * FROM dbo.Teams;
PRINT '';

PRINT 'Drivers:';
SELECT * FROM dbo.Drivers;
PRINT '';

PRINT 'Races:';
SELECT * FROM dbo.Races;
PRINT '';

PRINT 'Bets:';
SELECT * FROM dbo.Bets;
PRINT '';

-- ============================================================
-- SUMMARY STATISTICS
-- ============================================================
PRINT '=== Data Summary ===';
PRINT CONCAT('Total Users: ', (SELECT COUNT(*) FROM dbo.Users));
PRINT CONCAT('Total Teams: ', (SELECT COUNT(*) FROM dbo.Teams));
PRINT CONCAT('Total Drivers: ', (SELECT COUNT(*) FROM dbo.Drivers));
PRINT CONCAT('Total Races: ', (SELECT COUNT(*) FROM dbo.Races));
PRINT CONCAT('Total Bets: ', (SELECT COUNT(*) FROM dbo.Bets));
PRINT CONCAT('Total Points Wagered: ', (SELECT SUM(amount) FROM dbo.Bets));
PRINT CONCAT('Pending Bets: ', (SELECT COUNT(*) FROM dbo.Bets WHERE status = 'Pending'));
PRINT CONCAT('Won Bets: ', (SELECT COUNT(*) FROM dbo.Bets WHERE status = 'Won'));
PRINT CONCAT('Lost Bets: ', (SELECT COUNT(*) FROM dbo.Bets WHERE status = 'Lost'));
GO
