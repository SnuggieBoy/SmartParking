/* =====================================================
   SMART PARKING - Migration v1.3
   Add ParkingLocations table for map-based search
   ===================================================== */

USE SmartParkingDB;
GO

PRINT '======================================';
PRINT 'Starting Migration v1.3';
PRINT 'Adding ParkingLocations table for Map API';
PRINT '======================================';
GO

-- Step 1: Create ParkingLocations table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParkingLocations')
BEGIN
    PRINT 'Step 1: Creating ParkingLocations table...';
    
    CREATE TABLE ParkingLocations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        
        -- Geolocation data
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        
        -- Address details
        Province NVARCHAR(100) NOT NULL,
        District NVARCHAR(100) NOT NULL,
        Ward NVARCHAR(100) NOT NULL,
        Street NVARCHAR(200),
        Area NVARCHAR(100),
        FullAddress NVARCHAR(500),
        
        -- Parking info
        TotalSlots INT NOT NULL,
        AvailableSlots INT NOT NULL,
        PricePerHour DECIMAL(10,2) NOT NULL,
        
        -- Status
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2,
        
        -- Constraints
        CONSTRAINT CHK_ParkingLocation_Latitude CHECK (Latitude BETWEEN -90 AND 90),
        CONSTRAINT CHK_ParkingLocation_Longitude CHECK (Longitude BETWEEN -180 AND 180),
        CONSTRAINT CHK_ParkingLocation_Slots CHECK (AvailableSlots <= TotalSlots AND AvailableSlots >= 0),
        CONSTRAINT CHK_ParkingLocation_TotalSlots CHECK (TotalSlots > 0),
        CONSTRAINT CHK_ParkingLocation_Price CHECK (PricePerHour >= 0)
    );
    
    PRINT '✓ ParkingLocations table created';
END
ELSE
BEGIN
    PRINT '⚠ ParkingLocations table already exists (skipped)';
END
GO

-- Step 2: Create indexes for performance optimization
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_IsActive')
BEGIN
    PRINT 'Step 2: Creating index on IsActive column...';
    CREATE INDEX IX_ParkingLocations_IsActive ON ParkingLocations(IsActive);
    PRINT '✓ Index IX_ParkingLocations_IsActive created';
END
ELSE
BEGIN
    PRINT '⚠ Index IX_ParkingLocations_IsActive already exists (skipped)';
END
GO

-- Step 3: Create composite index for nearby search optimization
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Active_Slots')
BEGIN
    PRINT 'Step 3: Creating composite index for nearby search...';
    CREATE INDEX IX_ParkingLocations_Active_Slots 
    ON ParkingLocations(IsActive, AvailableSlots) 
    INCLUDE (Latitude, Longitude, Name, PricePerHour);
    PRINT '✓ Index IX_ParkingLocations_Active_Slots created';
END
ELSE
BEGIN
    PRINT '⚠ Index IX_ParkingLocations_Active_Slots already exists (skipped)';
END
GO

-- Step 4: Insert sample data (optional - for testing)
IF NOT EXISTS (SELECT 1 FROM ParkingLocations)
BEGIN
    PRINT 'Step 4: Inserting sample parking locations...';
    
    -- Ho Chi Minh City sample locations
    INSERT INTO ParkingLocations (
        Name, Description, Latitude, Longitude,
        Province, District, Ward, Street, FullAddress,
        TotalSlots, AvailableSlots, PricePerHour
    )
    VALUES
    -- District 1
    (
        'Vincom Center Dong Khoi Parking',
        'Underground parking at Vincom Center',
        10.7769, 106.7010,
        'Ho Chi Minh City', 'District 1', 'Ben Nghe', 'Dong Khoi',
        '72 Le Thanh Ton, Ben Nghe, District 1, Ho Chi Minh City',
        200, 50, 25000
    ),
    -- District 3
    (
        'Parkson Hung Vuong Parking',
        'Multi-level parking at Parkson Plaza',
        10.7828, 106.6877,
        'Ho Chi Minh City', 'District 3', 'Ward 9', 'Hung Vuong',
        '126 Hung Vuong, Ward 9, District 3, Ho Chi Minh City',
        150, 75, 20000
    ),
    -- District 7
    (
        'Crescent Mall Parking',
        'Shopping mall parking facility',
        10.7293, 106.7194,
        'Ho Chi Minh City', 'District 7', 'Tan Phu', 'Nguyen Luong Bang',
        '101 Ton Dat Tien, Tan Phu, District 7, Ho Chi Minh City',
        500, 200, 15000
    ),
    -- Hanoi samples
    (
        'Vincom Mega Mall Royal City',
        'Large shopping center parking',
        21.0015, 105.8237,
        'Hanoi', 'Thanh Xuan', 'Thanh Xuan Trung', 'Nguyen Trai',
        '72A Nguyen Trai, Thanh Xuan, Hanoi',
        800, 350, 18000
    ),
    (
        'Big C Thang Long Parking',
        'Supermarket parking area',
        21.0284, 105.7812,
        'Hanoi', 'Nam Tu Liem', 'Phu Do', 'Tran Dang Ninh',
        'Tran Dang Ninh, Phu Do, Nam Tu Liem, Hanoi',
        300, 120, 12000
    );
    
    DECLARE @sampleCount INT = @@ROWCOUNT;
    PRINT CONCAT('✓ ', @sampleCount, ' sample parking locations inserted');
END
ELSE
BEGIN
    PRINT '⚠ Sample data already exists (skipped)';
END
GO

-- Step 5: Verification
PRINT '======================================';
PRINT 'Step 5: Verifying migration...';
PRINT '======================================';

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParkingLocations')
    PRINT '✓ ParkingLocations table exists';
ELSE
    PRINT '✗ ERROR: ParkingLocations table not found!';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_IsActive')
    PRINT '✓ Index IX_ParkingLocations_IsActive exists';
ELSE
    PRINT '✗ ERROR: Index IX_ParkingLocations_IsActive not found!';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Active_Slots')
    PRINT '✓ Index IX_ParkingLocations_Active_Slots exists';
ELSE
    PRINT '✗ ERROR: Index IX_ParkingLocations_Active_Slots not found!';

-- Count records
DECLARE @count INT;
SELECT @count = COUNT(*) FROM ParkingLocations;
PRINT CONCAT('✓ Total parking locations: ', @count);

GO

-- Step 6: Summary
PRINT '======================================';
PRINT 'Migration v1.3 completed successfully! ✓';
PRINT '======================================';
PRINT '';
PRINT 'Summary of changes:';
PRINT '1. ✓ ParkingLocations table created';
PRINT '2. ✓ Constraints added (lat/lng validation, slot validation)';
PRINT '3. ✓ Performance indexes created';
PRINT '4. ✓ Sample data inserted (5 locations)';
PRINT '';
PRINT 'API Endpoints Available:';
PRINT '- GET /api/parking-locations (Admin only)';
PRINT '- GET /api/parking-locations/nearby?lat=10.77&lng=106.69&radius=3000 (User + Admin)';
PRINT '- POST /api/parking-locations (Admin only)';
PRINT '';
PRINT 'Next steps:';
PRINT '- Rebuild the application';
PRINT '- Test nearby search API with Swagger';
PRINT '- Test with real coordinates (Ho Chi Minh City or Hanoi)';
PRINT '';
PRINT CONCAT('Completion time: ', CONVERT(VARCHAR, GETDATE(), 120));
GO

/* =====================================================
   TEST QUERIES
   ===================================================== */

-- Test 1: View all locations
-- SELECT * FROM ParkingLocations;

-- Test 2: View active locations with slots
-- SELECT Name, Latitude, Longitude, AvailableSlots, PricePerHour
-- FROM ParkingLocations
-- WHERE IsActive = 1 AND AvailableSlots > 0;

-- Test 3: Check constraints
-- Try invalid latitude (should fail)
-- INSERT INTO ParkingLocations (Name, Latitude, Longitude, Province, District, Ward, TotalSlots, AvailableSlots, PricePerHour)
-- VALUES ('Test', 100, 106, 'HCM', 'D1', 'W1', 10, 5, 10000);  -- Latitude > 90 (ERROR)

-- Test 4: Check slot constraint
-- INSERT INTO ParkingLocations (Name, Latitude, Longitude, Province, District, Ward, TotalSlots, AvailableSlots, PricePerHour)
-- VALUES ('Test', 10, 106, 'HCM', 'D1', 'W1', 10, 15, 10000);  -- AvailableSlots > TotalSlots (ERROR)
