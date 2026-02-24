# Kế hoạch Test trên Web – Hoàn thiện luồng Smart Parking

> **Mục tiêu:** Test toàn bộ luồng trên Web trước (bỏ qua Android build), sau đó sửa lỗi nếu có.

---

## 1. Chuẩn bị – Chạy 3 thành phần

| Thành phần | Lệnh | URL |
|------------|------|-----|
| **Backend** | `dotnet run` trong `SmartParking` | `http://localhost:5070` |
| **Mobile (Web)** | `npm run web` trong `EXE201_MOBILE` | `http://localhost:8081` |
| **Web Admin** | `npm run dev` trong `EXE201_WEB` | `http://localhost:5173` |

**Lưu ý:** Mobile khi chạy `expo start --web` sẽ dùng `localhost:5070` cho API (đã cấu hình trong `api.js`).

---

## 2. Luồng test theo thứ tự

### Phase 1: Auth & Cơ bản

| # | Luồng | Cách test |
|---|-------|-----------|
| 1 | **Đăng ký User** | Mobile Web → Register → OTP → Verify |
| 2 | **Đăng nhập** | Mobile Web → Login |
| 3 | **Thêm xe** | Mobile Web → Profile/Vehicles → Thêm xe |
| 4 | **Tìm bãi gần nhất** | Mobile Web → Home → "Tìm bãi gần tôi" (cần cho phép location) |
| 5 | **Tìm bãi theo địa chỉ** | Mobile Web → Search → Nhập địa chỉ |

### Phase 2: Booking & Thanh toán

| # | Luồng | Cách test |
|---|-------|-----------|
| 6 | **Đặt chỗ** | Chọn bãi → Đặt chỗ → Chọn xe, thời gian |
| 7 | **Thanh toán VNPay** | Chọn VNPay → Redirect → (sandbox: nhập mã test) |
| 8 | **Check-in** | Booking History → Chọn booking đã thanh toán → Check-in |
| 9 | **Check-out** | Đang gửi xe → Check-out |

### Phase 3: Owner & Admin

| # | Luồng | Cách test |
|---|-------|-----------|
| 10 | **Đăng ký Owner** | Mobile Web → Đăng ký Owner → Chọn plan → Thanh toán phí (VNPay/SePay) |
| 11 | **Admin duyệt Owner** | Web Admin → `/admin/owner-requests` → Approve |
| 12 | **Owner tạo bãi** | Mobile Web (sau khi approve) → My Parking Lots → Thêm bãi |
| 13 | **Admin duyệt bãi** | Web Admin → Parking Lots → Approve bãi pending |
| 14 | **Owner xem booking** | Mobile Web → Owner → Bookings |

---

## 3. Checklist nhanh

- [ ] Backend chạy OK
- [ ] Mobile Web chạy OK (Expo web)
- [ ] Web Admin chạy OK
- [ ] Đăng ký + Login OK
- [ ] Tìm bãi (nearby + search) OK
- [ ] Đặt chỗ + Thanh toán OK
- [ ] Check-in / Check-out OK
- [ ] Đăng ký Owner + Thanh toán phí OK
- [ ] Admin approve Owner OK
- [ ] Owner tạo bãi + Admin approve bãi OK

---

## 4. Việc có thể làm sau (không chặn test)

| Việc | Mô tả |
|------|-------|
| Android build | Cấu hình `android/local.properties` khi cần build APK |
| Web ProtectedRoute | Thêm route guard chung cho `/admin/*` |
| Dashboard mock | Chuyển hết dữ liệu mock sang API thật (Admin đã dùng API) |
| Wallet | Backend chưa có wallet; hiện dùng mock client |

---

## 5. Thứ tự ưu tiên khi test

1. **Auth** → không có token thì mọi thứ fail  
2. **Tìm bãi + Đặt chỗ** → luồng chính user  
3. **Thanh toán + Check-in/out** → hoàn tất 1 vòng đỗ xe  
4. **Owner + Admin** → luồng nâng cấp và quản trị  

---

## 6. Lệnh chạy nhanh

```bash
# Terminal 1 - Backend
cd "E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking"
dotnet run

# Terminal 2 - Mobile Web
cd "E:\FPT UNIVERSITY\CN8\EXE201_MOBILE"
npm run web

# Terminal 3 - Web Admin
cd "E:\FPT UNIVERSITY\CN8\EXE201_WEB"
npm run dev
```

Sau đó mở:
- **User flow:** http://localhost:8081 (Mobile Web)
- **Admin flow:** http://localhost:5173 (Web Admin)
