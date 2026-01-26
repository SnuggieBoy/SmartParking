-- =============================================
-- PHASE 1: Parking Lot Management Enhancements
-- Date: 2026-01-26
-- Description: Adds audit fields, soft delete, and IsActive flag to ParkingLots table
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'PHASE 1: Parking Lot Enhancements';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Add IsActive Column
-- =============================================
PRINT 'STEP 1: Adding IsActive column...';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.ParkingLots') 
    AND name = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [IsActive] BIT NOT NULL DEFAULT 1;
    
    PRINT '✅ IsActive column added';
END
ELSE
BEGIN
    PRINT '⚠️  IsActive column already exists';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Add Audit Fields
-- =============================================
PRINT 'STEP 2: Adding audit fields (UpdatedAt, CreatedBy, UpdatedBy)...';

-- UpdatedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [UpdatedAt] DATETIME2(7) NULL;
    PRINT '✅ UpdatedAt column added';
END
ELSE PRINT '⚠️  UpdatedAt already exists';

-- CreatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [CreatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ CreatedBy column added';
END
ELSE PRINT '⚠️  CreatedBy already exists';

-- UpdatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ UpdatedBy column added';
END
ELSE PRINT '⚠️  UpdatedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 3: Add Soft Delete Fields
-- =============================================
PRINT 'STEP 3: Adding soft delete fields (IsDeleted, DeletedAt, DeletedBy)...';

-- IsDeleted
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT '✅ IsDeleted column added';
END
ELSE PRINT '⚠️  IsDeleted already exists';

-- DeletedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [DeletedAt] DATETIME2(7) NULL;
    PRINT '✅ DeletedAt column added';
END
ELSE PRINT '⚠️  DeletedAt already exists';

-- DeletedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [DeletedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ DeletedBy column added';
END
ELSE PRINT '⚠️  DeletedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 4: Update Existing Records
-- =============================================
PRINT 'STEP 4: Initializing audit fields for existing records...';

-- Set CreatedBy = OwnerId for existing records
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'CreatedBy')
BEGIN
    UPDATE [dbo].[ParkingLots]
    SET [CreatedBy] = [OwnerId]
    WHERE [CreatedBy] IS NULL;
    
    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT '✅ Updated ' + CAST(@UpdatedCount AS VARCHAR) + ' records with CreatedBy = OwnerId';
END

PRINT '';
GO

-- =============================================
-- STEP 5: Add Indexes for Performance
-- =============================================
PRINT 'STEP 5: Adding indexes...';

-- Index on IsActive (for filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLots_IsActive' AND object_id = OBJECT_ID('dbo.ParkingLots'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLots_IsActive]
    ON [dbo].[ParkingLots] ([IsActive])
    INCLUDE ([Status], [CurrentOccupancy], [TotalCapacity])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLots_IsActive created';
END
ELSE PRINT '⚠️  Index IX_ParkingLots_IsActive already exists';

-- Index on IsDeleted (for soft delete filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLots_IsDeleted' AND object_id = OBJECT_ID('dbo.ParkingLots'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLots_IsDeleted]
    ON [dbo].[ParkingLots] ([IsDeleted])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLots_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_ParkingLots_IsDeleted already exists';

-- Index on OwnerId + IsDeleted (for owner's parking lots query)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLots_OwnerId_IsDeleted' AND object_id = OBJECT_ID('dbo.ParkingLots'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLots_OwnerId_IsDeleted]
    ON [dbo].[ParkingLots] ([OwnerId], [IsDeleted])
    INCLUDE ([Name], [Status], [IsActive])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLots_OwnerId_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_ParkingLots_OwnerId_IsDeleted already exists';

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

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'IsActive')
    INSERT INTO @MissingColumns VALUES ('IsActive');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'UpdatedAt')
    INSERT INTO @MissingColumns VALUES ('UpdatedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'CreatedBy')
    INSERT INTO @MissingColumns VALUES ('CreatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'UpdatedBy')
    INSERT INTO @MissingColumns VALUES ('UpdatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'IsDeleted')
    INSERT INTO @MissingColumns VALUES ('IsDeleted');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'DeletedAt')
    INSERT INTO @MissingColumns VALUES ('DeletedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ParkingLots') AND name = 'DeletedBy')
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
    WHERE TABLE_NAME = 'ParkingLots'
      AND COLUMN_NAME IN ('IsActive', 'UpdatedAt', 'CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
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
WHERE i.object_id = OBJECT_ID('dbo.ParkingLots')
  AND i.name LIKE 'IX_ParkingLots_%'
ORDER BY i.name;

PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
PRINT '';

GO
