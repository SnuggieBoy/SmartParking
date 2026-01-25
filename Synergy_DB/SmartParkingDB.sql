/* =====================================================
   SMART PARKING – FINAL DATABASE SCHEMA
   Includes:
   - Auth (Local + Google)
   - JWT Refresh Token
   - Spatial Search
   - Booking
   - VNPay Payment
   - 5 Improvements Applied
   ===================================================== */

CREATE DATABASE SmartParkingDB;
GO
USE SmartParkingDB;
GO

/* =========================
   ROLES
   ========================= */
CREATE TABLE Roles (
    RoleId INT IDENTITY PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO Roles (RoleName)
VALUES ('Driver'), ('Owner'), ('Admin');

/* =========================
   USERS (PROFILE)
   ========================= */
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(20),
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

/* =========================
   USER AUTH (LOCAL + GOOGLE)
   ========================= */
CREATE TABLE UserAuth (
    AuthId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Provider NVARCHAR(20) CHECK (Provider IN ('Local', 'Google')),
    ProviderUserId NVARCHAR(200),
    PasswordHash NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_Provider UNIQUE (Provider, ProviderUserId)
);

/* =========================
   REFRESH TOKENS
   ========================= */
CREATE TABLE UserTokens (
    TokenId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RefreshToken NVARCHAR(255) NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsRevoked BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

/* =========================
   PARKING LOTS
   ========================= */
CREATE TABLE ParkingLots (
    ParkingLotId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100),
    Address NVARCHAR(255),
    Location GEOGRAPHY NOT NULL,
    TotalCapacity INT NOT NULL,
    CurrentOccupancy INT DEFAULT 0,
    PricePerHour DECIMAL(10,2),
    Status NVARCHAR(20) CHECK (Status IN ('Available','Full','Closed')),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    RowVersion ROWVERSION,
    FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    CHECK (CurrentOccupancy <= TotalCapacity)
);

CREATE SPATIAL INDEX IX_ParkingLots_Location
ON ParkingLots(Location);

/* =========================
   BOOKINGS
   ========================= */
CREATE TABLE Bookings (
    BookingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    ParkingLotId UNIQUEIDENTIFIER NOT NULL,
    BookingTime DATETIME2 DEFAULT GETDATE(),
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Active','Completed','Cancelled')),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    RowVersion ROWVERSION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ParkingLotId) REFERENCES ParkingLots(ParkingLotId)
);

CREATE INDEX IX_Bookings_UserId ON Bookings(UserId);

/* =========================
   PAYMENT TRANSACTIONS (VNPAY)
   ========================= */
CREATE TABLE PaymentTransactions (
    PaymentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BookingId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(20) DEFAULT 'VNPay',
    PaymentStatus NVARCHAR(20) CHECK (PaymentStatus IN ('Pending','Success','Failed')),
    VnpTxnRef NVARCHAR(100) NOT NULL UNIQUE,
    VnpTransactionNo NVARCHAR(100),
    VnpResponseCode NVARCHAR(10),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE INDEX IX_Payment_UserId ON PaymentTransactions(UserId);

/* =========================
   PAYMENT LOGS (CALLBACK AUDIT)
   ========================= */
CREATE TABLE PaymentLogs (
    LogId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    RawData NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (PaymentId) REFERENCES PaymentTransactions(PaymentId)
);

/* =========================
   STORED PROCEDURE – FIND NEARBY PARKING
   ========================= */
GO
CREATE PROCEDURE GetNearbyParkingLots
    @Latitude FLOAT,
    @Longitude FLOAT,
    @RadiusMeters FLOAT
AS
BEGIN
    DECLARE @UserLocation GEOGRAPHY =
        geography::Point(@Latitude, @Longitude, 4326);

    SELECT
        ParkingLotId,
        Name,
        Address,
        PricePerHour,
        Location.STDistance(@UserLocation) AS DistanceInMeters
    FROM ParkingLots
    WHERE Location.STDistance(@UserLocation) <= @RadiusMeters
      AND Status = 'Available'
    ORDER BY DistanceInMeters ASC;
END;
GO
