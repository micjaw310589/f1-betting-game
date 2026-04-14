-- ============================================================
-- F1 Betting Application - Database Creation Script
-- Microsoft SQL Server
-- ============================================================
-- This script creates the database and all necessary tables
-- for the F1 Betting Application based on the Domain Layer models
-- ============================================================

-- Create Database (if it doesn't exist)
-- Uncomment the line below if you want to create the database
-- CREATE DATABASE F1BettingAppDb;

-- Use the database
USE F1BettingAppDb;
GO

-- ============================================================
-- DROP TABLES (if they exist - for clean reinstalls)
-- ============================================================
-- Uncomment the section below to drop existing tables
/*
IF OBJECT_ID('dbo.Bets', 'U') IS NOT NULL DROP TABLE dbo.Bets;
IF OBJECT_ID('dbo.Races', 'U') IS NOT NULL DROP TABLE dbo.Races;
IF OBJECT_ID('dbo.Drivers', 'U') IS NOT NULL DROP TABLE dbo.Drivers;
IF OBJECT_ID('dbo.Teams', 'U') IS NOT NULL DROP TABLE dbo.Teams;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO
*/

-- ============================================================
-- CREATE TABLES
-- ============================================================

-- ============================================================
-- Table: Users
-- Description: Stores user account information
-- ============================================================
CREATE TABLE dbo.Users
(
    user_id         INT PRIMARY KEY IDENTITY(1,1),
    username        VARCHAR(100) NOT NULL UNIQUE,
    email           VARCHAR(150) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    points          INT NOT NULL DEFAULT 0,
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Create indexes on Users
CREATE INDEX idx_users_username ON dbo.Users(username);
CREATE INDEX idx_users_email ON dbo.Users(email);
GO

-- ============================================================
-- Table: Teams
-- Description: Stores F1 team information
-- ============================================================
CREATE TABLE dbo.Teams
(
    team_id         INT PRIMARY KEY IDENTITY(1,1),
    name            VARCHAR(100) NOT NULL UNIQUE
);

-- Create indexes on Teams
CREATE INDEX idx_teams_name ON dbo.Teams(name);
GO

-- ============================================================
-- Table: Drivers
-- Description: Stores F1 driver information with team relationship
-- ============================================================
CREATE TABLE dbo.Drivers
(
    driver_id       INT PRIMARY KEY IDENTITY(1,1),
    name            VARCHAR(100) NOT NULL,
    team_id         INT NOT NULL,
    CONSTRAINT FK_Drivers_Teams FOREIGN KEY (team_id) 
        REFERENCES dbo.Teams(team_id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE
);

-- Create indexes on Drivers
CREATE INDEX idx_drivers_team_id ON dbo.Drivers(team_id);
CREATE INDEX idx_drivers_name ON dbo.Drivers(name);
GO

-- ============================================================
-- Table: Races
-- Description: Stores F1 race information
-- ============================================================
CREATE TABLE dbo.Races
(
    race_id         INT PRIMARY KEY IDENTITY(1,1),
    name            VARCHAR(150) NOT NULL,
    date            DATETIME2 NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'Scheduled',
    CONSTRAINT CHK_RaceStatus CHECK (
        status IN ('Scheduled', 'InProgress', 'Finished', 'ResultsProcessed')
    )
);

-- Create indexes on Races
CREATE INDEX idx_races_status ON dbo.Races(status);
CREATE INDEX idx_races_date ON dbo.Races(date);
GO

-- ============================================================
-- Table: Bets
-- Description: Stores user bets on races and drivers
-- ============================================================
CREATE TABLE dbo.Bets
(
    bet_id          INT PRIMARY KEY IDENTITY(1,1),
    user_id         INT NOT NULL,
    race_id         INT NOT NULL,
    driver_id       INT NOT NULL,
    amount          DECIMAL(10,2) NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'Pending',
    created_at      DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Bets_Users FOREIGN KEY (user_id) 
        REFERENCES dbo.Users(user_id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
    CONSTRAINT FK_Bets_Races FOREIGN KEY (race_id) 
        REFERENCES dbo.Races(race_id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
    CONSTRAINT FK_Bets_Drivers FOREIGN KEY (driver_id) 
        REFERENCES dbo.Drivers(driver_id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
    CONSTRAINT CHK_BetAmount CHECK (amount > 0),
    CONSTRAINT CHK_BetStatus CHECK (
        status IN ('Pending', 'Won', 'Lost', 'Canceled')
    )
);

-- Create indexes on Bets for optimal query performance
CREATE INDEX idx_bets_user_id ON dbo.Bets(user_id);
CREATE INDEX idx_bets_race_id ON dbo.Bets(race_id);
CREATE INDEX idx_bets_driver_id ON dbo.Bets(driver_id);
CREATE INDEX idx_bets_status ON dbo.Bets(status);
CREATE INDEX idx_bets_created_at ON dbo.Bets(created_at);
CREATE INDEX idx_bets_user_status ON dbo.Bets(user_id, status);
GO

-- ============================================================
-- VERIFY TABLE CREATION
-- ============================================================
PRINT '=== Database Tables Created Successfully ===';
PRINT 'Tables:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO

-- ============================================================
-- DISPLAY TABLE SCHEMAS
-- ============================================================
PRINT '';
PRINT '=== Tables Structure ===';
EXEC sp_help 'dbo.Users';
EXEC sp_help 'dbo.Teams';
EXEC sp_help 'dbo.Drivers';
EXEC sp_help 'dbo.Races';
EXEC sp_help 'dbo.Bets';
GO
