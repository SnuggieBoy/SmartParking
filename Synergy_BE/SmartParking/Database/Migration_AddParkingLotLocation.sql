-- =============================================
-- SMARTPARKING - ADD PARKINGLOT LOCATION
-- =============================================
-- Adds Latitude / Longitude columns to ParkingLots table
-- Safe to run multiple times (idempotent)
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;

PRINT '';
PRINT '=============================================';
PRINT 'ADDING LATITUDE/LONGITUDE TO PARKINGLOTS ...';
PRINT '=============================================';
PRINT '';

IF DB_NAME() != 'SmartParkingDB'
BEGIN
    PRINT '❌ ERROR: Wrong database! Expected SmartParkingDB, current: ' + DB_NAME();
    RAISERROR('Wrong database', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParkingLots')
BEGIN
    PRINT '❌ ERROR: ParkingLots table not found.';
    RETURN;
END

-- Latitude
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ParkingLots')
      AND name = 'Latitude'
)
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [Latitude] DECIMAL(9, 6) NULL;

    PRINT '✅ Added column: Latitude (DECIMAL(9,6) NULL)';
END
ELSE
BEGIN
    PRINT '⚠️  Column already exists: Latitude';
END

-- Longitude
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ParkingLots')
      AND name = 'Longitude'
)
BEGIN
    ALTER TABLE [dbo].[ParkingLots]
    ADD [Longitude] DECIMAL(9, 6) NULL;

    PRINT '✅ Added column: Longitude (DECIMAL(9,6) NULL)';
END
 ELSE
BEGIN
    PRINT '⚠️  Column already exists: Longitude';
END

PRINT '';
PRINT 'Verification:';
PRINT '------------';

SELECT 
    c.name       AS ColumnName,
    t.name       AS DataType,
    c.precision  AS [Precision],
    c.scale      AS [Scale],
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.ParkingLots')
  AND c.name IN ('Latitude', 'Longitude')
ORDER BY c.name;

PRINT '';
PRINT 'Done.';
PRINT '';

GO

