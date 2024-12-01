-- SQL Server Script for creating Tadawi Database
-- ===========================================

-- Check if database exists and drop it if it does
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'tadawi_db')
BEGIN
    USE master;
    ALTER DATABASE tadawi_db SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE tadawi_db;
END
GO

-- Create the database
CREATE DATABASE tadawi_db
GO

-- Use the newly created database
USE tadawi_db
GO

-- Set database configurations
ALTER DATABASE tadawi_db
SET ANSI_NULL_DEFAULT ON,
    ANSI_NULLS ON,
    ANSI_PADDING ON,
    ANSI_WARNINGS ON,
    ARITHABORT ON,
    CONCAT_NULL_YIELDS_NULL ON,
    QUOTED_IDENTIFIER ON
GO

-- Basic configuration for recovery model and compatibility
ALTER DATABASE tadawi_db
SET RECOVERY SIMPLE,
    COMPATIBILITY_LEVEL = 150  -- SQL Server 2019
GO

-- Add a comment to indicate where to add future table definitions
-- ========================
-- TABLE DEFINITIONS
-- ========================
-- Add your table creation scripts here

-- ========================
-- VIEW DEFINITIONS
-- ========================
-- Add your view creation scripts here

-- ========================
-- STORED PROCEDURES
-- ========================
-- Add your stored procedure creation scripts here

-- ========================
-- FUNCTIONS
-- ========================
-- Add your function creation scripts here

-- ========================
-- TRIGGERS
-- ========================
-- Add your trigger creation scripts here
