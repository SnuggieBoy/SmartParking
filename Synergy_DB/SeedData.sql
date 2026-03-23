-- =====================================================
-- SYNERGY SMART PARKING - COMPREHENSIVE SEED DATA
-- Generated: 2026-03-23
-- Run on: SmartParkingDB (Azure SQL / SQL Server)
-- Password for ALL users: 123456
-- =====================================================
-- IMPORTANT: Run this script on a CLEAN database (after migrations).
-- This script will DELETE all existing data and re-insert clean seed data.
-- =====================================================

BEGIN TRANSACTION;
BEGIN TRY

-- =====================================================
-- PHASE 1: CLEANUP (Disable FKs safely, delete, re-enable for Azure DB)
-- =====================================================
DECLARE @sql NVARCHAR(MAX) = N'';

-- Disable foreign key constraints
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id;
EXEC sp_executesql @sql;

DELETE FROM [dbo].[WalletTransactions];
DELETE FROM [dbo].[UserWallets];
DELETE FROM [dbo].[PaymentLogs];
DELETE FROM [dbo].[Reviews];
DELETE FROM [dbo].[ExtensionRequests];
DELETE FROM [dbo].[PaymentTransactions];
DELETE FROM [dbo].[Bookings];
DELETE FROM [dbo].[Favorites];
DELETE FROM [dbo].[Notifications];
DELETE FROM [dbo].[ParkingLocations];
DELETE FROM [dbo].[ParkingLots];
DELETE FROM [dbo].[OwnerUpgradeRequests];
DELETE FROM [dbo].[OwnerBankAccounts];
DELETE FROM [dbo].[Vehicles];
DELETE FROM [dbo].[DevicePushTokens];
DELETE FROM [dbo].[SePayWebhookLogs];
DELETE FROM [dbo].[UserTokens];
DELETE FROM [dbo].[UserAuth];
DELETE FROM [dbo].[Users];
DELETE FROM [dbo].[EmailOtps];

-- Enable foreign key constraints
SET @sql = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id;
EXEC sp_executesql @sql;

PRINT 'Phase 1: Cleanup completed.';

-- =====================================================
-- PHASE 2: ROLES (Ensure roles exist)
-- =====================================================
SET IDENTITY_INSERT [dbo].[Roles] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 1)
    INSERT [dbo].[Roles] ([RoleId], [RoleName]) VALUES (1, N'User');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 2)
    INSERT [dbo].[Roles] ([RoleId], [RoleName]) VALUES (2, N'Owner');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleId = 3)
    INSERT [dbo].[Roles] ([RoleId], [RoleName]) VALUES (3, N'Admin');
SET IDENTITY_INSERT [dbo].[Roles] OFF;

PRINT 'Phase 2: Roles ready.';

-- =====================================================
-- PHASE 3: USERS (25 total: 1 Admin + 14 Drivers + 10 Owners)
-- =====================================================
-- BCrypt hash of "123456" (cost=11)
DECLARE @PwHash NVARCHAR(200) = N'$2a$11$JL/bzu566B0xBFuoYy7zWOaPnXKV80yQGgtxW7LFguthR5TWxZ13m';
DECLARE @Now DATETIME2 = GETUTCDATE();

-- === ADMIN (1 user) ===
-- 1. Snuggie (Admin)
INSERT [dbo].[Users] ([UserId],[FullName],[Email],[Phone],[RoleId],[IsActive],[AvatarUrl],[EmailConfirmed],[CreatedAt])
VALUES (N'9e5e38a3-b7a1-4252-846c-27c0ebdc7b59', N'Snuggie', N'hieultse161727@fpt.edu.vn', N'0986515253', 3, 1, NULL, 1, @Now);

-- === OWNERS (10 users, RoleId=2) ===
-- 2. Vũ Thanh Ngân
INSERT [dbo].[Users] VALUES (N'937b02b0-e8be-440e-a1b0-005aee94dcfd', N'Vũ Thanh Ngân', N'vtngan270902@gmail.com', N'0979854888', 2, 1, NULL, 1, @Now);
-- 3. Trọng Hiếu
INSERT [dbo].[Users] VALUES (N'5a038829-3f3e-4e19-8215-7d13b915fd79', N'Trọng Hiếu', N'tronghieu848@gmail.com', N'0986515254', 2, 1, NULL, 1, @Now);
-- 4. Vũ Hoàng Hiệp
INSERT [dbo].[Users] VALUES (N'c1607535-2789-42b2-b2d1-c65ff65c154b', N'Vũ Hoàng Hiệp', N'vuhoangh247@gmail.com', N'0327844234', 2, 1, NULL, 1, @Now);
-- 5. Nguyen Anh Hung (Owner account)
INSERT [dbo].[Users] VALUES (N'af0652c8-1df8-440c-83f1-f965c2885439', N'Nguyen Anh Hung', N'nguyenanhhung403@gmail.com', N'0868205404', 2, 1, NULL, 1, @Now);
-- 6. Vũ Thị Ngọc Ánh
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111106', N'Vũ Thị Ngọc Ánh', N'anhvu120702@gmail.com', N'0901234506', 2, 1, NULL, 1, @Now);
-- 7. Trần Minh Quân
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111107', N'Trần Minh Quân', N'tranminhquan99@gmail.com', N'0387654321', 2, 1, NULL, 1, @Now);
-- 8. Lê Hoàng Phúc
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111108', N'Lê Hoàng Phúc', N'lehoangphuc2003@gmail.com', N'0971234567', 2, 1, NULL, 1, @Now);
-- 9. Phạm Đức Anh
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111109', N'Phạm Đức Anh', N'phamducanh.bk@gmail.com', N'0334567890', 2, 1, NULL, 1, @Now);
-- 10. Đặng Ngọc Hân
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111110', N'Đặng Ngọc Hân', N'dangngochan.dev@gmail.com', N'0765432198', 2, 1, NULL, 1, @Now);
-- 11. Hoàng Thị Lan Anh
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-1111-4aaa-b111-111111111111', N'Hoàng Thị Lan Anh', N'hoangthilananh01@gmail.com', N'0956789012', 2, 1, NULL, 1, @Now);

-- === DRIVERS (14 users, RoleId=1) ===
-- 12. Nguyễn Anh Hùng (Driver account)
INSERT [dbo].[Users] VALUES (N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'Nguyễn Anh Hùng', N'hungnase180159@fpt.edu.vn', N'0868205403', 1, 1, NULL, 1, @Now);
-- 13. Thầy Huy
INSERT [dbo].[Users] VALUES (N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', N'Thầy Huy', N'aroa1lqd@gmail.com', N'0912345678', 1, 1, NULL, 1, @Now);
-- 14. Bùi Xuân Duy
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222214', N'Bùi Xuân Duy', N'Suangusi113@gmail.com', N'0908765432', 1, 1, NULL, 1, @Now);
-- 15. Nguyễn Hoàng Trường Duy
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222215', N'Nguyễn Hoàng Trường Duy', N'duy08201@gmail.com', N'0345678901', 1, 1, NULL, 1, @Now);
-- 16. Nguyễn Thị Minh Thảo
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222216', N'Nguyễn Thị Minh Thảo', N'nguyenthiminhthao2003@gmail.com', N'0876543210', 1, 1, NULL, 1, @Now);
-- 17. Lê Văn Tùng
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222217', N'Lê Văn Tùng', N'levantung.hcm@gmail.com', N'0923456789', 1, 1, NULL, 1, @Now);
-- 18. Ngô Quốc Bảo
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222218', N'Ngô Quốc Bảo', N'ngoquocbao2004@gmail.com', N'0356789012', 1, 1, NULL, 1, @Now);
-- 19. Trương Thị Hồng Nhung
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222219', N'Trương Thị Hồng Nhung', N'truonghongnhung@gmail.com', N'0789012345', 1, 1, NULL, 1, @Now);
-- 20. Huỳnh Tấn Phát
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222220', N'Huỳnh Tấn Phát', N'huynhtanphat.it@gmail.com', N'0967890123', 1, 1, NULL, 1, @Now);
-- 21. Võ Minh Khôi
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222221', N'Võ Minh Khôi', N'vominhkhoi99@gmail.com', N'0398765432', 1, 1, NULL, 1, @Now);
-- 22. Đỗ Thị Thanh Trúc
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222222', N'Đỗ Thị Thanh Trúc', N'dothithanhtruc@gmail.com', N'0701234567', 1, 1, NULL, 1, @Now);
-- 23. Phan Anh Kiệt
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222223', N'Phan Anh Kiệt', N'phananhkiet.dev@gmail.com', N'0934567890', 1, 1, NULL, 1, @Now);
-- 24. Trần Thị Bích Ngọc
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222224', N'Trần Thị Bích Ngọc', N'tranbichngoc2003@gmail.com', N'0812345678', 1, 1, NULL, 1, @Now);
-- 25. Lý Thanh Sơn
INSERT [dbo].[Users] VALUES (N'a1b2c3d4-2222-4bbb-b222-222222222225', N'Lý Thanh Sơn', N'lythanhson.hcm@gmail.com', N'0378901234', 1, 1, NULL, 1, @Now);

PRINT 'Phase 3: 25 Users inserted (1 Admin + 10 Owners + 14 Drivers).';

-- =====================================================
-- PHASE 4: USER AUTH (password = "123456" for all)
-- =====================================================
INSERT [dbo].[UserAuth] ([AuthId],[UserId],[Provider],[ProviderUserId],[PasswordHash],[CreatedAt]) 
SELECT NEWID(), UserId, N'Local', CAST(UserId AS NVARCHAR(50)), @PwHash, @Now 
FROM [dbo].[Users];

PRINT 'Phase 4: UserAuth for 25 users inserted.';

-- =====================================================
-- PHASE 5: VEHICLES (for Drivers, 1-2 vehicles each)
-- =====================================================
INSERT [dbo].[Vehicles] ([VehicleId],[UserId],[LicensePlate],[VehicleType],[Brand],[Model],[Color],[IsActive],[CreatedAt],[UpdatedAt]) VALUES
-- Driver: Nguyễn Anh Hùng
(N'b1000001-0001-4ccc-c001-000000000001', N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'59F1-12345', 0, N'Honda', N'Vision', N'Đen', 1, @Now, NULL),
-- Driver: Thầy Huy
(N'b1000001-0001-4ccc-c001-000000000002', N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', N'51G-98765', 1, N'Toyota', N'Camry', N'Trắng', 1, @Now, NULL),
-- Driver: Bùi Xuân Duy
(N'b1000001-0001-4ccc-c001-000000000003', N'a1b2c3d4-2222-4bbb-b222-222222222214', N'59C2-34567', 0, N'Yamaha', N'Exciter', N'Xanh', 1, @Now, NULL),
-- Driver: Nguyễn Hoàng Trường Duy
(N'b1000001-0001-4ccc-c001-000000000004', N'a1b2c3d4-2222-4bbb-b222-222222222215', N'60A-11223', 1, N'Mazda', N'CX-5', N'Đỏ', 1, @Now, NULL),
-- Driver: Nguyễn Thị Minh Thảo
(N'b1000001-0001-4ccc-c001-000000000005', N'a1b2c3d4-2222-4bbb-b222-222222222216', N'59S1-55667', 0, N'Honda', N'SH 150i', N'Trắng', 1, @Now, NULL),
-- Driver: Lê Văn Tùng
(N'b1000001-0001-4ccc-c001-000000000006', N'a1b2c3d4-2222-4bbb-b222-222222222217', N'51F-44332', 1, N'Hyundai', N'Accent', N'Bạc', 1, @Now, NULL),
-- Driver: Ngô Quốc Bảo
(N'b1000001-0001-4ccc-c001-000000000007', N'a1b2c3d4-2222-4bbb-b222-222222222218', N'59P1-78899', 0, N'Honda', N'Air Blade', N'Đen Nhám', 1, @Now, NULL),
-- Driver: Trương Thị Hồng Nhung
(N'b1000001-0001-4ccc-c001-000000000008', N'a1b2c3d4-2222-4bbb-b222-222222222219', N'59V1-22334', 0, N'Yamaha', N'Janus', N'Hồng', 1, @Now, NULL),
-- Driver: Huỳnh Tấn Phát
(N'b1000001-0001-4ccc-c001-000000000009', N'a1b2c3d4-2222-4bbb-b222-222222222220', N'51H-66778', 1, N'Kia', N'Morning', N'Xanh Dương', 1, @Now, NULL),
-- Driver: Võ Minh Khôi
(N'b1000001-0001-4ccc-c001-000000000010', N'a1b2c3d4-2222-4bbb-b222-222222222221', N'59D1-99001', 0, N'Honda', N'Winner X', N'Đỏ Đen', 1, @Now, NULL),
-- Driver: Đỗ Thị Thanh Trúc
(N'b1000001-0001-4ccc-c001-000000000011', N'a1b2c3d4-2222-4bbb-b222-222222222222', N'59K2-33445', 0, N'Vespa', N'Primavera', N'Xanh Mint', 1, @Now, NULL),
-- Driver: Phan Anh Kiệt
(N'b1000001-0001-4ccc-c001-000000000012', N'a1b2c3d4-2222-4bbb-b222-222222222223', N'51E-55667', 1, N'VinFast', N'Fadil', N'Đỏ', 1, @Now, NULL),
-- Driver: Trần Thị Bích Ngọc
(N'b1000001-0001-4ccc-c001-000000000013', N'a1b2c3d4-2222-4bbb-b222-222222222224', N'59T1-77889', 0, N'Honda', N'Lead 125', N'Nâu', 1, @Now, NULL),
-- Driver: Lý Thanh Sơn
(N'b1000001-0001-4ccc-c001-000000000014', N'a1b2c3d4-2222-4bbb-b222-222222222225', N'60B-12345', 1, N'Honda', N'City', N'Đen', 1, @Now, NULL);

PRINT 'Phase 5: 14 Vehicles inserted.';

-- =====================================================
-- PHASE 6: PARKING LOTS (10 lots, 1 per Owner)
-- =====================================================
INSERT [dbo].[ParkingLots] ([ParkingLotId],[OwnerId],[Name],[Address],[Latitude],[Longitude],[TotalCapacity],[CurrentOccupancy],[PricePerHour],[Status],[RejectReason],[ImageUrl],[IsActive],[CreatedAt],[UpdatedAt],[CreatedBy],[UpdatedBy],[IsDeleted],[DeletedAt],[DeletedBy]) VALUES
-- Owner: Vũ Thanh Ngân
(N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', N'937b02b0-e8be-440e-a1b0-005aee94dcfd', N'Bãi Xe Ngân', N'Nguyễn Văn Tiên, Phường Tân Phong, TP Biên Hòa, Đồng Nai', 10.9576, 106.8426, 30, 0, 5000, N'Active', NULL, NULL, 1, @Now, NULL, N'937b02b0-e8be-440e-a1b0-005aee94dcfd', NULL, 0, NULL, NULL),
-- Owner: Trọng Hiếu
(N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', N'5a038829-3f3e-4e19-8215-7d13b915fd79', N'Bãi Xe Hiếu', N'219, Kp Ngũ Phúc, Phường Hố Nai, TP Biên Hòa, Đồng Nai', 10.9612, 106.8510, 20, 0, 4000, N'Active', NULL, NULL, 1, @Now, NULL, N'5a038829-3f3e-4e19-8215-7d13b915fd79', NULL, 0, NULL, NULL),
-- Owner: Vũ Hoàng Hiệp  
(N'02d2e82c-850c-4d1e-8538-2691204ec7fb', N'c1607535-2789-42b2-b2d1-c65ff65c154b', N'Bãi Xe Hiệp', N'Nguyễn Văn Tiên, P. Tân Phong, TP Biên Hòa, Đồng Nai', 10.9590, 106.8440, 25, 0, 6000, N'Active', NULL, NULL, 1, @Now, NULL, N'c1607535-2789-42b2-b2d1-c65ff65c154b', NULL, 0, NULL, NULL),
-- Owner: Nguyen Anh Hung
(N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', N'af0652c8-1df8-440c-83f1-f965c2885439', N'Bãi Xe Hùng', N'26/1C Đường 34, Linh Đông, Thủ Đức, TP.HCM', 10.8554, 106.7263, 40, 0, 8000, N'Active', NULL, NULL, 1, @Now, NULL, N'af0652c8-1df8-440c-83f1-f965c2885439', NULL, 0, NULL, NULL),
-- Owner: Vũ Thị Ngọc Ánh
(N'c2000001-0001-4ddd-d001-000000000005', N'a1b2c3d4-1111-4aaa-b111-111111111106', N'Bãi Xe Ánh Sáng', N'12 Lê Duẩn, Phường Bến Nghé, Quận 1, TP.HCM', 10.7769, 106.7009, 50, 0, 10000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111106', NULL, 0, NULL, NULL),
-- Owner: Trần Minh Quân
(N'c2000001-0001-4ddd-d001-000000000006', N'a1b2c3d4-1111-4aaa-b111-111111111107', N'Bãi Xe Quân Trần', N'45 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM', 10.7740, 106.7035, 35, 0, 12000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111107', NULL, 0, NULL, NULL),
-- Owner: Lê Hoàng Phúc
(N'c2000001-0001-4ddd-d001-000000000007', N'a1b2c3d4-1111-4aaa-b111-111111111108', N'Bãi Xe Phúc Lê', N'78 Phạm Văn Đồng, Hiệp Bình Chánh, Thủ Đức, TP.HCM', 10.8485, 106.7213, 20, 0, 7000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111108', NULL, 0, NULL, NULL),
-- Owner: Phạm Đức Anh
(N'c2000001-0001-4ddd-d001-000000000008', N'a1b2c3d4-1111-4aaa-b111-111111111109', N'Bãi Xe Đức Anh', N'156 Võ Văn Ngân, Bình Thọ, Thủ Đức, TP.HCM', 10.8497, 106.7717, 30, 0, 6000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111109', NULL, 0, NULL, NULL),
-- Owner: Đặng Ngọc Hân
(N'c2000001-0001-4ddd-d001-000000000009', N'a1b2c3d4-1111-4aaa-b111-111111111110', N'Bãi Xe Hân', N'23 Trần Não, An Phú, Quận 2, TP.HCM', 10.7895, 106.7380, 15, 0, 15000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111110', NULL, 0, NULL, NULL),
-- Owner: Hoàng Thị Lan Anh
(N'c2000001-0001-4ddd-d001-000000000010', N'a1b2c3d4-1111-4aaa-b111-111111111111', N'Bãi Xe Lan Anh', N'89 Nguyễn Thị Minh Khai, Phường Bến Thành, Quận 1, TP.HCM', 10.7721, 106.6923, 45, 0, 9000, N'Active', NULL, NULL, 1, @Now, NULL, N'a1b2c3d4-1111-4aaa-b111-111111111111', NULL, 0, NULL, NULL);

PRINT 'Phase 6: 10 Parking Lots inserted.';

-- =====================================================
-- PHASE 7: PARKING LOCATIONS (1 per ParkingLot)
-- =====================================================
INSERT [dbo].[ParkingLocations] ([LocationId],[ParkingLotId],[Latitude],[Longitude],[ProvinceCode],[Province],[District],[WardCode],[Ward],[Street],[Area],[FullAddress],[CreatedAt],[UpdatedAt],[CreatedBy],[UpdatedBy],[IsDeleted],[DeletedAt],[DeletedBy]) VALUES
(NEWID(), N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', 10.9576, 106.8426, 75, N'Đồng Nai', N'TP Biên Hòa', NULL, N'Tân Phong', N'Nguyễn Văn Tiên', NULL, N'Nguyễn Văn Tiên, Tân Phong, Biên Hòa, Đồng Nai', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', 10.9612, 106.8510, 75, N'Đồng Nai', N'TP Biên Hòa', NULL, N'Hố Nai', N'Kp Ngũ Phúc', NULL, N'219, Hố Nai, Biên Hòa, Đồng Nai', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'02d2e82c-850c-4d1e-8538-2691204ec7fb', 10.9590, 106.8440, 75, N'Đồng Nai', N'TP Biên Hòa', NULL, N'Tân Phong', N'Nguyễn Văn Tiên', NULL, N'Nguyễn Văn Tiên, Tân Phong, Biên Hòa, Đồng Nai', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', 10.8554, 106.7263, 79, N'TP Hồ Chí Minh', N'Thủ Đức', NULL, N'Linh Đông', N'Đường 34', NULL, N'26/1C Đường 34, Linh Đông, Thủ Đức, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000005', 10.7769, 106.7009, 79, N'TP Hồ Chí Minh', N'Quận 1', NULL, N'Bến Nghé', N'Lê Duẩn', NULL, N'12 Lê Duẩn, Bến Nghé, Quận 1, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000006', 10.7740, 106.7035, 79, N'TP Hồ Chí Minh', N'Quận 1', NULL, N'Bến Nghé', N'Nguyễn Huệ', NULL, N'45 Nguyễn Huệ, Bến Nghé, Quận 1, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000007', 10.8485, 106.7213, 79, N'TP Hồ Chí Minh', N'Thủ Đức', NULL, N'Hiệp Bình Chánh', N'Phạm Văn Đồng', NULL, N'78 PVĐ, Hiệp Bình Chánh, Thủ Đức, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000008', 10.8497, 106.7717, 79, N'TP Hồ Chí Minh', N'Thủ Đức', NULL, N'Bình Thọ', N'Võ Văn Ngân', NULL, N'156 VVN, Bình Thọ, Thủ Đức, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000009', 10.7895, 106.7380, 79, N'TP Hồ Chí Minh', N'Quận 2', NULL, N'An Phú', N'Trần Não', NULL, N'23 Trần Não, An Phú, Quận 2, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL),
(NEWID(), N'c2000001-0001-4ddd-d001-000000000010', 10.7721, 106.6923, 79, N'TP Hồ Chí Minh', N'Quận 1', NULL, N'Bến Thành', N'Nguyễn Thị Minh Khai', NULL, N'89 NTMK, Bến Thành, Quận 1, TP.HCM', @Now, NULL, NULL, NULL, 0, NULL, NULL);

PRINT 'Phase 7: 10 Parking Locations inserted.';

PRINT '=== Part 1 complete. Proceeding to Part 2 (Bookings, Payments, Reviews, Wallets)... ===';

COMMIT TRANSACTION;
PRINT 'Part 1 committed successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;
GO
