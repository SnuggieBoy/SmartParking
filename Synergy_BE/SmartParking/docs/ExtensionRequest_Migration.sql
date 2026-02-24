-- Migration: Add ExtensionRequest table for booking extension requests (owner approval flow)
-- Run this script against your SmartParking database

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExtensionRequests')
BEGIN
    CREATE TABLE [dbo].[ExtensionRequests] (
        [ExtensionRequestId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [BookingId] UNIQUEIDENTIFIER NOT NULL,
        [RequestedEndTime] DATETIME2 NOT NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        [RejectReason] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ProcessedAt] DATETIME2 NULL,
        [ProcessedBy] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_ExtensionRequests] PRIMARY KEY ([ExtensionRequestId]),
        CONSTRAINT [FK_ExtensionRequests_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings]([BookingId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ExtensionRequests_BookingId_Status] ON [dbo].[ExtensionRequests]([BookingId], [Status]);
END
GO
