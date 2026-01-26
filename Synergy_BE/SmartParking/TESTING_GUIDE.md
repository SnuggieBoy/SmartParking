# 🧪 HƯỚNG DẪN TEST TOÀN BỘ LUỒNG HỆ THỐNG

**Date:** 2026-01-26  
**Application URL:** `https://localhost:7278/swagger`

---

## 📋 **TỔNG QUAN LUỒNG TEST**

```
1. Authentication (Đăng ký/Đăng nhập)
   ↓
2. Tạo Parking Lot (Owner/Admin)
   ↓
3. Tạo Location cho Parking Lot (Admin)
   ↓
4. Tạo Booking (User)
   ↓
5. Check-in Booking (User)
   ↓
6. Check-out Booking (User)
   ↓
7. Tạo Payment (User)
   ↓
8. Test Location Search (Public)
```

---

## 🚀 **BƯỚC 1: AUTHENTICATION**

### **1.1. Đăng ký User mới**

**Endpoint:** `POST /api/auth/register-request`

**Request Body:**
```json
{
  "email": "user1@test.com",
  "password": "Test123!@#"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "OTP sent to email",
  "data": null
}
```

**Action:** 
- Kiểm tra email để lấy OTP code (6 chữ số)
- Lưu lại email và OTP

---

### **1.2. Verify OTP để hoàn tất đăng ký**

**Endpoint:** `POST /api/auth/verify-otp`

**Request Body:**
```json
{
  "email": "user1@test.com",
  "otpCode": "123456"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "expiresIn": 3600
  }
}
```

**Action:**
- Lưu `accessToken` và `refreshToken`
- Token này dùng cho các request tiếp theo

---

### **1.3. Đăng nhập (nếu đã có tài khoản)**

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "user1@test.com",
  "password": "Test123!@#"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "eyJhbGc...",
    "expiresIn": 3600
  }
}
```

**Action:**
- Lưu token mới

---

### **1.4. Authorize trong Swagger**

1. Click nút **"Authorize"** ở góc trên bên phải Swagger
2. Nhập: `Bearer {accessToken}` (thay `{accessToken}` bằng token thực tế)
3. Click **"Authorize"**
4. Click **"Close"**

**✅ Bây giờ bạn có thể test các endpoints yêu cầu authentication**

---

## 🏢 **BƯỚC 2: TẠO PARKING LOT (Owner/Admin)**

### **2.1. Tạo Parking Lot**

**Endpoint:** `POST /api/parking-lots`

**Headers:**
- `Authorization: Bearer {accessToken}` (đã authorize ở bước 1.4)

**Request Body:**
```json
{
  "name": "Bãi đỗ xe Trung tâm",
  "address": "123 Nguyễn Huệ, Quận 1, TP.HCM",
  "totalCapacity": 50,
  "pricePerHour": 10000
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Parking lot created successfully",
  "data": {
    "parkingLotId": "guid-here",
    "name": "Bãi đỗ xe Trung tâm",
    "address": "123 Nguyễn Huệ, Quận 1, TP.HCM",
    "totalCapacity": 50,
    "currentOccupancy": 0,
    "pricePerHour": 10000,
    "status": "Active",
    "isActive": true,
    "ownerName": "User Full Name",
    "createdAt": "2026-01-26T18:00:00Z"
  }
}
```

**Action:**
- Lưu `parkingLotId` để dùng cho các bước tiếp theo

---

### **2.2. Xem danh sách Parking Lots**

**Endpoint:** `GET /api/parking-lots?page=1&pageSize=10`

**Expected Response:**
```json
{
  "success": true,
  "message": "Parking lots retrieved successfully",
  "data": {
    "items": [...],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### **2.3. Xem Parking Lot của tôi**

**Endpoint:** `GET /api/parking-lots/my`

**Expected Response:**
- Trả về danh sách parking lots mà user hiện tại là owner

---

## 📍 **BƯỚC 3: TẠO LOCATION (Admin)**

**⚠️ Lưu ý:** Endpoint này yêu cầu role **Admin**. Nếu bạn là User/Owner, cần đăng nhập bằng tài khoản Admin.

### **3.1. Tạo Location cho Parking Lot**

**Endpoint:** `POST /api/locations`

**Headers:**
- `Authorization: Bearer {adminAccessToken}`

**Request Body:**
```json
{
  "parkingLotId": "{parkingLotId từ bước 2.1}",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "province": "Ho Chi Minh",
  "district": "District 1",
  "ward": "Ben Nghe",
  "street": "123 Nguyen Hue",
  "fullAddress": "123 Nguyen Hue, Ben Nghe, District 1, Ho Chi Minh"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Location created successfully",
  "data": {
    "locationId": "guid-here",
    "parkingLotId": "guid-here",
    "parkingLotName": "Bãi đỗ xe Trung tâm",
    "latitude": 10.762622,
    "longitude": 106.660172,
    "province": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ben Nghe",
    "fullAddress": "123 Nguyen Hue, Ben Nghe, District 1, Ho Chi Minh"
  }
}
```

**Action:**
- Lưu `locationId` và coordinates để test search

---

### **3.2. Test Nearby Search (Public - không cần auth)**

**Endpoint:** `GET /api/locations/nearby?lat=10.762622&lng=106.660172&radius=3000`

**Expected Response:**
```json
{
  "success": true,
  "message": "Nearby locations retrieved successfully",
  "data": [
    {
      "locationId": "guid-here",
      "parkingLotId": "guid-here",
      "parkingLotName": "Bãi đỗ xe Trung tâm",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "distanceInMeters": 0.0,
      "fullAddress": "..."
    }
  ]
}
```

**✅ Test thành công nếu:**
- Trả về location vừa tạo
- `distanceInMeters` = 0 (vì search từ chính vị trí đó)

---

### **3.3. Test Address Search (Public)**

**Endpoint:** `GET /api/locations/search?province=Ho Chi Minh&district=District 1&page=1&pageSize=10`

**Expected Response:**
- Trả về locations theo province/district

---

## 📅 **BƯỚC 4: TẠO BOOKING (User)**

### **4.1. Tạo Booking**

**Endpoint:** `POST /api/bookings`

**Headers:**
- `Authorization: Bearer {userAccessToken}`

**Request Body:**
```json
{
  "parkingLotId": "{parkingLotId từ bước 2.1}",
  "startTime": "2026-01-27T10:00:00Z",
  "endTime": "2026-01-27T12:00:00Z"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Booking created successfully",
  "data": {
    "bookingId": "guid-here",
    "parkingLotId": "guid-here",
    "parkingLotName": "Bãi đỗ xe Trung tâm",
    "startTime": "2026-01-27T10:00:00Z",
    "endTime": "2026-01-27T12:00:00Z",
    "status": "Pending",
    "totalAmount": 0,
    "createdAt": "2026-01-26T18:00:00Z"
  }
}
```

**Action:**
- Lưu `bookingId` để dùng cho các bước tiếp theo

---

### **4.2. Xem danh sách Bookings của tôi**

**Endpoint:** `GET /api/bookings/my-bookings?page=1&pageSize=10`

**Query Parameters:**
- `status` (optional): "Pending", "Confirmed", "InProgress", "Completed", "Cancelled"
- `page`: 1
- `pageSize`: 10

**Expected Response:**
```json
{
  "success": true,
  "message": "Bookings retrieved successfully",
  "data": {
    "items": [...],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

---

### **4.3. Xem chi tiết Booking**

**Endpoint:** `GET /api/bookings/{bookingId}`

**Expected Response:**
- Trả về full details của booking

---

## ✅ **BƯỚC 5: CHECK-IN BOOKING**

### **5.1. Check-in**

**Endpoint:** `POST /api/bookings/{bookingId}/check-in`

**Headers:**
- `Authorization: Bearer {userAccessToken}`

**Request Body:** (empty)

**Expected Response:**
```json
{
  "success": true,
  "message": "Check-in successful",
  "data": {
    "bookingId": "guid-here",
    "status": "InProgress",
    "checkInTime": "2026-01-27T10:00:00Z",
    "currentOccupancy": 1
  }
}
```

**✅ Test thành công nếu:**
- Status chuyển từ "Pending" → "InProgress"
- `checkInTime` được set
- `currentOccupancy` của parking lot tăng lên 1

---

## 🚪 **BƯỚC 6: CHECK-OUT BOOKING**

### **6.1. Check-out**

**Endpoint:** `POST /api/bookings/{bookingId}/check-out`

**Headers:**
- `Authorization: Bearer {userAccessToken}`

**Request Body:** (empty)

**Expected Response:**
```json
{
  "success": true,
  "message": "Check-out successful",
  "data": {
    "bookingId": "guid-here",
    "status": "Completed",
    "checkInTime": "2026-01-27T10:00:00Z",
    "checkOutTime": "2026-01-27T12:00:00Z",
    "totalAmount": 20000,
    "currentOccupancy": 0
  }
}
```

**✅ Test thành công nếu:**
- Status chuyển từ "InProgress" → "Completed"
- `checkOutTime` được set
- `totalAmount` được tính đúng (2 giờ × 10000 = 20000)
- `currentOccupancy` giảm về 0

---

## 💳 **BƯỚC 7: TẠO PAYMENT**

### **7.1. Tạo Payment URL**

**Endpoint:** `POST /api/payments/create`

**Headers:**
- `Authorization: Bearer {userAccessToken}`

**Request Body:**
```json
{
  "bookingId": "{bookingId từ bước 4.1}",
  "amount": 20000,
  "description": "Parking fee for booking"
}
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Payment URL created successfully",
  "data": {
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...",
    "txnRef": "PAY202601261800001234"
  }
}
```

**Action:**
- Lưu `txnRef` để query payment status
- `paymentUrl` là URL để redirect user đến VNPay

---

### **7.2. Query Payment Status**

**Endpoint:** `GET /api/payments/booking/{bookingId}`

**Headers:**
- `Authorization: Bearer {userAccessToken}`

**Expected Response:**
```json
{
  "success": true,
  "message": "Payment status retrieved successfully",
  "data": {
    "bookingId": "guid-here",
    "amount": 20000,
    "paymentStatus": "Pending",
    "paymentMethod": "VNPay",
    "paidAt": null
  }
}
```

**✅ Test thành công nếu:**
- Trả về payment status
- Nếu payment thành công, `paymentStatus` = "Success" và `paidAt` có giá trị

---

## 🔍 **BƯỚC 8: TEST LOCATION SEARCH (Public)**

### **8.1. Nearby Search với tọa độ khác**

**Endpoint:** `GET /api/locations/nearby?lat=10.77&lng=106.67&radius=5000`

**Expected Response:**
- Trả về locations trong bán kính 5000m
- Sắp xếp theo distance (gần nhất trước)

---

### **8.2. Address Search**

**Endpoint:** `GET /api/locations/search?province=Ho Chi Minh&search=Nguyen Hue&page=1&pageSize=10`

**Expected Response:**
- Trả về locations match với search term

---

## ✅ **CHECKLIST TEST**

Sau khi test, verify:

- [ ] Đăng ký/Đăng nhập thành công
- [ ] Tạo Parking Lot thành công
- [ ] Tạo Location thành công
- [ ] Nearby search trả về đúng location
- [ ] Tạo Booking thành công
- [ ] Check-in thành công (status → InProgress, occupancy tăng)
- [ ] Check-out thành công (status → Completed, amount tính đúng, occupancy giảm)
- [ ] Tạo Payment URL thành công
- [ ] Query Payment Status thành công
- [ ] Pagination hoạt động đúng
- [ ] Authorization hoạt động (User không thể tạo location)
- [ ] Soft delete hoạt động (deleted items không xuất hiện)

---

## 🐛 **TROUBLESHOOTING**

### **Lỗi 401 Unauthorized:**
- Kiểm tra token còn hạn không
- Re-login để lấy token mới
- Đảm bảo đã click "Authorize" trong Swagger

### **Lỗi 403 Forbidden:**
- Kiểm tra role của user (User/Owner/Admin)
- Một số endpoints chỉ dành cho Admin

### **Lỗi 404 Not Found:**
- Kiểm tra ID có đúng không
- Kiểm tra item có bị soft delete không

### **Lỗi 400 Bad Request:**
- Kiểm tra request body format
- Kiểm tra validation errors trong response

---

## 📝 **NOTES**

1. **Token Expiry:** Access token hết hạn sau 1 giờ, cần refresh hoặc re-login
2. **Time Format:** Sử dụng ISO 8601 format: `2026-01-27T10:00:00Z`
3. **Coordinates:** Latitude: -90 to 90, Longitude: -180 to 180
4. **VNPay:** Payment URL chỉ hoạt động với VNPay credentials hợp lệ

---

**Happy Testing!** 🎉
