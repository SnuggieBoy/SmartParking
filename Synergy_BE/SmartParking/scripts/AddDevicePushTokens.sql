-- =============================================
-- Script: Thêm bảng DevicePushTokens để lưu Expo Push Token theo user
-- Dùng cho push notification qua Expo Push API
-- Chạy trên database SmartParking
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DevicePushTokens')
BEGIN
    CREATE TABLE [dbo].[DevicePushTokens] (
        [DevicePushTokenId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [ExpoPushToken] NVARCHAR(500) NOT NULL,
        [Platform] NVARCHAR(20) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [LastUsedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_DevicePushTokens] PRIMARY KEY ([DevicePushTokenId]),
        CONSTRAINT [FK_DevicePushTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_DevicePushTokens_UserId] ON [dbo].[DevicePushTokens] ([UserId]);
    CREATE UNIQUE INDEX [UQ_DevicePushTokens_UserId_Token] ON [dbo].[DevicePushTokens] ([UserId], [ExpoPushToken]) WHERE [IsActive] = 1;

    PRINT 'Bảng DevicePushTokens đã được tạo thành công.';
END
ELSE
BEGIN
    PRINT 'Bảng DevicePushTokens đã tồn tại.';
END
GO
