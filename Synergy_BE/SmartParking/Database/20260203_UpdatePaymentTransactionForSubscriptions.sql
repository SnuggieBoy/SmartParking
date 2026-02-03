-- Migration: Update PaymentTransactions for Owner Subscription Support
-- Date: 2026-02-03

-- 1. Make BookingId nullable
-- First, find the FK name (it might vary, but usually FK__PaymentTr__Booki__75A278F5 based on DbContext)
-- We will drop it and recreate it as nullable if needed, but in T-SQL ALTER COLUMN is enough if FK exists.
ALTER TABLE PaymentTransactions ALTER COLUMN BookingId UNIQUEIDENTIFIER NULL;
GO

-- 2. Add OwnerUpgradeRequestId column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'OwnerUpgradeRequestId')
BEGIN
    ALTER TABLE PaymentTransactions ADD OwnerUpgradeRequestId UNIQUEIDENTIFIER NULL;
END
GO

-- 3. Add PaymentType column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'PaymentType')
BEGIN
    ALTER TABLE PaymentTransactions ADD PaymentType NVARCHAR(20) NULL;
END
GO

-- 4. Add ForeignKey to OwnerUpgradeRequests
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentTransactions_OwnerUpgradeRequests')
BEGIN
    ALTER TABLE PaymentTransactions ADD CONSTRAINT FK_PaymentTransactions_OwnerUpgradeRequests 
        FOREIGN KEY (OwnerUpgradeRequestId) REFERENCES OwnerUpgradeRequests(RequestId);
END
GO

-- 5. Update existing records to have PaymentType = 'Booking'
UPDATE PaymentTransactions SET PaymentType = 'Booking' WHERE PaymentType IS NULL AND BookingId IS NOT NULL;
GO
