-- =============================================
-- CLEANUP: Remove Password Reset OTP Migration
-- Date: 2026-01-26
-- Description: Removes all changes from Migration_AddPasswordResetOtp.sql
--              Run this if migration failed and you need to start fresh
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'CLEANUP: Removing Password Reset OTP Changes';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Drop Check Constraint
-- =============================================
PRINT 'STEP 1: Dropping check constraint CK_EmailOtps_OtpType...';

IF EXISTS (
    SELECT 1 FROM sys.check_constraints 
    WHERE name = 'CK_EmailOtps_OtpType'
    AND parent_object_id = OBJECT_ID('dbo.EmailOtps')
)
BEGIN
    ALTER TABLE [dbo].[EmailOtps]
    DROP CONSTRAINT [CK_EmailOtps_OtpType];
    
    PRINT '✅ Constraint dropped successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Constraint does not exist - skipping';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Drop Index
-- =============================================
PRINT 'STEP 2: Dropping index IX_EmailOtps_OtpType_Email...';

IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_OtpType_Email' 
    AND object_id = OBJECT_ID('dbo.EmailOtps')
)
BEGIN
    DROP INDEX [IX_EmailOtps_OtpType_Email] ON [dbo].[EmailOtps];
    
    PRINT '✅ Index dropped successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Index does not exist - skipping';
END
PRINT '';
GO

-- =============================================
-- STEP 3: Drop OtpType Column
-- =============================================
PRINT 'STEP 3: Dropping OtpType column...';

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.EmailOtps') 
    AND name = 'OtpType'
)
BEGIN
    ALTER TABLE [dbo].[EmailOtps]
    DROP COLUMN [OtpType];
    
    PRINT '✅ Column dropped successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Column does not exist - skipping';
END
PRINT '';
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '========================================';
PRINT 'VERIFICATION:';
PRINT '========================================';

-- Verify column is gone
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.EmailOtps') 
    AND name = 'OtpType'
)
BEGIN
    PRINT '✅ OtpType column removed';
END
ELSE
BEGIN
    PRINT '❌ OtpType column still exists!';
END

-- Verify index is gone
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_OtpType_Email' 
    AND object_id = OBJECT_ID('dbo.EmailOtps')
)
BEGIN
    PRINT '✅ Index removed';
END
ELSE
BEGIN
    PRINT '❌ Index still exists!';
END

-- Verify constraint is gone
IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints 
    WHERE name = 'CK_EmailOtps_OtpType'
)
BEGIN
    PRINT '✅ Constraint removed';
END
ELSE
BEGIN
    PRINT '❌ Constraint still exists!';
END

PRINT '';
PRINT '========================================';
PRINT 'Cleanup completed successfully!';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEP:';
PRINT 'Run Migration_AddPasswordResetOtp.sql';
PRINT '';

GO
