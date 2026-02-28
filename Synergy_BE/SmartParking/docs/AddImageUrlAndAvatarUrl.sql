-- Add ImageUrl to ParkingLots and AvatarUrl to Users for Cloudinary integration
-- Run this script against SmartParkingDB if you haven't used EF migrations

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ParkingLots') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE ParkingLots ADD ImageUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD AvatarUrl NVARCHAR(500) NULL;
END
GO
