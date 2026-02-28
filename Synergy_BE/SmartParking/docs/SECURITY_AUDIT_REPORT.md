# Báo cáo kiểm tra bảo mật hệ thống SmartParking

**Ngày kiểm tra:** 2025-02-23  
**Phạm vi:** Backend API, Mobile App, phân quyền theo role, cách ly dữ liệu

---

## 1. Tổng quan

Hệ thống có 3 role chính:
- **User**: Người đặt chỗ (driver), quản lý booking/vehicle/payment của mình
- **Owner**: Chủ bãi xe, quản lý bãi xe và booking tại bãi của mình
- **Admin**: Toàn quyền hệ thống

---

## 2. Backend đã kiểm tra

### 2.1 Controllers – Authorization

| Controller | Policy | Ghi chú |
|------------|--------|---------|
| BookingsController | UserOrOwnerOrAdmin | my-bookings → userId; owner/my-bookings → OwnerOrAdmin |
| ParkingLotsController | OwnerOrAdmin (my, create, update, delete) | Public: nearby, get, getById |
| VehiclesController | UserOrOwnerOrAdmin | my-vehicles → userId |
| UsersController | Authorize | profile → userId |
| WalletController | UserOrOwnerOrAdmin | balance, transactions → userId |
| PaymentController | UserOrOwnerOrAdmin | create, history → userId |
| FavoritesController | Authorize | All → userId |
| ReviewsController | Authorize | Create/Update/Delete own → userId |
| NotificationsController | Authorize | All → userId |
| OwnersController | UserOrAdmin | upgrade-request → userId |
| OwnerDashboardController | OwnerOrAdmin | dashboard, earnings → ownerId |
| OwnerBankAccountsController | OwnerOrAdmin | All → userId |
| Admin/* | AdminOnly | Toàn bộ |

### 2.2 Services – Ownership validation

| Service | Method | Kiểm tra |
|---------|--------|----------|
| BookingService | GetMyBookingsAsync | GetByUserIdAsync(userId) |
| BookingService | GetOwnerBookingsAsync | GetByOwnerIdAsync(ownerId) |
| BookingService | GetByIdAsync | Admin \|\| UserId \|\| (Owner && ParkingLot.OwnerId) |
| BookingService | GetBookingsByParkingLotAsync | Owner \|\| Admin của parking lot |
| PaymentService | GetPaymentStatusByBookingAsync | Admin \|\| UserId \|\| (Owner && ParkingLot.OwnerId) |
| PaymentService | GetPaymentHistoryAsync | GetByUserIdAsync(userId) |
| WalletService | PayWithWalletAsync | booking.UserId == userId |
| WalletService | GetBalanceAsync, GetTransactionsAsync | userId |
| ParkingLotService | GetMyParkingLotsAsync | GetByOwnerIdAsync(ownerId) |
| ParkingLotService | UpdateAsync, DeleteAsync | OwnerId \|\| Admin |
| VehicleService | GetMyVehiclesAsync | GetByUserIdAsync(userId) |
| VehicleService | GetByIdAsync, UpdateAsync | UserId \|\| Admin |
| ReviewService | CreateReviewAsync | userId + completed booking |
| ReviewService | UpdateReviewAsync, DeleteReviewAsync | Review.UserId == userId |

### 2.3 Repositories – Query filters

| Repository | Method | Filter |
|------------|--------|--------|
| BookingRepository | GetByUserIdAsync | b.UserId == userId |
| BookingRepository | GetByOwnerIdAsync | b.ParkingLot.OwnerId == ownerId |
| PaymentRepository | GetByUserIdAsync | p.UserId == userId |
| ParkingLotRepository | GetByOwnerIdAsync | p.OwnerId == ownerId |
| UserWalletRepository | GetByUserIdAsync | userId |

---

## 3. Lỗ hổng đã sửa

### 3.1 PaymentService.GetPaymentStatusByBookingAsync
- **Trước:** Chỉ cho Admin hoặc booking.UserId == userId
- **Sau:** Thêm Owner: booking.ParkingLot.OwnerId == userId
- **Lý do:** Owner cần xem trạng thái thanh toán của booking tại bãi của mình

### 3.2 OwnerBookingsScreen (Mobile)
- **Trước:** Fallback sang my-bookings khi owner trả rỗng → lẫn lộn User/Owner
- **Sau:** Chỉ dùng owner/my-bookings, không fallback

### 3.3 OwnerBookingsScreen – Nút Duyệt/Từ chối
- **Trước:** Hiển thị cho mọi booking Pending
- **Sau:** Chỉ hiển thị khi item.userId !== user?.userId (booking do người khác tạo)

### 3.4 ParkingLotBookingDto
- **Thêm:** Trường UserId để mobile kiểm tra ownership

---

## 4. Các điểm đã xác minh an toàn

1. **Wallet**: GetBalance, TopUp, GetTransactions, PayBooking luôn dùng userId từ token
2. **Payment history**: GetByUserIdAsync(userId) – chỉ lịch sử của user
3. **Admin**: Tất cả Admin controllers dùng [Authorize(Policy = AdminOnly)]
4. **VNPay/SePay callback**: AllowAnonymous nhưng validate hash/signature
5. **User profile**: Chỉ get/update profile của chính mình

---

## 5. Khuyến nghị

1. **Logging**: Ghi log các truy cập nhạy cảm (payment, wallet, admin)
2. **Rate limiting**: Giới hạn API cho payment, wallet
3. **Audit trail**: Lưu lịch sử thay đổi cho booking, payment
4. **Token expiry**: Đảm bảo JWT có thời hạn hợp lý
