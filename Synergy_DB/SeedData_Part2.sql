-- =====================================================
-- SYNERGY SMART PARKING - COMPREHENSIVE SEED DATA (PART 2)
-- Generated: 2026-03-23
-- Run this AFTER SeedData.sql
-- =====================================================

BEGIN TRANSACTION;
BEGIN TRY

DECLARE @Now DATETIME2 = GETUTCDATE();
DECLARE @D1 DATETIME2 = DATEADD(day, -10, @Now);
DECLARE @D2 DATETIME2 = DATEADD(day, -8, @Now);
DECLARE @D3 DATETIME2 = DATEADD(day, -5, @Now);
DECLARE @D4 DATETIME2 = DATEADD(day, -2, @Now);
DECLARE @D5 DATETIME2 = DATEADD(day, -1, @Now);

-- =====================================================
-- PHASE 8: USER WALLETS (1 for each user)
-- =====================================================
INSERT [dbo].[UserWallets] ([UserId],[Balance],[UpdatedAt]) VALUES
(N'9e5e38a3-b7a1-4252-846c-27c0ebdc7b59', 0, @Now),
(N'937b02b0-e8be-440e-a1b0-005aee94dcfd', 1500000, @Now),
(N'5a038829-3f3e-4e19-8215-7d13b915fd79', 1200000, @Now),
(N'c1607535-2789-42b2-b2d1-c65ff65c154b', 850000, @Now),
(N'af0652c8-1df8-440c-83f1-f965c2885439', 2400000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111106', 500000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111107', 900000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111108', 350000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111109', 1100000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111110', 450000, @Now),
(N'a1b2c3d4-1111-4aaa-b111-111111111111', 800000, @Now),
(N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', 500000, @Now),
(N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', 1200000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222214', 850000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222215', 2500000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222216', 300000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222217', 1500000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222218', 600000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222219', 4500000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222220', 2100000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222221', 950000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222222', 1700000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222223', 3200000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222224', 400000, @Now),
(N'a1b2c3d4-2222-4bbb-b222-222222222225', 1800000, @Now);
PRINT 'Phase 8: 25 User Wallets inserted.';


-- =====================================================
-- PHASE 9: TOP-UP TRANSACTIONS (10 Giao dịch nạp tiền)
-- =====================================================
DECLARE @T1 UNIQUEIDENTIFIER = NEWID(), @T2 UNIQUEIDENTIFIER = NEWID(), @T3 UNIQUEIDENTIFIER = NEWID(), @T4 UNIQUEIDENTIFIER = NEWID(), @T5 UNIQUEIDENTIFIER = NEWID();
DECLARE @T6 UNIQUEIDENTIFIER = NEWID(), @T7 UNIQUEIDENTIFIER = NEWID(), @T8 UNIQUEIDENTIFIER = NEWID(), @T9 UNIQUEIDENTIFIER = NEWID(), @T10 UNIQUEIDENTIFIER = NEWID();

INSERT [dbo].[PaymentTransactions] ([PaymentId], [BookingId], [UserId], [Amount], [PaymentMethod], [PaymentStatus], [PaymentType], [VnpTxnRef], [CreatedAt], [IsDeleted]) VALUES
(@T1, NULL, N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', 500000, N'SePay', N'Success', N'TopUp', N'TOP1001', @D1, 0),
(@T2, NULL, N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', 1000000, N'SePay', N'Success', N'TopUp', N'TOP1002', @D1, 0),
(@T3, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222214', 800000, N'SePay', N'Success', N'TopUp', N'TOP1003', @D2, 0),
(@T4, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222215', 2000000, N'SePay', N'Success', N'TopUp', N'TOP1004', @D2, 0),
(@T5, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222216', 500000, N'SePay', N'Success', N'TopUp', N'TOP1005', @D3, 0),
(@T6, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222217', 1500000, N'SePay', N'Success', N'TopUp', N'TOP1006', @D3, 0),
(@T7, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222218', 600000, N'SePay', N'Success', N'TopUp', N'TOP1007', @D4, 0),
(@T8, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222219', 4000000, N'SePay', N'Success', N'TopUp', N'TOP1008', @D4, 0),
(@T9, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222220', 2000000, N'SePay', N'Success', N'TopUp', N'TOP1009', @D5, 0),
(@T10, NULL, N'a1b2c3d4-2222-4bbb-b222-222222222221', 1000000, N'SePay', N'Success', N'TopUp', N'TOP1010', @D5, 0);

INSERT [dbo].[WalletTransactions] ([WalletTransactionId], [UserId], [Amount], [Type], [BalanceAfter], [BookingId], [PaymentTransactionId], [Description], [CreatedAt]) VALUES
(NEWID(), N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', 500000, N'TopUp', 500000, NULL, @T1, N'Nạp tiền vào ví', @D1),
(NEWID(), N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', 1000000, N'TopUp', 1000000, NULL, @T2, N'Nạp tiền vào ví', @D1),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222214', 800000, N'TopUp', 800000, NULL, @T3, N'Nạp tiền vào ví', @D2),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222215', 2000000, N'TopUp', 2000000, NULL, @T4, N'Nạp tiền vào ví', @D2),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222216', 500000, N'TopUp', 500000, NULL, @T5, N'Nạp tiền vào ví', @D3),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222217', 1500000, N'TopUp', 1500000, NULL, @T6, N'Nạp tiền vào ví', @D3),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222218', 600000, N'TopUp', 600000, NULL, @T7, N'Nạp tiền vào ví', @D4),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222219', 4000000, N'TopUp', 4000000, NULL, @T8, N'Nạp tiền vào ví', @D4),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222220', 2000000, N'TopUp', 2000000, NULL, @T9, N'Nạp tiền vào ví', @D5),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222221', 1000000, N'TopUp', 1000000, NULL, @T10, N'Nạp tiền vào ví', @D5);


-- =====================================================
-- PHASE 10: BOOKINGS (15 Giao dịch Booking - TẤT CẢ LÀ COMPLETED)
-- =====================================================
DECLARE @B1 UNIQUEIDENTIFIER=NEWID(), @B2 UNIQUEIDENTIFIER=NEWID(), @B3 UNIQUEIDENTIFIER=NEWID(), @B4 UNIQUEIDENTIFIER=NEWID(), @B5 UNIQUEIDENTIFIER=NEWID();
DECLARE @B6 UNIQUEIDENTIFIER=NEWID(), @B7 UNIQUEIDENTIFIER=NEWID(), @B8 UNIQUEIDENTIFIER=NEWID(), @B9 UNIQUEIDENTIFIER=NEWID(), @B10 UNIQUEIDENTIFIER=NEWID();
DECLARE @B11 UNIQUEIDENTIFIER=NEWID(), @B12 UNIQUEIDENTIFIER=NEWID(), @B13 UNIQUEIDENTIFIER=NEWID(), @B14 UNIQUEIDENTIFIER=NEWID(), @B15 UNIQUEIDENTIFIER=NEWID();

-- Thay đổi: Tất cả Status = 'Completed'. Tất cả CheckInTime và CheckOutTime đều hợp lệ.
INSERT [dbo].[Bookings] ([BookingId], [UserId], [ParkingLotId], [VehicleId], [BookingTime], [StartTime], [EndTime], [Status], [TotalAmount], [CheckInTime], [CheckOutTime], [CreatedAt], [UpdatedAt], [IsDeleted]) VALUES
(@B1, N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', N'b1000001-0001-4ccc-c001-000000000001', @D1, DATEADD(hour, 1, @D1), DATEADD(hour, 4, @D1), N'Completed', 24000, DATEADD(hour, 1, @D1), DATEADD(hour, 4, @D1), @D1, @D1, 0),
(@B2, N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', N'b1000001-0001-4ccc-c001-000000000002', @D2, DATEADD(hour, 2, @D2), DATEADD(hour, 5, @D2), N'Completed', 15000, DATEADD(hour, 2, @D2), DATEADD(hour, 5, @D2), @D2, @D2, 0),
(@B3, N'a1b2c3d4-2222-4bbb-b222-222222222214', N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', N'b1000001-0001-4ccc-c001-000000000003', @D3, DATEADD(hour, 1, @D3), DATEADD(hour, 3, @D3), N'Completed', 8000,  DATEADD(hour, 1, @D3), DATEADD(hour, 3, @D3), @D3, @D3, 0),
(@B4, N'a1b2c3d4-2222-4bbb-b222-222222222215', N'02d2e82c-850c-4d1e-8538-2691204ec7fb', N'b1000001-0001-4ccc-c001-000000000004', @D4, DATEADD(hour, 1, @D4), DATEADD(hour, 6, @D4), N'Completed', 30000, DATEADD(hour, 1, @D4), DATEADD(hour, 6, @D4), @D4, @D4, 0),
(@B5, N'a1b2c3d4-2222-4bbb-b222-222222222216', N'c2000001-0001-4ddd-d001-000000000005', N'b1000001-0001-4ccc-c001-000000000005', @D5, DATEADD(hour, 1, @D5), DATEADD(hour, 4, @D5), N'Completed', 30000, DATEADD(hour, 1, @D5), DATEADD(hour, 4, @D5), @D5, @D5, 0),
(@B6, N'a1b2c3d4-2222-4bbb-b222-222222222217', N'c2000001-0001-4ddd-d001-000000000006', N'b1000001-0001-4ccc-c001-000000000006', @D1, DATEADD(hour, 1, @D1), DATEADD(hour, 3, @D1), N'Completed', 36000, DATEADD(hour, 1, @D1), DATEADD(hour, 3, @D1), @D1, @D1, 0),
(@B7, N'a1b2c3d4-2222-4bbb-b222-222222222218', N'c2000001-0001-4ddd-d001-000000000007', N'b1000001-0001-4ccc-c001-000000000007', @D2, DATEADD(hour, 2, @D2), DATEADD(hour, 4, @D2), N'Completed', 21000, DATEADD(hour, 2, @D2), DATEADD(hour, 4, @D2), @D2, @D2, 0),
(@B8, N'a1b2c3d4-2222-4bbb-b222-222222222219', N'c2000001-0001-4ddd-d001-000000000008', N'b1000001-0001-4ccc-c001-000000000008', @D3, DATEADD(hour, 2, @D3), DATEADD(hour, 5, @D3), N'Completed', 30000, DATEADD(hour, 2, @D3), DATEADD(hour, 5, @D3), @D3, @D3, 0),
(@B9, N'a1b2c3d4-2222-4bbb-b222-222222222220', N'c2000001-0001-4ddd-d001-000000000009', N'b1000001-0001-4ccc-c001-000000000009', @D4, DATEADD(hour, 1, @D4), DATEADD(hour, 6, @D4), N'Completed', 67500, DATEADD(hour, 1, @D4), DATEADD(hour, 6, @D4), @D4, @D4, 0),
(@B10, N'a1b2c3d4-2222-4bbb-b222-222222222221', N'c2000001-0001-4ddd-d001-000000000010', N'b1000001-0001-4ccc-c001-000000000010', @D5, DATEADD(hour, 2, @D5), DATEADD(hour, 8, @D5), N'Completed', 54000, DATEADD(hour, 2, @D5), DATEADD(hour, 8, @D5), @D5, @D5, 0),
(@B11, N'a1b2c3d4-2222-4bbb-b222-222222222222', N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', N'b1000001-0001-4ccc-c001-000000000011', @D1, DATEADD(hour, 2, @D1), DATEADD(hour, 5, @D1), N'Completed', 15000, DATEADD(hour, 2, @D1), DATEADD(hour, 5, @D1), @D1, @D1, 0),
(@B12, N'a1b2c3d4-2222-4bbb-b222-222222222223', N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', N'b1000001-0001-4ccc-c001-000000000012', @D2, DATEADD(hour, 1, @D2), DATEADD(hour, 4, @D2), N'Completed', 12000, DATEADD(hour, 1, @D2), DATEADD(hour, 4, @D2), @D2, @D2, 0),
(@B13, N'a1b2c3d4-2222-4bbb-b222-222222222224', N'02d2e82c-850c-4d1e-8538-2691204ec7fb', N'b1000001-0001-4ccc-c001-000000000013', @D3, DATEADD(hour, 3, @D3), DATEADD(hour, 6, @D3), N'Completed', 18000, DATEADD(hour, 3, @D3), DATEADD(hour, 6, @D3), @D3, @D3, 0),
(@B14, N'a1b2c3d4-2222-4bbb-b222-222222222225', N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', N'b1000001-0001-4ccc-c001-000000000014', @D4, DATEADD(hour, 1, @D4), DATEADD(hour, 2, @D4), N'Completed', 8000,  DATEADD(hour, 1, @D4), DATEADD(hour, 2, @D4), @D4, @D4, 0),
(@B15, N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'c2000001-0001-4ddd-d001-000000000005', N'b1000001-0001-4ccc-c001-000000000001', @D5, DATEADD(hour, 1, @D5), DATEADD(hour, 3, @D5), N'Completed', 20000, DATEADD(hour, 1, @D5), DATEADD(hour, 3, @D5), @D5, @D5, 0);


-- =====================================================
-- PHASE 11: PAYMENT & WALLET TRANSACTIONS FOR BOOKINGS (Tất cả Completed)
-- =====================================================
DECLARE @Tx1 UNIQUEIDENTIFIER=NEWID(), @Tx2 UNIQUEIDENTIFIER=NEWID(), @Tx3 UNIQUEIDENTIFIER=NEWID(), @Tx4 UNIQUEIDENTIFIER=NEWID(), @Tx5 UNIQUEIDENTIFIER=NEWID();
DECLARE @Tx6 UNIQUEIDENTIFIER=NEWID(), @Tx7 UNIQUEIDENTIFIER=NEWID(), @Tx8 UNIQUEIDENTIFIER=NEWID(), @Tx9 UNIQUEIDENTIFIER=NEWID(), @Tx10 UNIQUEIDENTIFIER=NEWID();
DECLARE @Tx11 UNIQUEIDENTIFIER=NEWID(), @Tx12 UNIQUEIDENTIFIER=NEWID(), @Tx13 UNIQUEIDENTIFIER=NEWID(), @Tx14 UNIQUEIDENTIFIER=NEWID(), @Tx15 UNIQUEIDENTIFIER=NEWID();

INSERT [dbo].[PaymentTransactions] ([PaymentId], [BookingId], [UserId], [Amount], [PaymentMethod], [PaymentStatus], [PaymentType], [VnpTxnRef], [CreatedAt], [IsDeleted]) VALUES
(@Tx1, @B1, N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', 24000, N'Wallet', N'Success', N'Booking', N'WALB001', @D1, 0),
(@Tx2, @B2, N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', 15000, N'Wallet', N'Success', N'Booking', N'WALB002', @D2, 0),
(@Tx3, @B3, N'a1b2c3d4-2222-4bbb-b222-222222222214', 8000,  N'SePay',  N'Success', N'Booking', N'SEPB003', @D3, 0),
(@Tx4, @B4, N'a1b2c3d4-2222-4bbb-b222-222222222215', 30000, N'SePay',  N'Success', N'Booking', N'SEPB004', @D4, 0),
(@Tx5, @B5, N'a1b2c3d4-2222-4bbb-b222-222222222216', 30000, N'Wallet', N'Success', N'Booking', N'WALB005', @D5, 0),
(@Tx6, @B6, N'a1b2c3d4-2222-4bbb-b222-222222222217', 36000, N'Wallet', N'Success', N'Booking', N'WALB006', @D1, 0),
(@Tx7, @B7, N'a1b2c3d4-2222-4bbb-b222-222222222218', 21000, N'SePay',  N'Success', N'Booking', N'SEPB007', @D2, 0),
(@Tx8, @B8, N'a1b2c3d4-2222-4bbb-b222-222222222219', 30000, N'SePay',  N'Success', N'Booking', N'SEPB008', @D3, 0),
(@Tx9, @B9, N'a1b2c3d4-2222-4bbb-b222-222222222220', 67500, N'Wallet', N'Success', N'Booking', N'WALB009', @D4, 0),
(@Tx10, @B10, N'a1b2c3d4-2222-4bbb-b222-222222222221', 54000, N'Wallet', N'Success', N'Booking', N'WALB010', @D5, 0),
(@Tx11, @B11, N'a1b2c3d4-2222-4bbb-b222-222222222222', 15000, N'SePay',  N'Success', N'Booking', N'SEPB011', @D1, 0),
(@Tx12, @B12, N'a1b2c3d4-2222-4bbb-b222-222222222223', 12000, N'SePay',  N'Success', N'Booking', N'SEPB012', @D2, 0),
(@Tx13, @B13, N'a1b2c3d4-2222-4bbb-b222-222222222224', 18000, N'Wallet', N'Success', N'Booking', N'WALB013', @D3, 0),
(@Tx14, @B14, N'a1b2c3d4-2222-4bbb-b222-222222222225', 8000,  N'SePay',  N'Success', N'Booking', N'SEPB014', @D4, 0),
(@Tx15, @B15, N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', 20000, N'Wallet', N'Success', N'Booking', N'WALB015', @D5, 0);

-- Các giao dịch được thanh toán bằng VÍ (Wallet) sẽ trừ vào ví
INSERT [dbo].[WalletTransactions] ([WalletTransactionId], [UserId], [Amount], [Type], [BalanceAfter], [BookingId], [PaymentTransactionId], [Description], [CreatedAt]) VALUES
(NEWID(), N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', -24000, N'BookingPayment', 476000, @B1, @Tx1, N'Thanh toán giữ xe', @D1),
(NEWID(), N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', -15000, N'BookingPayment', 1185000, @B2, @Tx2, N'Thanh toán giữ xe', @D2),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222216', -30000, N'BookingPayment', 270000, @B5, @Tx5, N'Thanh toán giữ xe', @D5),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222217', -36000, N'BookingPayment', 1464000, @B6, @Tx6, N'Thanh toán giữ xe', @D1),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222220', -67500, N'BookingPayment', 2032500, @B9, @Tx9, N'Thanh toán giữ xe', @D4),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222221', -54000, N'BookingPayment', 896000, @B10, @Tx10, N'Thanh toán giữ xe', @D5),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222224', -18000, N'BookingPayment', 382000, @B13, @Tx13, N'Thanh toán giữ xe', @D3),
(NEWID(), N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', -20000, N'BookingPayment', 456000, @B15, @Tx15, N'Thanh toán giữ xe', @D5);


-- =====================================================
-- PHASE 12: REVIEWS (15 Đánh giá cho 15 bookings completed)
-- =====================================================
INSERT [dbo].[Reviews] ([ReviewId], [UserId], [ParkingLotId], [BookingId], [Rating], [Comment], [CreatedAt], [IsDeleted]) VALUES
(NEWID(), N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', @B1, 5, N'Bãi xe rộng rãi, nhân viên nhiệt tình!', @D1, 0),
(NEWID(), N'77bfd936-ad46-4f56-aee1-fa2cf6ac906e', N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', @B2, 4, N'Giá cả hợp lý, dễ tìm.', @D2, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222214', N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', @B3, 5, N'An toàn tuyệt đối, rất yên tâm.', @D3, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222215', N'02d2e82c-850c-4d1e-8538-2691204ec7fb', @B4, 4, N'Hơi đông nhưng bảo vệ vui vẻ.', @D4, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222216', N'c2000001-0001-4ddd-d001-000000000005', @B5, 5, N'Tuyệt vời ông mặt trời!!', @D5, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222217', N'c2000001-0001-4ddd-d001-000000000006', @B6, 5, N'Bãi đậu xe sạch sẽ.', @D1, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222218', N'c2000001-0001-4ddd-d001-000000000007', @B7, 4, N'Gần trung tâm mua sắm, siêu tiện lợi.', @D2, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222219', N'c2000001-0001-4ddd-d001-000000000008', @B8, 5, N'Vote 5 sao nha.', @D3, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222220', N'c2000001-0001-4ddd-d001-000000000009', @B9, 4, N'Bãi đậu ok, thẻ giữ xe hiện đại.', @D4, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222221', N'c2000001-0001-4ddd-d001-000000000010', @B10, 5, N'Thích nhất hệ thống check-in nhanh.', @D5, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222222', N'e20faf9c-c6b9-41fe-8e41-6371b1e1cabb', @B11, 4, N'Tốt.', @D1, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222223', N'20c51ead-40d9-427c-9e04-9a3aeb2c91a5', @B12, 5, N'Luôn ưu tiên gửi xe ở đây.', @D2, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222224', N'02d2e82c-850c-4d1e-8538-2691204ec7fb', @B13, 5, N'Mình để xe cả ngày vẫn rất rẻ.', @D3, 0),
(NEWID(), N'a1b2c3d4-2222-4bbb-b222-222222222225', N'507b4e34-ab50-4d15-bba8-2a6e4ac2a8b4', @B14, 4, N'Sẽ quay lại lần tới.', @D4, 0),
(NEWID(), N'6ac6e7b1-711c-4fc6-b195-295f2bea5b41', N'c2000001-0001-4ddd-d001-000000000005', @B15, 5, N'Rất xuất sắc.', @D5, 0);

PRINT 'Phase 12: 15 Reviews inserted.';

PRINT '=== Part 2 complete. TOTAL OVER 30 TRANSACTIONS GENERATED (10 Top-Ups + 15 Completed Bookings) ===';

COMMIT TRANSACTION;
PRINT 'Part 2 committed successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
END CATCH;
GO
