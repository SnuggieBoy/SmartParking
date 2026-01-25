/*
================================================
PHASE 2 MIGRATION: Audit Fields & Soft Delete
================================================
Purpose: Add audit tracking and soft delete to ParkingLocations table
Date: 2026-01-25
Author: Production Team

Changes:
1. Add CreatedBy, UpdatedBy audit fields
2. Add IsDeleted, DeletedAt, DeletedBy soft delete fields
3. Add indexes for performance optimization
4. Set default values for new fields on existing records

SAFETY: This migration is BACKWARD COMPATIBLE
- All new columns are nullable or have defaults
- Existing queries continue to work
- No data loss

FIXED: Added GO statements to separate batches
================================================
*/

USE [SmartParkingDB];
GO

PRINT '================================================'
PRINT 'PHASE 2 MIGRATION: Starting...'
PRINT '================================================'
GO

-- ==========================================
-- STEP 1: ADD AUDIT AND SOFT DELETE COLUMNS
-- ==========================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParkingLocations') AND name = 'CreatedBy')
BEGIN
    PRINT 'Step 1: Adding audit and soft delete columns...'
    
    -- Audit fields
    ALTER TABLE [dbo].[ParkingLocations] ADD [CreatedBy] UNIQUEIDENTIFIER NULL;
    ALTER TABLE [dbo].[ParkingLocations] ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;
    
    -- Soft delete fields
    ALTER TABLE [dbo].[ParkingLocations] ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    ALTER TABLE [dbo].[ParkingLocations] ADD [DeletedAt] DATETIME2(7) NULL;
    ALTER TABLE [dbo].[ParkingLocations] ADD [DeletedBy] UNIQUEIDENTIFIER NULL;
    
    PRINT '✓ Columns added: CreatedBy, UpdatedBy, IsDeleted, DeletedAt, DeletedBy'
END
ELSE
BEGIN
    PRINT 'Step 1: Columns already exist (skipping)'
END
GO

-- ==========================================
-- STEP 2: ADD PERFORMANCE INDEXES
-- ==========================================

PRINT 'Step 2: Creating performance indexes...'

-- Geospatial index (Latitude, Longitude)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Latitude_Longitude' AND object_id = OBJECT_ID('ParkingLocations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_Latitude_Longitude]
    ON [dbo].[ParkingLocations]([Latitude], [Longitude])
    INCLUDE ([IsActive], [AvailableSlots])
    WHERE [IsDeleted] = 0;
    
    PRINT '✓ Geospatial index created'
END
ELSE
BEGIN
    PRINT '⊙ Geospatial index already exists'
END

-- Soft delete index
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_IsDeleted' AND object_id = OBJECT_ID('ParkingLocations'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_IsDeleted]
    ON [dbo].[ParkingLocations]([IsDeleted])
    INCLUDE ([IsActive], [AvailableSlots]);
    
    PRINT '✓ Soft delete index created'
END
ELSE
BEGIN
    PRINT '⊙ Soft delete index already exists'
END
GO

-- ==========================================
-- STEP 3: VERIFICATION
-- ==========================================

PRINT '================================================'
PRINT 'Step 3: Verification'
PRINT '================================================'

-- Verify columns
PRINT ''
PRINT 'New Columns:'
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS IsNullable,
    ISNULL(dc.definition, 'N/A') AS DefaultValue
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('ParkingLocations')
AND c.name IN ('CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
ORDER BY c.column_id;

-- Verify indexes
PRINT ''
PRINT 'New Indexes:'
SELECT DISTINCT
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('ParkingLocations')
AND i.name IN ('IX_ParkingLocations_Latitude_Longitude', 'IX_ParkingLocations_IsDeleted')
ORDER BY i.name;

-- Verify existing records
PRINT ''
PRINT 'Existing Records Status:'
IF EXISTS (SELECT 1 FROM [dbo].[ParkingLocations])
BEGIN
    SELECT 
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS ActiveRecords,
        SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedRecords
    FROM [dbo].[ParkingLocations];
END
ELSE
BEGIN
    PRINT 'No existing records in ParkingLocations table'
END

PRINT ''
PRINT '================================================'
PRINT 'PHASE 2 MIGRATION: COMPLETED SUCCESSFULLY ✓'
PRINT '================================================'
PRINT 'Summary:'
PRINT '  ✓ Audit tracking fields: CreatedBy, UpdatedBy'
PRINT '  ✓ Soft delete fields: IsDeleted, DeletedAt, DeletedBy'
PRINT '  ✓ Performance indexes created'
PRINT '  ✓ All existing records preserved (IsDeleted = 0)'
PRINT ''
PRINT 'Next steps:'
PRINT '  1. Deploy updated application code'
PRINT '  2. Verify audit fields populate on create/update'
PRINT '  3. Test soft delete functionality'
PRINT '================================================'
GO
