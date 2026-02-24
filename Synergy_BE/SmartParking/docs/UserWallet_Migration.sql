-- Migration: UserWallet + WalletTransaction
-- Date: 2026-02-24
-- Mô tả: Ví tiền cho từng user, dùng cho thanh toán booking, gia hạn, nạp tiền.

-- 1. Bảng UserWallets
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserWallets')
BEGIN
    CREATE TABLE [dbo].[UserWallets] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Balance] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserWallets] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_UserWallets_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
    );
END
GO

-- 2. Bảng WalletTransactions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WalletTransactions')
BEGIN
    CREATE TABLE [dbo].[WalletTransactions] (
        [WalletTransactionId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [BalanceAfter] DECIMAL(18,2) NOT NULL,
        [BookingId] UNIQUEIDENTIFIER NULL,
        [PaymentTransactionId] UNIQUEIDENTIFIER NULL,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_WalletTransactions] PRIMARY KEY ([WalletTransactionId]),
        CONSTRAINT [FK_WalletTransactions_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
    );
    CREATE INDEX [IX_WalletTransactions_UserId_CreatedAt] ON [dbo].[WalletTransactions]([UserId], [CreatedAt] DESC);
END
GO

-- 3. Khởi tạo ví cho users hiện có (balance = 0)
INSERT INTO [dbo].[UserWallets] ([UserId], [Balance], [UpdatedAt])
SELECT u.[UserId], 0, GETUTCDATE()
FROM [dbo].[Users] u
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[UserWallets] w WHERE w.[UserId] = u.[UserId]);
GO
