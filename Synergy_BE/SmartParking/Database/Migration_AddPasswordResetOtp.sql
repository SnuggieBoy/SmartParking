-- =============================================
-- Migration: Add Password Reset OTP Support
-- Date: 2026-01-26
-- Description: Extends EmailOtps table to support both Registration and Password Reset OTPs
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'Starting Migration: Add Password Reset OTP Support';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Add OtpType Column
-- =============================================
PRINT 'STEP 1: Adding OtpType column to EmailOtps table...';

-- Check if column already exists
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.EmailOtps') 
    AND name = 'OtpType'
)
BEGIN
    PRINT '⚠️  OtpType column already exists - skipping';
END
ELSE
BEGIN
    -- Add the column
    ALTER TABLE [dbo].[EmailOtps]
    ADD [OtpType] NVARCHAR(50) NOT NULL DEFAULT 'Registration';
    
    PRINT '✅ OtpType column added successfully';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Update Existing Records
-- =============================================
PRINT 'STEP 2: Updating existing OTP records to Registration type...';

-- Update any records that might have NULL or empty OtpType
-- (This is for safety, default value should handle this)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmailOtps') AND name = 'OtpType')
BEGIN
    UPDATE [dbo].[EmailOtps]
    SET [OtpType] = 'Registration'
    WHERE [OtpType] IS NULL OR [OtpType] = '';

    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT '✅ Updated ' + CAST(@UpdatedCount AS VARCHAR) + ' existing OTP records';
END
ELSE
BEGIN
    PRINT '❌ OtpType column not found - skipping update';
END
PRINT '';
GO

-- =============================================
-- STEP 3: Add Index on OtpType
-- =============================================
PRINT 'STEP 3: Adding index on OtpType for performance...';

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_OtpType_Email' 
    AND object_id = OBJECT_ID('dbo.EmailOtps')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_EmailOtps_OtpType_Email]
    ON [dbo].[EmailOtps] ([OtpType], [Email], [IsUsed])
    WHERE [IsUsed] = 0;
    
    PRINT '✅ Index IX_EmailOtps_OtpType_Email created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Index IX_EmailOtps_OtpType_Email already exists - skipping';
END
PRINT '';
GO

-- =============================================
-- STEP 4: Add Check Constraint (Optional - for data integrity)
-- =============================================
PRINT 'STEP 4: Adding check constraint on OtpType values...';

IF NOT EXISTS (
    SELECT 1 FROM sys.check_constraints 
    WHERE name = 'CK_EmailOtps_OtpType'
)
BEGIN
    ALTER TABLE [dbo].[EmailOtps]
    ADD CONSTRAINT [CK_EmailOtps_OtpType]
    CHECK ([OtpType] IN ('Registration', 'PasswordReset'));
    
    PRINT '✅ Check constraint CK_EmailOtps_OtpType added successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Check constraint CK_EmailOtps_OtpType already exists - skipping';
END
PRINT '';
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '========================================';
PRINT 'VERIFICATION:';
PRINT '========================================';

-- Check column exists
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.EmailOtps') 
    AND name = 'OtpType'
)
BEGIN
    PRINT '✅ OtpType column exists';
    
    -- Show column info
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM [dbo].[EmailOtps];
    PRINT '   Total OTP records: ' + CAST(@TotalCount AS VARCHAR);
    
    -- Count by type
    SELECT 
        [OtpType],
        COUNT(*) AS [Count]
    FROM [dbo].[EmailOtps]
    GROUP BY [OtpType];
END
ELSE
BEGIN
    PRINT '❌ OtpType column NOT found!';
END

-- Check index exists
IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_EmailOtps_OtpType_Email' 
    AND object_id = OBJECT_ID('dbo.EmailOtps')
)
BEGIN
    PRINT '✅ Index IX_EmailOtps_OtpType_Email exists';
END
ELSE
BEGIN
    PRINT '❌ Index IX_EmailOtps_OtpType_Email NOT found!';
END

-- Check constraint exists
IF EXISTS (
    SELECT 1 FROM sys.check_constraints 
    WHERE name = 'CK_EmailOtps_OtpType'
)
BEGIN
    PRINT '✅ Check constraint CK_EmailOtps_OtpType exists';
END
ELSE
BEGIN
    PRINT '❌ Check constraint CK_EmailOtps_OtpType NOT found!';
END

PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Verify the results above';
PRINT '2. Test OTP generation for both Registration and PasswordReset types';
PRINT '3. Update application code to use OtpType parameter';
PRINT '';

-- =============================================
-- SAMPLE QUERIES FOR TESTING
-- =============================================
PRINT '========================================';
PRINT 'SAMPLE QUERIES FOR TESTING:';
PRINT '========================================';
PRINT '';
PRINT '-- Check all OTPs:';
PRINT 'SELECT OtpId, Email, OtpType, OtpCode, IsUsed, ExpiredAt, CreatedAt';
PRINT 'FROM EmailOtps';
PRINT 'ORDER BY CreatedAt DESC';
PRINT '';
PRINT '-- Check unused Registration OTPs:';
PRINT 'SELECT * FROM EmailOtps';
PRINT 'WHERE OtpType = ''Registration'' AND IsUsed = 0';
PRINT '';
PRINT '-- Check unused PasswordReset OTPs:';
PRINT 'SELECT * FROM EmailOtps';
PRINT 'WHERE OtpType = ''PasswordReset'' AND IsUsed = 0';
PRINT '';

GO
