-- =============================================
-- SMARTPARKING - SEPAY INTEGRATION (COMPLETE)
-- =============================================
-- Date: 2026-01-27
-- Version: 2.0 (Production-Ready)
-- Description: Complete SePay integration for SmartParking system
--              This script adds all necessary database changes for SePay payment support
--
-- USAGE:
-- 1. Backup your database first!
-- 2. Run this script on SmartParkingDB
-- 3. Verify results at the end
-- 4. Update C# code and appsettings (see docs/)
--
-- IDEMPOTENT: Safe to run multiple times
-- =============================================

USE [SmartParkingDB]
GO

SET NOCOUNT ON;
PRINT '';
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║                                                                ║';
PRINT '║         SMARTPARKING - SEPAY INTEGRATION SCRIPT               ║';
PRINT '║                    Version 2.0                                 ║';
PRINT '║                                                                ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';
PRINT 'Date: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- =============================================
-- SAFETY CHECK
-- =============================================
PRINT '>>> Safety Check: Verifying database...';
PRINT '';

IF DB_NAME() != 'SmartParkingDB'
BEGIN
    PRINT '❌ ERROR: Wrong database! Expected SmartParkingDB, current: ' + DB_NAME();
    PRINT '   Please run: USE [SmartParkingDB]';
    RAISERROR('Wrong database', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PaymentTransactions')
BEGIN
    PRINT '❌ ERROR: PaymentTransactions table not found!';
    PRINT '   Please run the base SmartParkingFull.sql first.';
    RAISERROR('PaymentTransactions table not found', 16, 1);
    RETURN;
END

PRINT '✅ Safety check passed';
PRINT '';

-- =============================================
-- PART 1: ADD SEPAY COLUMNS TO PAYMENTTRANSACTIONS
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  PART 1: Add SePay Columns to PaymentTransactions             ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

DECLARE @ColumnsAdded INT = 0;

-- SePayOrderId
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayOrderId')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayOrderId] NVARCHAR(100) NULL;
    PRINT '✅ Added column: SePayOrderId';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayOrderId';

-- SePayTransactionId
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayTransactionId')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayTransactionId] NVARCHAR(100) NULL;
    PRINT '✅ Added column: SePayTransactionId';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayTransactionId';

-- SePayBankCode
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayBankCode')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayBankCode] NVARCHAR(50) NULL;
    PRINT '✅ Added column: SePayBankCode';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayBankCode';

-- SePayBankAccount
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayBankAccount')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayBankAccount] NVARCHAR(50) NULL;
    PRINT '✅ Added column: SePayBankAccount';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayBankAccount';

-- SePayTransferContent
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayTransferContent')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayTransferContent] NVARCHAR(200) NULL;
    PRINT '✅ Added column: SePayTransferContent';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayTransferContent';

-- SePayQrCode
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayQrCode')
BEGIN
    ALTER TABLE [dbo].[PaymentTransactions] ADD [SePayQrCode] NVARCHAR(MAX) NULL;
    PRINT '✅ Added column: SePayQrCode';
    SET @ColumnsAdded = @ColumnsAdded + 1;
END
ELSE
    PRINT '⚠️  Column already exists: SePayQrCode';

PRINT '';
PRINT 'Summary: Added ' + CAST(@ColumnsAdded AS VARCHAR) + ' new column(s)';
PRINT '';

-- =============================================
-- PART 2: CREATE SEPAYWEBHOOKLOGS TABLE
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  PART 2: Create SePayWebhookLogs Table                        ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SePayWebhookLogs')
BEGIN
    CREATE TABLE [dbo].[SePayWebhookLogs] (
        [LogId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [OrderId] NVARCHAR(100) NULL,
        [TransactionId] NVARCHAR(100) NULL,
        [Amount] DECIMAL(18, 2) NULL,
        [Status] NVARCHAR(50) NULL,
        [RawPayload] NVARCHAR(MAX) NULL,
        [Signature] NVARCHAR(500) NULL,
        [IsVerified] BIT NOT NULL DEFAULT 0,
        [IsProcessed] BIT NOT NULL DEFAULT 0,
        [ProcessedAt] DATETIME2(7) NULL,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        [IpAddress] NVARCHAR(50) NULL
    );
    
    PRINT '✅ Created table: SePayWebhookLogs';
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_SePayWebhookLogs_OrderId]
    ON [dbo].[SePayWebhookLogs] ([OrderId] ASC)
    INCLUDE ([Status], [IsProcessed], [CreatedAt])
    WHERE ([IsProcessed] = 0);
    
    PRINT '✅ Created index: IX_SePayWebhookLogs_OrderId';
    
    CREATE NONCLUSTERED INDEX [IX_SePayWebhookLogs_TransactionId]
    ON [dbo].[SePayWebhookLogs] ([TransactionId] ASC);
    
    PRINT '✅ Created index: IX_SePayWebhookLogs_TransactionId';
END
ELSE
BEGIN
    PRINT '⚠️  Table already exists: SePayWebhookLogs';
END

PRINT '';

-- =============================================
-- PART 3: CREATE INDEX FOR SEPAYORDERID
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  PART 3: Create Index for SePayOrderId                        ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'SePayOrderId')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PaymentTransactions_SePayOrderId')
    BEGIN
        -- Check if IsDeleted column exists for filtered index
        IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PaymentTransactions') AND name = 'IsDeleted')
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_SePayOrderId]
            ON [dbo].[PaymentTransactions] ([SePayOrderId] ASC)
            WHERE ([SePayOrderId] IS NOT NULL AND [IsDeleted] = 0);
            
            PRINT '✅ Created index: IX_PaymentTransactions_SePayOrderId (with IsDeleted filter)';
        END
        ELSE
        BEGIN
            CREATE NONCLUSTERED INDEX [IX_PaymentTransactions_SePayOrderId]
            ON [dbo].[PaymentTransactions] ([SePayOrderId] ASC)
            WHERE ([SePayOrderId] IS NOT NULL);
            
            PRINT '✅ Created index: IX_PaymentTransactions_SePayOrderId (without IsDeleted filter)';
        END
    END
    ELSE
    BEGIN
        PRINT '⚠️  Index already exists: IX_PaymentTransactions_SePayOrderId';
    END
END
ELSE
BEGIN
    PRINT '❌ Cannot create index: SePayOrderId column does not exist';
END

PRINT '';

-- =============================================
-- PART 4: UPDATE PAYMENTMETHOD CONSTRAINT
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║  PART 4: Update PaymentMethod Constraint                      ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

-- Find existing PaymentMethod constraint
DECLARE @ConstraintName NVARCHAR(255);
SELECT @ConstraintName = name
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.PaymentTransactions')
  AND definition LIKE '%PaymentMethod%';

IF @ConstraintName IS NOT NULL
BEGIN
    -- Drop old constraint
    DECLARE @DropSQL NVARCHAR(MAX) = 'ALTER TABLE [dbo].[PaymentTransactions] DROP CONSTRAINT [' + @ConstraintName + '];';
    EXEC sp_executesql @DropSQL;
    PRINT '✅ Dropped old constraint: ' + @ConstraintName;
    
    -- Add new constraint including SePay
    ALTER TABLE [dbo].[PaymentTransactions]
    ADD CONSTRAINT [CK_PaymentTransactions_PaymentMethod]
    CHECK ([PaymentMethod] IN ('VNPay', 'SePay', 'Cash', 'Wallet'));
    
    PRINT '✅ Created new constraint: CK_PaymentTransactions_PaymentMethod (includes SePay)';
END
ELSE
BEGIN
    -- Create constraint if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PaymentTransactions_PaymentMethod')
    BEGIN
        ALTER TABLE [dbo].[PaymentTransactions]
        ADD CONSTRAINT [CK_PaymentTransactions_PaymentMethod]
        CHECK ([PaymentMethod] IN ('VNPay', 'SePay', 'Cash', 'Wallet'));
        
        PRINT '✅ Created constraint: CK_PaymentTransactions_PaymentMethod';
    END
    ELSE
    BEGIN
        PRINT '⚠️  Constraint already exists: CK_PaymentTransactions_PaymentMethod';
    END
END

PRINT '';

-- =============================================
-- VERIFICATION
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║                      VERIFICATION                              ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

-- Check SePay columns
PRINT '>>> Checking SePay columns in PaymentTransactions:';
PRINT '';

DECLARE @SePayColumnCount INT;
SELECT @SePayColumnCount = COUNT(*)
FROM sys.columns
WHERE object_id = OBJECT_ID('dbo.PaymentTransactions')
  AND name LIKE 'SePay%';

IF @SePayColumnCount = 6
BEGIN
    PRINT '✅ All 6 SePay columns exist';
    
    SELECT 
        '  - ' + c.name AS ColumnName,
        t.name AS DataType,
        CASE WHEN c.is_nullable = 1 THEN 'NULL' ELSE 'NOT NULL' END AS Nullable
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.PaymentTransactions')
      AND c.name LIKE 'SePay%'
    ORDER BY c.name;
END
ELSE
BEGIN
    PRINT '❌ Expected 6 SePay columns, found: ' + CAST(@SePayColumnCount AS VARCHAR);
END

PRINT '';

-- Check SePayWebhookLogs table
PRINT '>>> Checking SePayWebhookLogs table:';
PRINT '';

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SePayWebhookLogs')
BEGIN
    PRINT '✅ SePayWebhookLogs table exists';
    
    SELECT 
        COUNT(*) AS ColumnCount
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.SePayWebhookLogs');
END
ELSE
BEGIN
    PRINT '❌ SePayWebhookLogs table does NOT exist';
END

PRINT '';

-- Check indexes
PRINT '>>> Checking SePay indexes:';
PRINT '';

SELECT 
    '  - ' + t.name + '.' + i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name LIKE '%SePay%'
ORDER BY t.name, i.name;

DECLARE @IndexCount INT;
SELECT @IndexCount = COUNT(*)
FROM sys.indexes
WHERE name LIKE '%SePay%';

IF @IndexCount >= 3
    PRINT '';
    PRINT '✅ Expected indexes created (' + CAST(@IndexCount AS VARCHAR) + ' found)';
ELSE
    PRINT '';
    PRINT '⚠️  Expected at least 3 indexes, found: ' + CAST(@IndexCount AS VARCHAR);

PRINT '';

-- Check PaymentMethod constraint
PRINT '>>> Checking PaymentMethod constraint:';
PRINT '';

IF EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_PaymentTransactions_PaymentMethod'
      AND definition LIKE '%SePay%'
)
BEGIN
    PRINT '✅ PaymentMethod constraint includes SePay';
    
    SELECT 
        '  ' + name AS ConstraintName,
        definition AS Definition
    FROM sys.check_constraints
    WHERE name = 'CK_PaymentTransactions_PaymentMethod';
END
ELSE
BEGIN
    PRINT '⚠️  PaymentMethod constraint may not include SePay';
END

PRINT '';

-- =============================================
-- FINAL SUMMARY
-- =============================================
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║                      MIGRATION SUMMARY                         ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

IF @SePayColumnCount = 6 
   AND EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SePayWebhookLogs')
   AND @IndexCount >= 3
BEGIN
    PRINT '✅✅✅ MIGRATION COMPLETED SUCCESSFULLY! ✅✅✅';
    PRINT '';
    PRINT 'Database changes:';
    PRINT '  ✅ 6 SePay columns added to PaymentTransactions';
    PRINT '  ✅ SePayWebhookLogs table created';
    PRINT '  ✅ Indexes created for performance';
    PRINT '  ✅ PaymentMethod constraint updated';
    PRINT '';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'NEXT STEPS:';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '';
    PRINT '1. UPDATE C# CODE:';
    PRINT '   - Copy SePay service files';
    PRINT '   - Update PaymentController';
    PRINT '   - Update Domain entities';
    PRINT '';
    PRINT '2. CONFIGURE ENVIRONMENT VARIABLES:';
    PRINT '   PowerShell:';
    PRINT '     $env:SEPAY_API_KEY = "your-api-key"';
    PRINT '     $env:SEPAY_WEBHOOK_SECRET = "your-32-char-secret"';
    PRINT '';
    PRINT '   Linux/Mac:';
    PRINT '     export SEPAY_API_KEY="your-api-key"';
    PRINT '     export SEPAY_WEBHOOK_SECRET="your-32-char-secret"';
    PRINT '';
    PRINT '3. UPDATE appsettings.Development.json:';
    PRINT '   - Configure Bank settings (Code, AccountNumber, AccountName)';
    PRINT '   - Set WebhookUrl';
    PRINT '   - Leave ApiKey and WebhookSecret EMPTY (use env vars)';
    PRINT '';
    PRINT '4. REBUILD & RUN:';
    PRINT '   dotnet build';
    PRINT '   dotnet run';
    PRINT '';
    PRINT '5. VERIFY STARTUP LOGS:';
    PRINT '   Should see: "SePay configuration validated successfully"';
    PRINT '';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT 'DOCUMENTATION:';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
    PRINT '  - Quick Setup: docs/SEPAY_QUICK_SETUP.md';
    PRINT '  - Environment Variables: docs/SEPAY_ENVIRONMENT_VARIABLES.md';
    PRINT '  - Production Deployment: docs/SEPAY_PRODUCTION_DEPLOYMENT.md';
    PRINT '  - Testing Guide: TESTING_GUIDE.md (Section 7B)';
    PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
END
ELSE
BEGIN
    PRINT '⚠️⚠️⚠️ MIGRATION INCOMPLETE ⚠️⚠️⚠️';
    PRINT '';
    PRINT 'Issues detected:';
    
    IF @SePayColumnCount != 6
        PRINT '  ❌ Expected 6 SePay columns, found: ' + CAST(@SePayColumnCount AS VARCHAR);
    
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SePayWebhookLogs')
        PRINT '  ❌ SePayWebhookLogs table not created';
    
    IF @IndexCount < 3
        PRINT '  ❌ Expected at least 3 indexes, found: ' + CAST(@IndexCount AS VARCHAR);
    
    PRINT '';
    PRINT 'Please review error messages above and re-run the script.';
END

PRINT '';
PRINT '╔════════════════════════════════════════════════════════════════╗';
PRINT '║                    SCRIPT COMPLETED                            ║';
PRINT '╚════════════════════════════════════════════════════════════════╝';
PRINT '';

GO
