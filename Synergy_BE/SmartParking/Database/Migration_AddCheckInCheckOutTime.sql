/* =====================================================
   SMART PARKING - Migration v1.2
   Add CheckInTime and CheckOutTime to Bookings table
   ===================================================== */

USE SmartParkingDB;
GO

PRINT '======================================';
PRINT 'Starting Migration v1.2';
PRINT 'Adding CheckInTime and CheckOutTime columns';
PRINT '======================================';
GO

-- Step 1: Add CheckInTime column if not exists
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Bookings') 
    AND name = 'CheckInTime'
)
BEGIN
    PRINT 'Step 1: Adding CheckInTime column...';
    ALTER TABLE Bookings ADD CheckInTime DATETIME2 NULL;
    PRINT '✓ CheckInTime column added';
END
ELSE
BEGIN
    PRINT '⚠ CheckInTime column already exists (skipped)';
END
GO

-- Step 2: Add CheckOutTime column if not exists
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Bookings') 
    AND name = 'CheckOutTime'
)
BEGIN
    PRINT 'Step 2: Adding CheckOutTime column...';
    ALTER TABLE Bookings ADD CheckOutTime DATETIME2 NULL;
    PRINT '✓ CheckOutTime column added';
END
ELSE
BEGIN
    PRINT '⚠ CheckOutTime column already exists (skipped)';
END
GO

-- Step 3: Migrate existing data (optional - if you have existing bookings)
PRINT 'Step 3: Migrating existing data (if any)...';

-- Copy BookingTime to CheckInTime for Active/Completed bookings
UPDATE Bookings
SET CheckInTime = BookingTime
WHERE BookingTime IS NOT NULL 
  AND CheckInTime IS NULL
  AND Status IN ('InProgress', 'Completed');

DECLARE @migratedCheckIn INT = @@ROWCOUNT;

-- Copy UpdatedAt to CheckOutTime for Completed bookings
UPDATE Bookings
SET CheckOutTime = UpdatedAt
WHERE UpdatedAt IS NOT NULL 
  AND CheckOutTime IS NULL
  AND Status = 'Completed';

DECLARE @migratedCheckOut INT = @@ROWCOUNT;

PRINT '✓ Data migration complete';
PRINT CONCAT('  - ', @migratedCheckIn, ' records updated with CheckInTime');
PRINT CONCAT('  - ', @migratedCheckOut, ' records updated with CheckOutTime');
GO

-- Step 4: Verification
PRINT '======================================';
PRINT 'Step 4: Verifying migration...';
PRINT '======================================';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bookings') AND name = 'CheckInTime')
    PRINT '✓ Bookings.CheckInTime exists';
ELSE
    PRINT '✗ ERROR: Bookings.CheckInTime not found!';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bookings') AND name = 'CheckOutTime')
    PRINT '✓ Bookings.CheckOutTime exists';
ELSE
    PRINT '✗ ERROR: Bookings.CheckOutTime not found!';

GO

-- Step 5: Summary
PRINT '======================================';
PRINT 'Migration v1.2 completed successfully! ✓';
PRINT '======================================';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. ✓ CheckInTime column added (DATETIME2 NULL)';
PRINT '2. ✓ CheckOutTime column added (DATETIME2 NULL)';
PRINT '3. ✓ Existing data migrated (if any)';
PRINT '';
PRINT 'Next steps:';
PRINT '- Update your C# entity models';
PRINT '- Rebuild the application';
PRINT '- Test check-in/check-out endpoints';
PRINT '';
PRINT CONCAT('Completion time: ', CONVERT(VARCHAR, GETDATE(), 120));
GO
