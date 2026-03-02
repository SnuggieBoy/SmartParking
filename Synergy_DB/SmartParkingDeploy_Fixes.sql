-- ============================================================
-- SmartParkingDeploy_Fixes.sql
-- Chạy trên SmartParkingDB (Azure SQL) - Kết nối trực tiếp vào SmartParkingDB
-- Sửa 2 lỗi: Location.STDistance + USE [master]
-- ============================================================

-- FIX 1: GetNearbyParkingLots - ParkingLots dùng Latitude/Longitude, KHÔNG có cột Location
-- Cần DROP và tạo lại procedure với logic dùng geography::Point từ Lat/Long
IF OBJECT_ID('dbo.GetNearbyParkingLots', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[GetNearbyParkingLots];
GO

CREATE PROCEDURE [dbo].[GetNearbyParkingLots]
    @Latitude FLOAT,
    @Longitude FLOAT,
    @RadiusMeters FLOAT
AS
BEGIN
    DECLARE @UserLocation GEOGRAPHY =
        geography::Point(@Latitude, @Longitude, 4326);

    SELECT
        pl.ParkingLotId,
        pl.Name,
        pl.Address,
        pl.PricePerHour,
        geography::Point(pl.Latitude, pl.Longitude, 4326).STDistance(@UserLocation) AS DistanceInMeters
    FROM [dbo].[ParkingLots] pl
    WHERE pl.Latitude IS NOT NULL
      AND pl.Longitude IS NOT NULL
      AND geography::Point(pl.Latitude, pl.Longitude, 4326).STDistance(@UserLocation) <= @RadiusMeters
      AND pl.Status = 'Available'
      AND (pl.IsDeleted = 0 OR pl.IsDeleted IS NULL)
    ORDER BY DistanceInMeters ASC;
END;
GO

-- FIX 2: Không cần USE [master] và ALTER DATABASE - Azure SQL không hỗ trợ
-- (Đã bỏ - chỉ cần kết nối trực tiếp vào SmartParkingDB)

PRINT 'SmartParkingDeploy_Fixes.sql - Hoàn tất sửa lỗi.';
