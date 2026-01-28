USE [SmartParkingDB];
GO

-- 1. Cho phép BookingId NULL (để dùng cho các payment không gắn booking, ví dụ Owner subscription)
ALTER TABLE dbo.PaymentTransactions
ALTER COLUMN BookingId UNIQUEIDENTIFIER NULL;
GO

-- 2. Thêm cột OwnerUpgradeRequestId nếu chưa có
IF COL_LENGTH('dbo.PaymentTransactions', 'OwnerUpgradeRequestId') IS NULL
BEGIN
    ALTER TABLE dbo.PaymentTransactions
    ADD OwnerUpgradeRequestId UNIQUEIDENTIFIER NULL;
END;
GO

-- 3. Thêm FK tới OwnerUpgradeRequests (nếu bảng tồn tại)
IF OBJECT_ID('dbo.OwnerUpgradeRequests', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_PaymentTransactions_OwnerUpgradeRequests'
          AND parent_object_id = OBJECT_ID('dbo.PaymentTransactions')
    )
    BEGIN
        ALTER TABLE dbo.PaymentTransactions WITH CHECK
        ADD CONSTRAINT FK_PaymentTransactions_OwnerUpgradeRequests
            FOREIGN KEY (OwnerUpgradeRequestId) REFERENCES dbo.OwnerUpgradeRequests(RequestId);
    END;
END;
GO

-- 4. Index hỗ trợ truy vấn theo OwnerUpgradeRequestId
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PaymentTransactions_OwnerUpgradeRequestId'
      AND object_id = OBJECT_ID('dbo.PaymentTransactions')
)
BEGIN
    CREATE INDEX IX_PaymentTransactions_OwnerUpgradeRequestId
        ON dbo.PaymentTransactions (OwnerUpgradeRequestId);
END;
GO