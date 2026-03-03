-- Add ImageUrl and Description to OwnerUpgradeRequests for owner registration flow
-- Run this script against SmartParkingDB

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OwnerUpgradeRequests') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE OwnerUpgradeRequests ADD ImageUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OwnerUpgradeRequests') AND name = 'Description')
BEGIN
    ALTER TABLE OwnerUpgradeRequests ADD [Description] NVARCHAR(1000) NULL;
END
GO
