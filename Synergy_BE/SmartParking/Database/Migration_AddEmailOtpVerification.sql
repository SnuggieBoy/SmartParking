-- =====================================================
-- Migration: Add Email OTP Verification System
-- Author: System
-- Date: 2026-01-25
-- Description: 
--   - Creates EmailOtps table for OTP verification
--   - Adds EmailConfirmed column to Users table
--   - Adds indexes for performance
-- =====================================================

USE [SmartParkingDB]
GO

PRINT 'Starting Migration: Email OTP Verification System'
GO

-- =====================================================
-- STEP 1: Add EmailConfirmed column to Users table
-- =====================================================

IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') 
    AND name = 'EmailConfirmed'
)
BEGIN
    PRINT 'Adding EmailConfirmed column to Users table...'
    
    ALTER TABLE [dbo].[Users]
    ADD [EmailConfirmed] BIT NOT NULL DEFAULT 0
    
    PRINT '✓ EmailConfirmed column added successfully'
END
ELSE
BEGIN
    PRINT '  EmailConfirmed column already exists, skipping...'
END
GO

-- =====================================================
-- STEP 2: Create EmailOtps table
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailOtps' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT 'Creating EmailOtps table...'
    
    CREATE TABLE [dbo].[EmailOtps] (
        [OtpId]                   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Email]                   NVARCHAR(255) NOT NULL,
        [OtpCode]                 NVARCHAR(6) NOT NULL,
        [ExpiredAt]               DATETIME2(7) NOT NULL,
        [IsUsed]                  BIT NOT NULL DEFAULT 0,
        [CreatedAt]               DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [TemporaryPasswordHash]   NVARCHAR(MAX) NULL,
        [TemporaryFullName]       NVARCHAR(255) NULL,
        [TemporaryPhone]          NVARCHAR(20) NULL
    )
    
    PRINT '✓ EmailOtps table created successfully'
END
ELSE
BEGIN
    PRINT '  EmailOtps table already exists, skipping...'
END
GO

-- =====================================================
-- STEP 3: Create indexes for performance
-- =====================================================

-- Index for EmailConfirmed in Users
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Users_EmailConfirmed' 
    AND object_id = OBJECT_ID(N'[dbo].[Users]')
)
BEGIN
    PRINT 'Creating index IX_Users_EmailConfirmed...'
    
    CREATE NONCLUSTERED INDEX [IX_Users_EmailConfirmed]
    ON [dbo].[Users] ([EmailConfirmed])
    INCLUDE ([UserId], [Email])
    
    PRINT '✓ Index IX_Users_EmailConfirmed created'
END
ELSE
BEGIN
    PRINT '  Index IX_Users_EmailConfirmed already exists, skipping...'
END
GO

-- Index for Email lookup in EmailOtps
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_Email_IsUsed' 
    AND object_id = OBJECT_ID(N'[dbo].[EmailOtps]')
)
BEGIN
    PRINT 'Creating index IX_EmailOtps_Email_IsUsed...'
    
    CREATE NONCLUSTERED INDEX [IX_EmailOtps_Email_IsUsed]
    ON [dbo].[EmailOtps] ([Email], [IsUsed])
    INCLUDE ([OtpCode], [ExpiredAt], [CreatedAt])
    WHERE [IsUsed] = 0
    
    PRINT '✓ Index IX_EmailOtps_Email_IsUsed created'
END
ELSE
BEGIN
    PRINT '  Index IX_EmailOtps_Email_IsUsed already exists, skipping...'
END
GO

-- Index for cleanup of expired OTPs
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_ExpiredAt' 
    AND object_id = OBJECT_ID(N'[dbo].[EmailOtps]')
)
BEGIN
    PRINT 'Creating index IX_EmailOtps_ExpiredAt...'
    
    CREATE NONCLUSTERED INDEX [IX_EmailOtps_ExpiredAt]
    ON [dbo].[EmailOtps] ([ExpiredAt])
    WHERE [IsUsed] = 0
    
    PRINT '✓ Index IX_EmailOtps_ExpiredAt created'
END
ELSE
BEGIN
    PRINT '  Index IX_EmailOtps_ExpiredAt already exists, skipping...'
END
GO

-- =====================================================
-- STEP 4: Create cleanup stored procedure (optional but recommended)
-- =====================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'sp_CleanupExpiredOtps' AND type = 'P')
BEGIN
    DROP PROCEDURE [dbo].[sp_CleanupExpiredOtps]
    PRINT 'Dropped existing sp_CleanupExpiredOtps'
END
GO

PRINT 'Creating stored procedure sp_CleanupExpiredOtps...'
GO

CREATE PROCEDURE [dbo].[sp_CleanupExpiredOtps]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT
    
    DELETE FROM [dbo].[EmailOtps]
    WHERE [ExpiredAt] < DATEADD(DAY, -1, GETUTCDATE())
    
    SET @DeletedCount = @@ROWCOUNT
    
    PRINT 'Cleaned up ' + CAST(@DeletedCount AS VARCHAR(10)) + ' expired OTP records'
    
    RETURN @DeletedCount
END
GO

PRINT '✓ Stored procedure sp_CleanupExpiredOtps created'
GO

-- =====================================================
-- VERIFICATION: Check migration results
-- =====================================================

PRINT ''
PRINT '========================================='
PRINT 'MIGRATION VERIFICATION'
PRINT '========================================='

-- Check Users table
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') 
    AND name = 'EmailConfirmed'
)
    PRINT '✓ Users.EmailConfirmed column exists'
ELSE
    PRINT '✗ Users.EmailConfirmed column NOT FOUND!'
GO

-- Check EmailOtps table
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailOtps')
BEGIN
    PRINT '✓ EmailOtps table exists'
    
    SELECT 
        'EmailOtps' AS TableName,
        COUNT(*) AS ColumnCount
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[EmailOtps]')
    
    PRINT '  Columns:'
    SELECT 
        '  - ' + name + ' (' + TYPE_NAME(system_type_id) + ')' AS ColumnInfo
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[EmailOtps]')
    ORDER BY column_id
END
ELSE
    PRINT '✗ EmailOtps table NOT FOUND!'
GO

-- Check indexes
PRINT ''
PRINT 'Indexes created:'
SELECT 
    '  - ' + i.name AS IndexName
FROM sys.indexes i
WHERE i.object_id IN (
    OBJECT_ID(N'[dbo].[Users]'),
    OBJECT_ID(N'[dbo].[EmailOtps]')
)
AND i.name LIKE 'IX_%'
GO

PRINT ''
PRINT '========================================='
PRINT 'MIGRATION COMPLETED SUCCESSFULLY!'
PRINT '========================================='
PRINT ''
PRINT 'Next steps:'
PRINT '1. Update Entity Framework DbContext to include EmailOtp entity'
PRINT '2. Create IEmailOtpRepository and implementation'
PRINT '3. Create EmailService for sending OTP emails'
PRINT '4. Update AuthenticationService with OTP methods'
PRINT '5. Create API endpoints for OTP registration flow'
GO
