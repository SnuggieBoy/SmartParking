# Hướng Dẫn Quy Trình Hoạt Động (API Flows) - SmartParking

Tài liệu này mô tả chi tiết 3 luồng hoạt động chính của hệ thống dựa trên các API endpoint hiện có.

## 1. Luồng Người Lái Xe (Driver Flow)
Đây là luồng dành cho người dùng cuối muốn tìm kiếm và đỗ xe.

### Bước 1: Đăng ký & Đăng nhập
- **Đăng ký (2 bước)**: 
    1. Gửi yêu cầu: Gọi `POST /api/auth/register-request` với thông tin (Email, Password, FullName, Phone) -> Nhận OTP qua email.
    2. Xác thực OTP: Gọi `POST /api/auth/verify-otp` với (Email, OtpCode) -> Hoàn tất đăng ký, nhận `accessToken`.
- **Đăng nhập**:
    - Cách 1: Gọi `POST /api/auth/login` với Email/Password -> Nhận `accessToken`.
    - Cách 2: Gọi `POST /api/auth/google` (Lưu ý: endpoint là `google`, không phải `google-login`) với Google ID Token -> Nhận `accessToken`.
- **Các tiện ích khác**:
    - Quên mật khẩu: `POST /api/auth/forgot-password` (Gửi OTP) -> `POST /api/auth/reset-password` (Mật khẩu mới + OTP).
    - Refresh Token: `POST /api/auth/refresh` để lấy token mới khi hết hạn.
- **Lưu Token**: Client cần lưu `accessToken` để sử dụng cho các bước sau (gửi trong Header `Authorization: Bearer <token>`).

### Bước 2: Quản lý phương tiện
- **Thêm xe**: Gọi `POST /api/vehicles` để thêm xe mới (Biển số, Loại xe, Màu sắc...).
- **Xem danh sách xe**: Gọi `GET /api/vehicles/my-vehicles` để lấy danh sách xe đã thêm.

### Bước 3: Tìm kiếm & Đặt chỗ
- **Tìm bãi đỗ**:
    - Gọi `GET /api/parking-lots/nearby` (gửi toạ độ `lat`, `lng`) để tìm bãi xe gần nhất.
    - Hoặc gọi `GET /api/parking-lots` để tìm theo từ khoá/bộ lọc.
- **Đặt chỗ (Booking)**:
    - Chọn bãi xe (`parkingLotId`) và xe (`vehicleId`).
    - Gọi `POST /api/bookings` với thông tin giờ vào (`startTime`) và giờ ra (`endTime`).
    - **Kết quả**: Nhận được `bookingId` và trạng thái `Pending` (Chờ thanh toán).

### Bước 4: Thanh toán
- **Tạo thanh toán**:
    - Gọi `POST /api/payments/create` (hoặc `/api/payments/sepay/create`) với `bookingId` và số tiền.
    - Nhận về URL thanh toán (VNPay) hoặc QR Code chuyển khoản (SePay).
- **Xác nhận**:
    - Sau khi thanh toán thành công, hệ thống tự động gọi Webhook/Callback.
    - Trạng thái Booking chuyển từ `Pending` -> `Confirmed`.

### Bước 5: Sử dụng dịch vụ
- **Check-in**: Khi đến bãi xe, gọi `POST /api/bookings/{id}/check-in` (Admin/Owner bãi xe thực hiện hoặc quét QR). Trạng thái chuyển sang `InProgress`.
- **Check-out**: Khi rời bãi xe, gọi `POST /api/bookings/{id}/check-out`. Trạng thái chuyển sang `Completed`.

---

## 2. Luồng Chủ Bãi Xe (Owner Flow)
Quy trình từ một người dùng thường trở thành đối tác kinh doanh bãi đỗ xe.

### Bước 1: Nâng cấp tài khoản
- **Đăng nhập**: Như người dùng thường.
- **Xem gói cước**: Gọi `GET /api/owners/plans` để xem các gói đăng ký (Tháng/Năm).
- **Gửi yêu cầu**: Gọi `POST /api/owners/upgrade-request` với thông tin bãi xe dự kiến (Tên, Địa chỉ, Gói cước). Nhận về `requestId`.

### Bước 2: Thanh toán gói đăng ký (MỚI)
- **Thanh toán**:
    - Gọi `POST /api/payments/owner-subscription/vnpay` (hoặc `sepay`) với `ownerUpgradeRequestId`.
    - Nhận link thanh toán/QR Code.
- **Xử lý**:
    - Sau khi thanh toán, trạng thái yêu cầu chuyển thành `PendingApproval` (Chờ Admin duyệt).

### Bước 3: Quản lý bãi đỗ (Sau khi được Admin duyệt)
- **Tạo bãi xe**: Gọi `POST /api/parking-lots` để tạo bãi xe chính thức trên hệ thống. Trạng thái ban đầu là `Pending` (chờ duyệt nội dung).
- **Cập nhật thông tin**: Gọi `PUT /api/parking-lots/{id}` để sửa giá, giờ mở cửa, hình ảnh.
- **Bật/Tắt hoạt động**: Gọi `PATCH /api/parking-lots/{id}/toggle-active` để mở/đóng bãi xe.

### Bước 4: Quản lý doanh thu & Booking
- **Xem Booking**: Gọi `GET /api/parking-lots/{id}/bookings` để xem ai đã đặt chỗ tại bãi của mình.
- **Thống kê**: (Tuỳ chọn) Xem dashboard doanh thu qua các API thống kê.

---

## 3. Luồng Quản Trị Viên (Admin Flow)
Quyền hạn cao nhất để vận hành hệ thống.

### Bước 1: Quản trị người dùng & Đối tác
- **Duyệt yêu cầu Owner**:
    - Gọi `GET /api/admin/owners/upgrade-requests?status=PendingApproval` để xem ai đã thanh toán và chờ duyệt.
    - Gọi `POST /api/admin/owners/upgrade-requests/{id}/approve` để chấp thuận -> User trở thành Owner.
    - Gọi `POST /api/admin/owners/upgrade-requests/{id}/reject` để từ chối.

### Bước 2: Kiểm duyệt Bãi đỗ xe
- **Duyệt bãi xe mới**:
    - Chủ xe tạo bãi -> Admin kiểm tra thông tin.
    - Gọi `PUT /api/admin/parking-lots/{id}/status` (hoặc tương tự trong `ParkingLotsController` nếu có endpoint admin) để duyệt bãi xe sang trạng thái `Approved`.
    *(Lưu ý: Trong code hiện tại, việc duyệt bãi xe có thể nằm trong luồng update status hoặc admin có quyền update trực tiếp).*

### Bước 3: Vận hành & Thống kê
- **Dashboard**: Gọi `GET /api/admin/dashboard/stats` để xem tổng quan: Số người dùng, doanh thu, số bãi xe đang hoạt động.
- **Quản lý chung**: Admin có thể thực hiện mọi hành động thay cho User/Owner (Huỷ booking, Xoá xe, Đóng bãi xe vi phạm) thông qua các API có phân quyền `UserOrAdmin` hoặc `OwnerOrAdmin`.

---
**Lưu ý kỹ thuật:**
- Mọi API (trừ Login/Register/Public Search) đều yêu cầu Header: `Authorization: Bearer <token>`.
- `bookingId`, `parkingLotId`, `vehicleId` là GUID.
- Luồng thanh toán dựa trên Webhook/Callback là tự động, không cần client gọi xác nhận thủ công.
