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
3. Add indexes for performance optimization (Latitude, Longitude for geospatial queries)
4. Set default values for new fields on existing records

SAFETY: This migration is BACKWARD COMPATIBLE
- All new columns are nullable or have defaults
- Existing queries continue to work
- No data loss
================================================
*/

USE [SmartParkingDB];
GO

-- Check if migration already applied
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('ParkingLocations') 
    AND name = 'CreatedBy'
)
BEGIN
    PRINT 'Phase 2 Migration: Adding audit and soft delete columns to ParkingLocations...'

    -- ==========================================
    -- 1. ADD AUDIT FIELDS
    -- ==========================================
    
    ALTER TABLE [dbo].[ParkingLocations]
    ADD [CreatedBy] UNIQUEIDENTIFIER NULL;

    ALTER TABLE [dbo].[ParkingLocations]
    ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;

    PRINT '✓ Audit fields (CreatedBy, UpdatedBy) added'

    -- ==========================================
    -- 2. ADD SOFT DELETE FIELDS
    -- ==========================================
    
    ALTER TABLE [dbo].[ParkingLocations]
    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;

    ALTER TABLE [dbo].[ParkingLocations]
    ADD [DeletedAt] DATETIME2(7) NULL;

    ALTER TABLE [dbo].[ParkingLocations]
    ADD [DeletedBy] UNIQUEIDENTIFIER NULL;

    PRINT '✓ Soft delete fields (IsDeleted, DeletedAt, DeletedBy) added'

END
ELSE
BEGIN
    PRINT 'Phase 2 Migration: Already applied (skipping)'
END
GO

-- ==========================================
-- 3. ADD PERFORMANCE INDEXES
-- ==========================================

-- Check if indexes need to be created
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_Latitude_Longitude' AND object_id = OBJECT_ID('ParkingLocations'))
BEGIN
    PRINT 'Creating geospatial index...'
    
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_Latitude_Longitude]
    ON [dbo].[ParkingLocations]([Latitude], [Longitude])
    INCLUDE ([IsActive], [AvailableSlots])
    WHERE [IsDeleted] = 0;
    
    PRINT '✓ Geospatial index created (Latitude, Longitude)'
END
ELSE
BEGIN
    PRINT 'Geospatial index already exists (skipping)'
END

-- Index for soft delete queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLocations_IsDeleted' AND object_id = OBJECT_ID('ParkingLocations'))
BEGIN
    PRINT 'Creating soft delete index...'
    
    CREATE NONCLUSTERED INDEX [IX_ParkingLocations_IsDeleted]
    ON [dbo].[ParkingLocations]([IsDeleted])
    INCLUDE ([IsActive], [AvailableSlots]);
    
    PRINT '✓ Soft delete index created'
END
ELSE
BEGIN
    PRINT 'Soft delete index already exists (skipping)'
END

PRINT '================================================'
PRINT 'Phase 2 Migration COMPLETED SUCCESSFULLY'
PRINT 'Summary:'
PRINT '  - Audit tracking fields added'
PRINT '  - Soft delete support added'
PRINT '  - Performance indexes created'
PRINT '  - Existing records preserved'
PRINT '================================================'
GO

-- ==========================================
-- VERIFICATION QUERIES
-- ==========================================

-- Verify new columns exist
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable AS IsNullable,
    dc.definition AS DefaultValue
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('ParkingLocations')
AND c.name IN ('CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
ORDER BY c.column_id;

-- Verify indexes
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('ParkingLocations')
AND i.name IN ('IX_ParkingLocations_Latitude_Longitude', 'IX_ParkingLocations_IsDeleted')
ORDER BY i.name, ic.key_ordinal;

-- Check existing records (should all have IsDeleted = 0)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParkingLocations') AND name = 'IsDeleted')
BEGIN
    SELECT 
        COUNT(*) AS TotalRecords,
        SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS ActiveRecords,
        SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedRecords
    FROM [dbo].[ParkingLocations];
    
    PRINT 'Verification complete - check results above'
END
ELSE
BEGIN
    PRINT 'IsDeleted column not found - migration may not have completed'
END
GO
