-- =============================================================
-- SmartParking Database Migration V2.0
-- Add Favorites, Reviews, and Notifications tables
-- =============================================================

-- 1. Favorites Table
-- User can save favorite parking lots
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Favorites')
BEGIN
    CREATE TABLE Favorites (
        FavoriteId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        ParkingLotId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        
        CONSTRAINT PK_Favorites PRIMARY KEY (FavoriteId),
        CONSTRAINT FK_Favorites_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_Favorites_ParkingLots FOREIGN KEY (ParkingLotId) REFERENCES ParkingLots(ParkingLotId) ON DELETE CASCADE,
        CONSTRAINT UQ_Favorites_User_ParkingLot UNIQUE (UserId, ParkingLotId)
    );

    CREATE INDEX IX_Favorites_UserId ON Favorites(UserId);
    CREATE INDEX IX_Favorites_ParkingLotId ON Favorites(ParkingLotId);
    
    PRINT 'Created Favorites table';
END
GO

-- 2. Reviews Table
-- User reviews for parking lots (one per booking)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE Reviews (
        ReviewId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        ParkingLotId UNIQUEIDENTIFIER NOT NULL,
        BookingId UNIQUEIDENTIFIER NOT NULL,
        Rating INT NOT NULL,
        Comment NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        
        CONSTRAINT PK_Reviews PRIMARY KEY (ReviewId),
        CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT FK_Reviews_ParkingLots FOREIGN KEY (ParkingLotId) REFERENCES ParkingLots(ParkingLotId),
        CONSTRAINT FK_Reviews_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
        CONSTRAINT CK_Reviews_Rating CHECK (Rating >= 1 AND Rating <= 5)
    );

    CREATE UNIQUE INDEX UQ_Reviews_BookingId ON Reviews(BookingId) WHERE IsDeleted = 0;
    CREATE INDEX IX_Reviews_ParkingLotId_IsDeleted ON Reviews(ParkingLotId, IsDeleted) WHERE IsDeleted = 0;
    CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
    
    PRINT 'Created Reviews table';
END
GO

-- 3. Notifications Table
-- User notifications (personal + broadcast)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        NotificationId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NULL, -- NULL = broadcast to all
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(2000) NOT NULL,
        Type NVARCHAR(50) NOT NULL DEFAULT 'Info', -- Info, Success, Warning, Error, Booking, Payment, System
        Data NVARCHAR(4000) NULL, -- JSON data
        IsRead BIT NOT NULL DEFAULT 0,
        ReadAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy UNIQUEIDENTIFIER NULL, -- Admin who sent broadcast
        
        CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId),
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );

    CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
    CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt DESC);
    
    PRINT 'Created Notifications table';
END
GO

-- =============================================================
-- Summary of changes:
-- 1. Favorites - User can save/remove favorite parking lots
-- 2. Reviews - User can rate and comment on parking lots after booking
-- 3. Notifications - System can send notifications to users
-- =============================================================

PRINT 'Migration V2.0 completed successfully';
GO
