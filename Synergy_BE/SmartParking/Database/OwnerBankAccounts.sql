-- Script: Create OwnerBankAccounts table for linking Owner bank accounts
USE [SmartParkingDB];
GO

IF OBJECT_ID('dbo.OwnerBankAccounts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OwnerBankAccounts
    (
        OwnerBankAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OwnerBankAccounts PRIMARY KEY,
        UserId             UNIQUEIDENTIFIER NOT NULL,

        BankCode           NVARCHAR(20)     NOT NULL,
        BankName           NVARCHAR(100)    NOT NULL,
        AccountNumber      NVARCHAR(50)     NOT NULL,
        AccountHolderName  NVARCHAR(100)    NOT NULL,

        IsDefault          BIT              NOT NULL CONSTRAINT DF_OwnerBankAccounts_IsDefault DEFAULT (0),
        IsVerified         BIT              NOT NULL CONSTRAINT DF_OwnerBankAccounts_IsVerified DEFAULT (0),

        CreatedAt          DATETIME2(0)     NOT NULL CONSTRAINT DF_OwnerBankAccounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2(0)     NULL,
        VerifiedAt         DATETIME2(0)     NULL,
        VerifiedBy         UNIQUEIDENTIFIER NULL
    );

    ALTER TABLE dbo.OwnerBankAccounts WITH CHECK
    ADD CONSTRAINT FK_OwnerBankAccounts_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId);

    CREATE INDEX IX_OwnerBankAccounts_UserId
        ON dbo.OwnerBankAccounts (UserId);

    CREATE INDEX IX_OwnerBankAccounts_UserId_IsDefault
        ON dbo.OwnerBankAccounts (UserId, IsDefault);
END
GO

