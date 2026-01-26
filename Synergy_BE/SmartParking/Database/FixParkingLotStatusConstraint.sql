-- =============================================
-- FIX PARKING LOT STATUS CHECK CONSTRAINT
-- Date: 2026-01-26
-- Description: Fixes Status check constraint to allow "Active" value
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'FIX PARKING LOT STATUS CONSTRAINT';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- Check current constraint
PRINT 'Current Status check constraint:';
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('dbo.ParkingLots')
  AND cc.name LIKE '%Status%';

PRINT '';

-- Drop ALL existing Status constraints
DECLARE @ConstraintName NVARCHAR(255);
DECLARE @DropSQL NVARCHAR(MAX);

-- First, try to drop the specific constraint that's causing the error
IF EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.ParkingLots')
      AND name = 'CK__ParkingLo__Statu__628FA481'
)
BEGIN
    PRINT 'Dropping specific constraint: CK__ParkingLo__Statu__628FA481';
    ALTER TABLE [dbo].[ParkingLots] DROP CONSTRAINT [CK__ParkingLo__Statu__628FA481];
    PRINT '✅ Constraint CK__ParkingLo__Statu__628FA481 dropped';
END

-- Also drop CK_ParkingLots_Status if it exists (from previous run)
IF EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.ParkingLots')
      AND name = 'CK_ParkingLots_Status'
)
BEGIN
    PRINT 'Dropping existing constraint: CK_ParkingLots_Status';
    ALTER TABLE [dbo].[ParkingLots] DROP CONSTRAINT [CK_ParkingLots_Status];
    PRINT '✅ Constraint CK_ParkingLots_Status dropped';
END

-- Drop all other constraints that might reference Status (by name pattern)
DECLARE constraint_cursor CURSOR FOR
SELECT cc.name
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('dbo.ParkingLots')
  AND (cc.name LIKE '%Status%' OR cc.name LIKE '%Statu%');

OPEN constraint_cursor;
FETCH NEXT FROM constraint_cursor INTO @ConstraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT 'Dropping existing constraint: ' + @ConstraintName;
    SET @DropSQL = 'ALTER TABLE [dbo].[ParkingLots] DROP CONSTRAINT [' + @ConstraintName + '];';
    EXEC sp_executesql @DropSQL;
    PRINT '✅ Constraint ' + @ConstraintName + ' dropped';
    FETCH NEXT FROM constraint_cursor INTO @ConstraintName;
END

CLOSE constraint_cursor;
DEALLOCATE constraint_cursor;

-- Add new constraint that allows common status values
PRINT '';
PRINT 'Adding new Status constraint (allowing: Active, Inactive, Closed, Full, Maintenance)...';
ALTER TABLE [dbo].[ParkingLots]
ADD CONSTRAINT [CK_ParkingLots_Status] 
CHECK ([Status] IN ('Active', 'Inactive', 'Closed', 'Full', 'Maintenance', 'Pending'));

PRINT '✅ New Status constraint added';

PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION: Status constraint after fix';
PRINT '========================================';
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('dbo.ParkingLots')
  AND cc.name LIKE '%Status%';

PRINT '';
PRINT '========================================';
PRINT 'Fix completed!';
PRINT '========================================';
PRINT '';

GO
