-- 1. Thêm cột RejectReason nếu chưa có
IF COL_LENGTH('dbo.ParkingLots', 'RejectReason') IS NULL
BEGIN
    ALTER TABLE dbo.ParkingLots
    ADD RejectReason NVARCHAR(500) NULL;
END;
GO

-- 2. Chuẩn hoá Status cho các bản ghi cũ (nếu Status đang NULL/empty)
UPDATE dbo.ParkingLots
SET Status = 'Approved'
WHERE (Status IS NULL OR LTRIM(RTRIM(Status)) = '')
  AND (IsDeleted = 0 OR IsDeleted IS NULL);
GO

-- 3. Đảm bảo IsActive không NULL
UPDATE dbo.ParkingLots
SET IsActive = 1
WHERE IsActive IS NULL
  AND Status = 'Approved'
  AND (IsDeleted = 0 OR IsDeleted IS NULL);
GO