-- =============================================
-- PHASE 4: Map & Location Services
-- Date: 2026-01-26
-- Description: Creates ParkingLocations table for geospatial search
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'PHASE 4: Map & Location Services';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Create ParkingLocations Table
-- =============================================
PRINT 'STEP 1: Creating ParkingLocations table...';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParkingLocations' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ParkingLocations] (
        [LocationId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ParkingLotId] UNIQUEIDENTIFIER NOT NULL,
        [Latitude] DECIMAL(10, 7) NOT NULL,
        [Longitude] DECIMAL(10, 7) NOT NULL,
        [Province] NVARCHAR(100) NULL,
        [District] NVARCHAR(100) NULL,
        [Ward] NVARCHAR(100) NULL,
        [Street] NVARCHAR(255) NULL,
        [Area] NVARCHAR(255) NULL,
        [FullAddress] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] DATETIME2(7) NULL,
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2(7) NULL,
        [DeletedBy] UNIQUEIDENTIFIER NULL,
        
        CONSTRAINT [PK_ParkingLocations] PRIMARY KEY CLUSTERED ([LocationId] ASC),
        CONSTRAINT [FK_ParkingLocations_ParkingLots] FOREIGN KEY ([ParkingLotId])
            REFERENCES [dbo].[ParkingLots] ([ParkingLotId])
            ON DELETE CASCADE
    );
    
    PRINT '✅ ParkingLocations table created';
END
ELSE
BEGIN
    PRINT '⚠️  ParkingLocations table already exists';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Add Constraints
-- =============================================
PRINT 'STEP 2: Adding constraints...';

-- Check constraint for valid coordinates
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ParkingLocations_Latitude' AND parent_object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    ALTER TABLE [dbo].[ParkingLocations]
    ADD CONSTRAINT [CK_ParkingLocations_Latitude] CHECK ([Latitude] >= -90 AND [Latitude] <= 90);
    PRINT '✅ Latitude check constraint added';
END
ELSE PRINT '⚠️  Latitude constraint already exists';

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_ParkingLocations_Longitude' AND parent_object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    ALTER TABLE [dbo].[ParkingLocations]
    ADD CONSTRAINT [CK_ParkingLocations_Longitude] CHECK ([Longitude] >= -180 AND [Longitude] <= 180);
    PRINT '✅ Longitude check constraint added';
END
ELSE PRINT '⚠️  Longitude constraint already exists';

-- Unique constraint: One location per parking lot
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_ParkingLocations_ParkingLotId' AND object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_ParkingLocations_ParkingLotId]
    ON [dbo].[ParkingLocations] ([ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Unique index on ParkingLotId created';
END
ELSE PRINT '⚠️  Unique index on ParkingLotId already exists';

PRINT '';
GO

-- =============================================
-- STEP 3: Add Indexes for Performance
-- =============================================
PRINT 'STEP 3: Adding indexes for geospatial queries...';

-- Index on IsDeleted (for soft delete filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_IsDeleted' AND object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_IsDeleted]
    ON [dbo].[ParkingLocations] ([IsDeleted])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLocations_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_ParkingLocations_IsDeleted already exists';

-- Composite index on Latitude + Longitude (for bounding box queries)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Lat_Lng' AND object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_Lat_Lng]
    ON [dbo].[ParkingLocations] ([Latitude], [Longitude])
    INCLUDE ([ParkingLotId], [FullAddress])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLocations_Lat_Lng created';
END
ELSE PRINT '⚠️  Index IX_ParkingLocations_Lat_Lng already exists';

-- Index on Province + District (for address-based search)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Province_District' AND object_id = OBJECT_ID('dbo.ParkingLocations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_Province_District]
    ON [dbo].[ParkingLocations] ([Province], [District])
    INCLUDE ([Ward], [Street], [FullAddress], [ParkingLotId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_ParkingLocations_Province_District created';
END
ELSE PRINT '⚠️  Index IX_ParkingLocations_Province_District already exists';

PRINT '';
GO

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '========================================';
PRINT 'VERIFICATION:';
PRINT '========================================';

-- Check table exists
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParkingLocations' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    PRINT '✅ ParkingLocations table exists';
    
    -- Show table structure
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ParkingLocations'
    ORDER BY ORDINAL_POSITION;
    
    -- Show indexes
    PRINT '';
    PRINT 'Indexes:';
    SELECT 
        i.name AS IndexName,
        i.type_desc AS IndexType,
        i.is_unique AS IsUnique
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID('dbo.ParkingLocations')
    ORDER BY i.name;
END
ELSE
BEGIN
    PRINT '❌ ParkingLocations table does not exist';
END

PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
PRINT '';

GO
