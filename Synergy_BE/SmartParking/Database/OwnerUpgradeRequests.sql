-- Script: Create OwnerUpgradeRequests table for P2P owner subscription flow
USE [SmartParkingDB];
GO

IF OBJECT_ID('dbo.OwnerUpgradeRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OwnerUpgradeRequests
    (
        RequestId            UNIQUEIDENTIFIER   NOT NULL CONSTRAINT PK_OwnerUpgradeRequests PRIMARY KEY,
        UserId               UNIQUEIDENTIFIER   NOT NULL,

        -- Snapshot of user info at the time of request
        FullNameSnapshot     NVARCHAR(100)      NOT NULL,
        EmailSnapshot        NVARCHAR(256)      NOT NULL,
        PhoneSnapshot        NVARCHAR(50)       NULL,

        -- Initial parking lot information proposed by the user
        ParkingLotName       NVARCHAR(100)      NOT NULL,
        ParkingLotAddress    NVARCHAR(255)      NOT NULL,
        Latitude             DECIMAL(9,6)       NULL,
        Longitude            DECIMAL(9,6)       NULL,

        -- Subscription plan
        PlanType             NVARCHAR(50)       NOT NULL,      -- e.g. 'Monthly', 'Yearly'
        FeeAmount            DECIMAL(18,2)      NOT NULL,      -- snapshot of fee at time of request

        -- Workflow status
        Status               NVARCHAR(50)       NOT NULL,      -- Pending, Approved, Rejected, Cancelled
        RejectReason         NVARCHAR(500)      NULL,

        -- Optional payment link (if integrated with PaymentTransactions)
        PaymentTransactionId UNIQUEIDENTIFIER   NULL,

        CreatedAt            DATETIME2(0)       NOT NULL CONSTRAINT DF_OwnerUpgradeRequests_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ApprovedAt           DATETIME2(0)       NULL,
        RejectedAt           DATETIME2(0)       NULL,
        ProcessedBy          UNIQUEIDENTIFIER   NULL
    );

    -- Relationships
    ALTER TABLE dbo.OwnerUpgradeRequests WITH CHECK
    ADD CONSTRAINT FK_OwnerUpgradeRequests_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

    -- Optional FK to PaymentTransactions if table exists
    IF OBJECT_ID('dbo.PaymentTransactions', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE dbo.OwnerUpgradeRequests WITH CHECK
        ADD CONSTRAINT FK_OwnerUpgradeRequests_PaymentTransactions
            FOREIGN KEY (PaymentTransactionId) REFERENCES dbo.PaymentTransactions(PaymentId);
    END;

    -- Indexes to help admin queries
    CREATE INDEX IX_OwnerUpgradeRequests_Status_CreatedAt
        ON dbo.OwnerUpgradeRequests (Status, CreatedAt DESC);

    CREATE INDEX IX_OwnerUpgradeRequests_UserId
        ON dbo.OwnerUpgradeRequests (UserId);
END
GO

