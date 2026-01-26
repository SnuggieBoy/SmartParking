-- =============================================
-- FIX PARKING LOT NAME AND ADDRESS COLUMNS
-- Date: 2026-01-26
-- Description: Ensures Name and Address columns are NOT NULL to match EF Core config
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'FIX PARKING LOT NAME AND ADDRESS';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- Check current state
PRINT 'Current state of Name and Address columns:';
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ParkingLots'
  AND COLUMN_NAME IN ('Name', 'Address');

PRINT '';

-- Fix Name column: Make it NOT NULL if it's nullable
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ParkingLots' 
      AND COLUMN_NAME = 'Name'
      AND IS_NULLABLE = 'YES'
)
BEGIN
    PRINT 'Fixing Name column (making it NOT NULL)...';
    
    -- Step 1: Drop indexes that might depend on Name column
    DECLARE @IndexName NVARCHAR(255);
    DECLARE @DropIndexSQL NVARCHAR(MAX);
    
    DECLARE index_cursor CURSOR FOR
    SELECT DISTINCT i.name
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID('dbo.ParkingLots')
      AND c.name = 'Name'
      AND i.name IS NOT NULL;
    
    OPEN index_cursor;
    FETCH NEXT FROM index_cursor INTO @IndexName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'Dropping index ' + @IndexName + ' (will recreate after column fix)...';
        SET @DropIndexSQL = 'DROP INDEX [' + @IndexName + '] ON [dbo].[ParkingLots];';
        EXEC sp_executesql @DropIndexSQL;
        PRINT '✅ Index ' + @IndexName + ' dropped';
        FETCH NEXT FROM index_cursor INTO @IndexName;
    END
    
    CLOSE index_cursor;
    DEALLOCATE index_cursor;
    
    -- Step 2: Update any NULL values to empty string
    UPDATE [dbo].[ParkingLots]
    SET [Name] = ''
    WHERE [Name] IS NULL;
    
    -- Step 3: Alter column to NOT NULL
    ALTER TABLE [dbo].[ParkingLots]
    ALTER COLUMN [Name] NVARCHAR(100) NOT NULL;
    
    -- Step 4: Recreate index if it was IX_ParkingLots_OwnerId_IsDeleted
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.indexes 
        WHERE object_id = OBJECT_ID('dbo.ParkingLots')
          AND name = 'IX_ParkingLots_OwnerId_IsDeleted'
    )
    BEGIN
        PRINT 'Recreating index IX_ParkingLots_OwnerId_IsDeleted...';
        CREATE NONCLUSTERED INDEX [IX_ParkingLots_OwnerId_IsDeleted]
        ON [dbo].[ParkingLots] ([OwnerId], [IsDeleted])
        WHERE [IsDeleted] = 0;
        PRINT '✅ Index IX_ParkingLots_OwnerId_IsDeleted recreated';
    END
    
    PRINT '✅ Name column is now NOT NULL';
END
ELSE
BEGIN
    PRINT '✅ Name column is already NOT NULL';
END

-- Fix Address column: Make it NOT NULL if it's nullable
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ParkingLots' 
      AND COLUMN_NAME = 'Address'
      AND IS_NULLABLE = 'YES'
)
BEGIN
    PRINT 'Fixing Address column (making it NOT NULL)...';
    
    -- Step 1: Drop indexes that might depend on Address column
    DECLARE @IndexName2 NVARCHAR(255);
    DECLARE @DropIndexSQL2 NVARCHAR(MAX);
    
    DECLARE index_cursor2 CURSOR FOR
    SELECT DISTINCT i.name
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID('dbo.ParkingLots')
      AND c.name = 'Address'
      AND i.name IS NOT NULL;
    
    OPEN index_cursor2;
    FETCH NEXT FROM index_cursor2 INTO @IndexName2;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'Dropping index ' + @IndexName2 + ' (will recreate after column fix)...';
        SET @DropIndexSQL2 = 'DROP INDEX [' + @IndexName2 + '] ON [dbo].[ParkingLots];';
        EXEC sp_executesql @DropIndexSQL2;
        PRINT '✅ Index ' + @IndexName2 + ' dropped';
        FETCH NEXT FROM index_cursor2 INTO @IndexName2;
    END
    
    CLOSE index_cursor2;
    DEALLOCATE index_cursor2;
    
    -- Step 2: Update any NULL values to empty string
    UPDATE [dbo].[ParkingLots]
    SET [Address] = ''
    WHERE [Address] IS NULL;
    
    -- Step 3: Alter column to NOT NULL
    ALTER TABLE [dbo].[ParkingLots]
    ALTER COLUMN [Address] NVARCHAR(255) NOT NULL;
    
    PRINT '✅ Address column is now NOT NULL';
END
ELSE
BEGIN
    PRINT '✅ Address column is already NOT NULL';
END

PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION: Name and Address columns after fix';
PRINT '========================================';
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ParkingLots'
  AND COLUMN_NAME IN ('Name', 'Address');

PRINT '';
PRINT '========================================';
PRINT 'Fix completed!';
PRINT '========================================';
PRINT '';

GO
