-- =============================================
-- PHASE 2: Booking System Enhancements
-- Date: 2026-01-26
-- Description: Adds audit fields, soft delete, and enhances Booking table
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'PHASE 2: Booking Enhancements';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Update BookingTime to NOT NULL
-- =============================================
PRINT 'STEP 1: Updating BookingTime column...';

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Bookings') 
    AND name = 'BookingTime'
    AND is_nullable = 1
)
BEGIN
    -- Set default for existing NULL values
    UPDATE [dbo].[Bookings]
    SET [BookingTime] = [CreatedAt]
    WHERE [BookingTime] IS NULL;
    
    -- Make NOT NULL
    ALTER TABLE [dbo].[Bookings]
    ALTER COLUMN [BookingTime] DATETIME2(7) NOT NULL;
    
    PRINT '✅ BookingTime updated to NOT NULL';
END
ELSE
BEGIN
    PRINT '⚠️  BookingTime already NOT NULL or does not exist';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Update TotalAmount to NOT NULL
-- =============================================
PRINT 'STEP 2: Updating TotalAmount column...';

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Bookings') 
    AND name = 'TotalAmount'
    AND is_nullable = 1
)
BEGIN
    -- Set default for existing NULL values
    UPDATE [dbo].[Bookings]
    SET [TotalAmount] = 0
    WHERE [TotalAmount] IS NULL;
    
    -- Make NOT NULL
    ALTER TABLE [dbo].[Bookings]
    ALTER COLUMN [TotalAmount] DECIMAL(18, 2) NOT NULL;
    
    PRINT '✅ TotalAmount updated to NOT NULL';
END
ELSE
BEGIN
    PRINT '⚠️  TotalAmount already NOT NULL or does not exist';
END
PRINT '';
GO

-- =============================================
-- STEP 3: Update CreatedAt to NOT NULL
-- =============================================
PRINT 'STEP 3: Updating CreatedAt column...';

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Bookings') 
    AND name = 'CreatedAt'
    AND is_nullable = 1
)
BEGIN
    -- Set default for existing NULL values
    UPDATE [dbo].[Bookings]
    SET [CreatedAt] = GETUTCDATE()
    WHERE [CreatedAt] IS NULL;
    
    -- Make NOT NULL
    ALTER TABLE [dbo].[Bookings]
    ALTER COLUMN [CreatedAt] DATETIME2(7) NOT NULL;
    
    -- Add default constraint (only if it doesn't exist)
    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Bookings_CreatedAt' AND parent_object_id = OBJECT_ID('dbo.Bookings'))
    BEGIN
        ALTER TABLE [dbo].[Bookings]
        ADD CONSTRAINT [DF_Bookings_CreatedAt] DEFAULT (GETUTCDATE()) FOR [CreatedAt];
    END
    
    PRINT '✅ CreatedAt updated to NOT NULL with default';
END
ELSE
BEGIN
    PRINT '⚠️  CreatedAt already NOT NULL or does not exist';
END
PRINT '';
GO

-- =============================================
-- STEP 4: Add Audit Fields
-- =============================================
PRINT 'STEP 4: Adding audit fields (CreatedBy, UpdatedBy)...';

-- CreatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD [CreatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ CreatedBy column added';
END
ELSE PRINT '⚠️  CreatedBy already exists';

-- UpdatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ UpdatedBy column added';
END
ELSE PRINT '⚠️  UpdatedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 5: Add Soft Delete Fields
-- =============================================
PRINT 'STEP 5: Adding soft delete fields (IsDeleted, DeletedAt, DeletedBy)...';

-- IsDeleted
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT '✅ IsDeleted column added';
END
ELSE PRINT '⚠️  IsDeleted already exists';

-- DeletedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD [DeletedAt] DATETIME2(7) NULL;
    PRINT '✅ DeletedAt column added';
END
ELSE PRINT '⚠️  DeletedAt already exists';

-- DeletedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD [DeletedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ DeletedBy column added';
END
ELSE PRINT '⚠️  DeletedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 6: Initialize Audit Fields
-- =============================================
PRINT 'STEP 6: Initializing audit fields for existing records...';

-- Set CreatedBy = UserId for existing records
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'CreatedBy')
BEGIN
    UPDATE [dbo].[Bookings]
    SET [CreatedBy] = [UserId]
    WHERE [CreatedBy] IS NULL;
    
    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT '✅ Updated ' + CAST(@UpdatedCount AS VARCHAR) + ' records with CreatedBy = UserId';
END

PRINT '';
GO

-- =============================================
-- STEP 7: Add Indexes for Performance
-- =============================================
PRINT 'STEP 7: Adding indexes...';

-- Index on Status (for filtering by status)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_Status' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_Status]
    ON [dbo].[Bookings] ([Status])
    INCLUDE ([StartTime], [EndTime], [ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_Status created';
END
ELSE PRINT '⚠️  Index IX_Bookings_Status already exists';

-- Index on IsDeleted (for soft delete filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_IsDeleted' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_IsDeleted]
    ON [dbo].[Bookings] ([IsDeleted])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_Bookings_IsDeleted already exists';

-- Index on UserId + IsDeleted (for user's bookings query)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_UserId_IsDeleted' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_UserId_IsDeleted]
    ON [dbo].[Bookings] ([UserId], [IsDeleted])
    INCLUDE ([Status], [StartTime], [EndTime], [ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_UserId_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_Bookings_UserId_IsDeleted already exists';

-- Index on ParkingLotId + Status + StartTime (for conflict checking)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_ParkingLotId_Status_StartTime' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_ParkingLotId_Status_StartTime]
    ON [dbo].[Bookings] ([ParkingLotId], [Status], [StartTime])
    INCLUDE ([EndTime], [IsDeleted])
    WHERE [IsDeleted] = 0 AND [Status] <> 'Cancelled' AND [Status] <> 'Completed';
    
    PRINT '✅ Index IX_Bookings_ParkingLotId_Status_StartTime created';
END
ELSE PRINT '⚠️  Index IX_Bookings_ParkingLotId_Status_StartTime already exists';

PRINT '';
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '========================================';
PRINT 'VERIFICATION:';
PRINT '========================================';

-- Check all columns exist
DECLARE @MissingColumns TABLE (ColumnName NVARCHAR(50));

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'CreatedBy')
    INSERT INTO @MissingColumns VALUES ('CreatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'UpdatedBy')
    INSERT INTO @MissingColumns VALUES ('UpdatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'IsDeleted')
    INSERT INTO @MissingColumns VALUES ('IsDeleted');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'DeletedAt')
    INSERT INTO @MissingColumns VALUES ('DeletedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bookings') AND name = 'DeletedBy')
    INSERT INTO @MissingColumns VALUES ('DeletedBy');

IF NOT EXISTS (SELECT 1 FROM @MissingColumns)
BEGIN
    PRINT '✅ All columns added successfully';
    
    -- Show table structure
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Bookings'
      AND COLUMN_NAME IN ('BookingTime', 'TotalAmount', 'CreatedAt', 'CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '❌ Missing columns:';
    SELECT ColumnName FROM @MissingColumns;
END

-- Check indexes
PRINT '';
PRINT 'Indexes:';
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Bookings')
  AND i.name LIKE 'IX_Bookings_%'
ORDER BY i.name;

PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
PRINT '';

GO
