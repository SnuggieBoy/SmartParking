-- =============================================
-- FIX SCRIPT: Phase 2 Booking Migration Issues
-- Date: 2026-01-26
-- Description: Fixes issues from Migration_Phase2_BookingEnhancements.sql
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'FIX: Phase 2 Booking Migration Issues';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Fix CreatedAt Default Constraint
-- =============================================
PRINT 'STEP 1: Fixing CreatedAt default constraint...';

-- Drop existing constraint if it exists with a different name
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('dbo.Bookings')
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.Bookings'), 'CreatedAt', 'ColumnId');

IF @ConstraintName IS NOT NULL AND @ConstraintName <> 'DF_Bookings_CreatedAt'
BEGIN
    DECLARE @DropSql NVARCHAR(MAX) = 'ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [' + @ConstraintName + ']';
    EXEC sp_executesql @DropSql;
    PRINT '✅ Dropped existing constraint: ' + @ConstraintName;
END

-- Add the correct constraint if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Bookings_CreatedAt' AND parent_object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    ALTER TABLE [dbo].[Bookings]
    ADD CONSTRAINT [DF_Bookings_CreatedAt] DEFAULT (GETUTCDATE()) FOR [CreatedAt];
    PRINT '✅ Added DF_Bookings_CreatedAt constraint';
END
ELSE
BEGIN
    PRINT '⚠️  DF_Bookings_CreatedAt constraint already exists';
END

PRINT '';
GO

-- =============================================
-- STEP 2: Create Missing Indexes
-- =============================================
PRINT 'STEP 2: Creating missing indexes...';

-- Index on Status
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_Status' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_Status]
    ON [dbo].[Bookings] ([Status])
    INCLUDE ([StartTime], [EndTime], [ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_Status created';
END
ELSE PRINT '⚠️  Index IX_Bookings_Status already exists';

-- Index on IsDeleted
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_IsDeleted' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_IsDeleted]
    ON [dbo].[Bookings] ([IsDeleted])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_Bookings_IsDeleted already exists';

-- Index on UserId + IsDeleted
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bookings_UserId_IsDeleted' AND object_id = OBJECT_ID('dbo.Bookings'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Bookings_UserId_IsDeleted]
    ON [dbo].[Bookings] ([UserId], [IsDeleted])
    INCLUDE ([Status], [StartTime], [EndTime], [ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_Bookings_UserId_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_Bookings_UserId_IsDeleted already exists';

-- Index on ParkingLotId + Status + StartTime (fixed syntax)
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

-- Check columns
PRINT 'Columns:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Bookings'
  AND COLUMN_NAME IN ('BookingTime', 'TotalAmount', 'CreatedAt', 'CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
ORDER BY ORDINAL_POSITION;

-- Check indexes
PRINT '';
PRINT 'Indexes:';
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Bookings')
  AND i.name LIKE 'IX_Bookings_%'
ORDER BY i.name;

PRINT '';
PRINT '========================================';
PRINT 'Fix completed successfully!';
PRINT '========================================';
PRINT '';

GO
