# Phân tích luồng dự án Smart Parking – API doc vs Backend vs Web vs Mobile

> Tài liệu so sánh API doc (bạn cung cấp), backend thực tế, Web admin và Mobile app; xác nhận luồng chính và điểm cần chỉnh.

---

## 1. Luồng tổng quát (theo mô tả của bạn)

- **Mobile (app người gửi xe):** Người dùng tìm bãi đỗ gần nhất (đề xuất theo vị trí) → booking bãi → tới bãi → thực hiện đỗ xe (check-in/check-out). Ngoài ra có thể đăng ký làm chủ bãi: gửi yêu cầu lên → Admin phê duyệt (Admin dùng Web).
- **Web (Admin):** Quản lý hệ thống + phê duyệt yêu cầu nâng cấp Owner và phê duyệt bãi xe.

---

## 2. So sánh API doc vs Backend thực tế

### 2.1. USER (Driver) – Auth

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| POST /api/auth/register-request | ✅ Có | |
| POST /api/auth/verify-otp | ✅ Có | |
| POST /api/auth/resend-otp | ✅ Có | |
| POST /api/auth/login | ✅ Có | |
| POST /api/auth/google | ✅ Có | Body: `{ "googleToken": "..." }` |
| POST /api/auth/refresh | ✅ Có | |
| POST /api/auth/logout | ✅ Có | [Authorize] |
| POST /api/auth/forgot-password | ✅ Có | |
| POST /api/auth/reset-password | ✅ Có | |

**Kết luận:** Khớp hoàn toàn.

---

### 2.2. USER – Parking lots & locations

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/parking-lots/nearby?lat=&lng=&radiusKm=&maxResults= | ✅ Có | |
| GET /api/parking-lots?search=&page=&pageSize= (Approved + Active) | ✅ Có | BE **không** mặc định filter Approved/Active; client cần gửi `status=Approved&isActive=true` nếu muốn chỉ bãi “public”. |
| GET /api/parking-lots/{id} | ✅ Có | |
| GET /api/locations/nearby?lat=&lng=&radius= | ✅ Có | `radius` đơn vị mét (BE: double radius = 3000) |
| GET /api/locations/search?province=&district=&ward=&search=&page=&pageSize= | ✅ Có | |
| GET /api/locations/{id} | ✅ Có | |
| GET /api/locations/parking-lot/{parkingLotId} | ✅ Có | |

**Kết luận:** Khớp. Mobile nên gọi **nearby** cho “bãi gần nhất”; list chung dùng GET /api/parking-lots với filter phù hợp.

---

### 2.3. USER – Bookings

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/bookings/my-bookings?status=&page=&pageSize= | ✅ Có | |
| GET /api/bookings/{id} | ✅ Có | |
| POST /api/bookings | ✅ Có | |
| PUT /api/bookings/{id} | ✅ Có | |
| POST /api/bookings/{id}/cancel | ✅ Có | |
| POST /api/bookings/{id}/check-in | ✅ Có | **Đường dẫn có dấu gạch:** `check-in` |
| POST /api/bookings/{id}/check-out | ✅ Có | **Đường dẫn có dấu gạch:** `check-out` |

**Kết luận:** Khớp. Mobile hiện đang gọi sai: `checkin` / `checkout` (không gạch) → cần sửa thành `check-in` / `check-out`.

---

### 2.4. USER – Vehicles

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/vehicles/my-vehicles?activeOnly=true | ✅ Có | |
| GET /api/vehicles/{id} | ✅ Có | |
| POST /api/vehicles | ✅ Có | |
| PUT /api/vehicles/{id} | ✅ Có | |
| DELETE /api/vehicles/{id} | ✅ Có | |

**Kết luận:** Khớp.

---

### 2.5. USER – Payments (booking)

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| POST /api/payments/create (VNPay booking) | ✅ Có | **Path đầy đủ:** `/api/payments/create` |
| POST /api/payments/sepay/create (SePay booking) | ✅ Có | **Path đầy đủ:** `/api/payments/sepay/create` |
| GET /api/payments/booking/{bookingId} | ✅ Có | |

**Kết luận:** Khớp. Mobile hiện gọi `POST /api/payments` và `POST /api/payments/sepay` → sai; cần sửa thành `.../create` và `.../sepay/create`.

---

### 2.6. USER – Upgrade lên OWNER

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/owners/plans | ✅ Có | |
| POST /api/owners/upgrade-request | ✅ Có | Body có thể có thêm `paymentTransactionId` (optional). |
| GET /api/owners/upgrade-request/my | ✅ Có | |

**Thanh toán phí Owner (VNPay/SePay):**

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| POST /api/payments/owner-subscription/vnpay | ✅ Có | Body: `ownerUpgradeRequestId`, `planType`, (optional) `description` |
| POST /api/payments/owner-subscription/sepay | ✅ Có | Body giống trên |

**Kết luận:** Backend đã implement đủ; không cần FE “mock” nữa. Luồng hợp lý: 1) POST /api/owners/upgrade-request (tạo request, status PendingPayment) → 2) POST owner-subscription/vnpay hoặc sepay → 3) Sau khi thanh toán thành công, backend cập nhật request → PendingApproval → 4) Admin trên Web approve/reject.

---

### 2.7. OWNER – Parking lots (host)

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/parking-lots/my | ✅ Có | |
| POST /api/parking-lots | ✅ Có | |
| PUT /api/parking-lots/{id} | ✅ Có | |
| PATCH /api/parking-lots/{id}/toggle-active | ✅ Có | |
| DELETE /api/parking-lots/{id} | ✅ Có | |
| GET /api/parking-lots/{id}/bookings?page=&pageSize= | ✅ Có | |

**Kết luận:** Khớp.

---

### 2.8. ADMIN – Dashboard & Owner upgrade

| API Doc | Backend | Ghi chú |
|--------|---------|--------|
| GET /api/admin/dashboard/users-summary | ✅ Có | |
| GET /api/admin/dashboard/parking-lots-summary | ✅ Có | |
| GET /api/admin/dashboard/revenue-today | ✅ Có | |
| GET /api/admin/dashboard/bookings-summary | ✅ Có | |
| GET /api/admin/dashboard/recent-activities?limit=10 | ✅ Có | |
| GET /api/admin/owners/upgrade-requests?status=&page=&pageSize= | ✅ Có | |
| POST /api/admin/owners/upgrade-requests/{id}/approve | ✅ Có | |
| POST /api/admin/owners/upgrade-requests/{id}/reject | ✅ Có | Body: `{ "reason": "..." }` (RejectOwnerUpgradeRequestDto) |

**Kết luận:** Khớp.

---

### 2.9. ADMIN – Users, Parking lots, Locations, Bookings, Vehicles

Các endpoint bạn liệt kê (GET/PUT/PATCH toggle, POST approve/reject, DELETE, …) đều **có trên backend** và khớp với API doc. Chi tiết đã được kiểm tra trong từng controller (AdminUsersController, AdminParkingLotsController, AdminParkingLocationsController, AdminBookingsController, AdminVehiclesController).

---

## 3. Luồng hiện tại: BE / Web / Mobile đã và đang đi tới đâu

### 3.1. Backend

- **Đã có đủ** theo API doc: Auth, Parking lots (public + owner), Locations, Bookings (user + owner approve/reject), Vehicles, Payments (booking VNPay/SePay + owner subscription VNPay/SePay), Owners (plans, upgrade-request, my), Admin (dashboard, owners, users, parking-lots, locations, bookings, vehicles, transactions, reviews, reports, settings, notifications).
- Callback/webhook: VNPay callback, SePay webhook (AllowAnonymous, validate hash/signature).

### 3.2. Web (EXE201_WEB)

- **Đã dùng:** Login (auth), Admin dashboard (users-summary, parking-lots-summary, revenue-today, bookings-summary, recent-activities, …), Users, Parking lots, Bookings, Owner requests (owner-requests), Reviews, Transactions, Reports.
- **Chưa có:** Route guard chung cho `/admin/*` (hiện check token trong từng trang). Một phần dữ liệu dashboard trong `api.js` vẫn mock → nên chuyển hết sang API thật.

### 3.3. Mobile (EXE201_MOBILE)

- **Đã dùng:** Auth (login, register OTP, verify, resend, google, refresh, logout, forgot/reset password), users profile, vehicles (list, add), bookings (list, create, getById, cancel, history, owner my-bookings, approve, reject), parkingLots (list, getById, my, create), payments (create, createSePay, getStatus, getHistory), owners (getPlans, createUpgradeRequest, getMyRequest).
- **Lỗi đường dẫn cần sửa:**
  - `bookings.checkIn` → gọi `.../check-in` (có gạch).
  - `bookings.checkOut` → gọi `.../check-out` (có gạch).
  - `payments.create` → gọi `POST /api/payments/create` (hiện đang `POST /api/payments`).
  - `payments.createSePay` → gọi `POST /api/payments/sepay/create` (hiện đang `POST /api/payments/sepay`).
- **Chưa dùng / chưa có:** 
  - **GET /api/parking-lots/nearby** – luồng “tìm bãi gần nhất” nên gọi nearby (lat, lng, radiusKm, maxResults) thay vì chỉ list chung.
  - **GET /api/locations/nearby** và **search** – nếu app có map/tìm theo địa chỉ thì nên tích hợp.
  - **Owner subscription payment** – chưa thấy Mobile gọi `owner-subscription/vnpay` hay `owner-subscription/sepay`; cần gọi sau khi tạo upgrade-request để thanh toán phí.
- **Wallet:** Vẫn mock ở client; backend không có wallet API.

---

## 4. API_TEST_FLOWS_COMPLETE.md so với thực tế

- Nội dung trong `API_TEST_FLOWS_COMPLETE.md` **theo đúng** backend hiện tại: Auth, User (vehicles, parking lots, locations, bookings, payments, owner upgrade), Owner (my parking lots, bookings), Admin. Sample request/response và status flow (Pending → Confirmed → InProgress → Completed, Owner upgrade PendingPayment → PendingApproval → Approved/Rejected) đều khớp.
- Có thể bổ sung trong doc: sau khi tạo upgrade-request, gọi POST /api/payments/owner-subscription/vnpay hoặc sepay với `ownerUpgradeRequestId` (và `planType`) để thanh toán phí; sau khi thanh toán thành công, request chuyển PendingApproval để Admin approve trên Web.

---

## 5. Tóm tắt và đề xuất

### Luồng chính (hợp lý với mô tả của bạn)

1. **Mobile – Người gửi xe:**  
   Tìm bãi gần nhất (GET **/api/parking-lots/nearby**) → xem chi tiết (GET /api/parking-lots/{id}) → đặt chỗ (POST /api/bookings) → thanh toán (POST /api/payments/create hoặc sepay/create) → tới bãi → check-in → check-out.
2. **Mobile – Đăng ký Owner:**  
   GET /api/owners/plans → POST /api/owners/upgrade-request → POST /api/payments/owner-subscription/vnpay (hoặc sepay) → sau khi thanh toán, request chuyển PendingApproval → **Web Admin** duyệt (approve/reject).
3. **Web Admin:**  
   Dashboard, quản lý users/parking-lots/bookings/transactions/reviews/reports, **phê duyệt owner upgrade** (GET upgrade-requests, POST approve/reject), phê duyệt bãi (pending → approve/reject).

### Cần chỉnh

| Nơi | Việc |
|-----|------|
| **Mobile** | Sửa `api.js`: `check-in` / `check-out`, `POST /api/payments/create`, `POST /api/payments/sepay/create`. |
| **Mobile** | Tích hợp **GET /api/parking-lots/nearby** cho màn “bãi gần nhất”. |
| **Mobile** | Sau khi tạo upgrade-request, gọi **owner-subscription/vnpay** hoặc **sepay** để thanh toán phí (nếu chưa có). |
| **Web** | Thêm ProtectedRoute cho `/admin/*`; chuyển hết dữ liệu dashboard từ mock sang API. |
| **API doc** | Có thể ghi rõ: list public bãi nên dùng `status=Approved&isActive=true`; luồng owner: upgrade-request → thanh toán phí → admin approve. |

---

## 6. Bảng nhanh: Mobile đang gọi đúng/sai

| Chức năng | Mobile hiện tại | Backend đúng | Trạng thái |
|-----------|------------------|--------------|------------|
| Check-in  | /bookings/{id}/**checkin**  | /bookings/{id}/**check-in**  | ❌ Sai |
| Check-out | /bookings/{id}/**checkout** | /bookings/{id}/**check-out** | ❌ Sai |
| Tạo thanh toán VNPay | POST /api/**payments** | POST /api/payments/**create** | ❌ Sai |
| Tạo thanh toán SePay | POST /api/payments/**sepay** | POST /api/payments/**sepay/create** | ❌ Sai |
| Tìm bãi gần nhất | Chưa dùng nearby | GET /api/parking-lots/**nearby** | ⚠️ Chưa dùng |
| Owner subscription | Chưa gọi | owner-subscription/vnpay, sepay | ⚠️ Cần tích hợp |

Sau khi sửa các đường dẫn và bổ sung nearby + owner-subscription, luồng Mobile sẽ khớp hoàn toàn với API doc và backend.
