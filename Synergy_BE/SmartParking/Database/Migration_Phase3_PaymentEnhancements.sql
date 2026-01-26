-- =============================================
-- PHASE 3: Payment Integration Enhancements
-- Date: 2026-01-26
-- Description: Adds refund support, metadata, and audit fields to PaymentTransactions
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '========================================';
PRINT 'PHASE 3: Payment Enhancements';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '========================================';
PRINT '';

-- =============================================
-- STEP 1: Update CreatedAt to NOT NULL
-- =============================================
PRINT 'STEP 1: Updating CreatedAt column...';

IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') 
    AND name = 'CreatedAt'
    AND is_nullable = 1
)
BEGIN
    -- Set default for existing NULL values
    UPDATE [dbo].[PaymentTransactions]
    SET [CreatedAt] = GETUTCDATE()
    WHERE [CreatedAt] IS NULL;
    
    -- Make NOT NULL
    ALTER TABLE [dbo].[PaymentTransactions]
    ALTER COLUMN [CreatedAt] DATETIME2(7) NOT NULL;
    
    -- Add default constraint (only if it doesn't exist)
    IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_PaymentTransactions_CreatedAt' AND parent_object_id = OBJECT_ID('dbo.PaymentTransactions'))
    BEGIN
        ALTER TABLE [dbo].[PaymentTransactions]
        ADD CONSTRAINT [DF_PaymentTransactions_CreatedAt] DEFAULT (GETUTCDATE()) FOR [CreatedAt];
    END
    
    PRINT '✅ CreatedAt updated to NOT NULL with default';
END
ELSE
BEGIN
    PRINT '⚠️  CreatedAt already NOT NULL or does not exist';
END
PRINT '';
GO

-- =============================================
-- STEP 2: Add VNPay Additional Fields
-- =============================================
PRINT 'STEP 2: Adding VNPay additional fields...';

-- VnpBankCode
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'VnpBankCode')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [VnpBankCode] NVARCHAR(50) NULL;
    PRINT '✅ VnpBankCode column added';
END
ELSE PRINT '⚠️  VnpBankCode already exists';

-- VnpCardType
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'VnpCardType')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [VnpCardType] NVARCHAR(50) NULL;
    PRINT '✅ VnpCardType column added';
END
ELSE PRINT '⚠️  VnpCardType already exists';

PRINT '';
GO

-- =============================================
-- STEP 3: Add Refund Support Fields
-- =============================================
PRINT 'STEP 3: Adding refund support fields...';

-- RefundAmount
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundAmount')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [RefundAmount] DECIMAL(18, 2) NULL;
    PRINT '✅ RefundAmount column added';
END
ELSE PRINT '⚠️  RefundAmount already exists';

-- RefundReason
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundReason')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [RefundReason] NVARCHAR(500) NULL;
    PRINT '✅ RefundReason column added';
END
ELSE PRINT '⚠️  RefundReason already exists';

-- RefundedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundedAt')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [RefundedAt] DATETIME2(7) NULL;
    PRINT '✅ RefundedAt column added';
END
ELSE PRINT '⚠️  RefundedAt already exists';

-- RefundedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundedBy')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [RefundedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ RefundedBy column added';
END
ELSE PRINT '⚠️  RefundedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 4: Add Metadata Field (JSON)
-- =============================================
PRINT 'STEP 4: Adding metadata field...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'Metadata')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [Metadata] NVARCHAR(MAX) NULL;
    PRINT '✅ Metadata column added';
END
ELSE PRINT '⚠️  Metadata already exists';

PRINT '';
GO

-- =============================================
-- STEP 5: Add Audit Fields
-- =============================================
PRINT 'STEP 5: Adding audit fields (UpdatedAt, CreatedBy, UpdatedBy)...';

-- UpdatedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [UpdatedAt] DATETIME2(7) NULL;
    PRINT '✅ UpdatedAt column added';
END
ELSE PRINT '⚠️  UpdatedAt already exists';

-- CreatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [CreatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ CreatedBy column added';
END
ELSE PRINT '⚠️  CreatedBy already exists';

-- UpdatedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ UpdatedBy column added';
END
ELSE PRINT '⚠️  UpdatedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 6: Add Soft Delete Fields
-- =============================================
PRINT 'STEP 6: Adding soft delete fields (IsDeleted, DeletedAt, DeletedBy)...';

-- IsDeleted
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [IsDeleted] BIT NOT NULL DEFAULT 0;
    PRINT '✅ IsDeleted column added';
END
ELSE PRINT '⚠️  IsDeleted already exists';

-- DeletedAt
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [DeletedAt] DATETIME2(7) NULL;
    PRINT '✅ DeletedAt column added';
END
ELSE PRINT '⚠️  DeletedAt already exists';

-- DeletedBy
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'DeletedBy')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD [DeletedBy] UNIQUEIDENTIFIER NULL;
    PRINT '✅ DeletedBy column added';
END
ELSE PRINT '⚠️  DeletedBy already exists';

PRINT '';
GO

-- =============================================
-- STEP 7: Initialize Audit Fields
-- =============================================
PRINT 'STEP 7: Initializing audit fields for existing records...';

-- Set CreatedBy = UserId for existing records
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'CreatedBy')
BEGIN
    UPDATE [dbo].[PaymentTransactions]
    SET [CreatedBy] = [UserId]
    WHERE [CreatedBy] IS NULL;
    
    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    PRINT '✅ Updated ' + CAST(@UpdatedCount AS VARCHAR) + ' records with CreatedBy = UserId';
END

PRINT '';
GO

-- =============================================
-- STEP 8: Add Indexes for Performance
-- =============================================
PRINT 'STEP 8: Adding indexes...';

-- Index on VnpTxnRef (for callback lookup - unique)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentTransactions_VnpTxnRef' AND object_id = OBJECT_ID('dbo.PaymentTransactions'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PaymentTransactions_VnpTxnRef]
    ON [dbo].[PaymentTransactions] ([VnpTxnRef])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_PaymentTransactions_VnpTxnRef created';
END
ELSE PRINT '⚠️  Index IX_PaymentTransactions_VnpTxnRef already exists';

-- Index on PaymentStatus (for filtering)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentTransactions_PaymentStatus' AND object_id = OBJECT_ID('dbo.PaymentTransactions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_PaymentStatus]
    ON [dbo].[PaymentTransactions] ([PaymentStatus])
    INCLUDE ([Amount], [CreatedAt], [BookingId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_PaymentTransactions_PaymentStatus created';
END
ELSE PRINT '⚠️  Index IX_PaymentTransactions_PaymentStatus already exists';

-- Index on BookingId (for booking-payment lookup)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentTransactions_BookingId' AND object_id = OBJECT_ID('dbo.PaymentTransactions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_BookingId]
    ON [dbo].[PaymentTransactions] ([BookingId])
    INCLUDE ([PaymentStatus], [Amount], [CreatedAt])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_PaymentTransactions_BookingId created';
END
ELSE PRINT '⚠️  Index IX_PaymentTransactions_BookingId already exists';

-- Index on UserId + IsDeleted (for user's payments query)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentTransactions_UserId_IsDeleted' AND object_id = OBJECT_ID('dbo.PaymentTransactions'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_UserId_IsDeleted]
    ON [dbo].[PaymentTransactions] ([UserId], [IsDeleted])
    INCLUDE ([PaymentStatus], [Amount], [CreatedAt], [BookingId])
    WHERE [IsDeleted] = 0;
    
    PRINT '✅ Index IX_PaymentTransactions_UserId_IsDeleted created';
END
ELSE PRINT '⚠️  Index IX_PaymentTransactions_UserId_IsDeleted already exists';

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

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'VnpBankCode')
    INSERT INTO @MissingColumns VALUES ('VnpBankCode');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'VnpCardType')
    INSERT INTO @MissingColumns VALUES ('VnpCardType');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundAmount')
    INSERT INTO @MissingColumns VALUES ('RefundAmount');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundReason')
    INSERT INTO @MissingColumns VALUES ('RefundReason');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundedAt')
    INSERT INTO @MissingColumns VALUES ('RefundedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'RefundedBy')
    INSERT INTO @MissingColumns VALUES ('RefundedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'Metadata')
    INSERT INTO @MissingColumns VALUES ('Metadata');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'UpdatedAt')
    INSERT INTO @MissingColumns VALUES ('UpdatedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'CreatedBy')
    INSERT INTO @MissingColumns VALUES ('CreatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'UpdatedBy')
    INSERT INTO @MissingColumns VALUES ('UpdatedBy');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'IsDeleted')
    INSERT INTO @MissingColumns VALUES ('IsDeleted');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'DeletedAt')
    INSERT INTO @MissingColumns VALUES ('DeletedAt');
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'DeletedBy')
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
    WHERE TABLE_NAME = 'PaymentTransactions'
      AND COLUMN_NAME IN ('CreatedAt', 'VnpBankCode', 'VnpCardType', 'RefundAmount', 'RefundReason', 'RefundedAt', 'RefundedBy', 'Metadata', 'UpdatedAt', 'CreatedBy', 'UpdatedBy', 'IsDeleted', 'DeletedAt', 'DeletedBy')
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
WHERE i.object_id = OBJECT_ID('dbo.PaymentTransactions')
  AND i.name LIKE 'IX_PaymentTransactions_%'
ORDER BY i.name;

PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
PRINT '';

GO
