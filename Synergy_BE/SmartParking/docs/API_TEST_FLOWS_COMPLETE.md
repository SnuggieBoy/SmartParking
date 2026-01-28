# SmartParking API - Complete Test Flows Documentation

> **Tài liệu dành cho Mobile & Frontend Developers**  
> **Version:** 1.0  
> **Base URL (Development):** `https://localhost:7278` hoặc `http://localhost:5070`

---

## Table of Contents

1. [Authentication & Authorization](#1-authentication--authorization)
2. [Role: USER (Driver) - Test Flows](#2-role-user-driver---test-flows)
3. [Role: OWNER - Test Flows](#3-role-owner---test-flows)
4. [Role: ADMIN - Test Flows](#4-role-admin---test-flows)
5. [Error Handling & Edge Cases](#5-error-handling--edge-cases)
6. [Payment Flow - Complete Guide](#6-payment-flow---complete-guide)
7. [Sample Request/Response](#7-sample-requestresponse)

---

## 1. Authentication & Authorization

### 1.1 Register New User (2-Step OTP Flow)

#### Step 1: Request OTP
```
POST /api/auth/register-request
Content-Type: application/json

{
  "fullName": "Nguyen Van A",
  "email": "test@example.com",
  "phone": "0901234567",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "OTP has been sent to your email",
  "data": null
}
```

**Possible Errors:**
- `400 Bad Request`: Email đã tồn tại, validation fail
- `429 Too Many Requests`: Gửi OTP quá nhiều lần

#### Step 2: Verify OTP & Complete Registration
```
POST /api/auth/verify-otp
Content-Type: application/json

{
  "email": "test@example.com",
  "otpCode": "123456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "OTP verified successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "d4f7a8b2-9c1e-4f3a-b5d6-e7f8a9b0c1d2",
    "expiresAt": "2026-01-25T14:00:00Z",
    "userId": "12345678-1234-1234-1234-123456789012",
    "email": "test@example.com",
    "fullName": "Nguyen Van A",
    "role": "User"
  }
}
```

**Possible Errors:**
- `400 Bad Request`: OTP sai, hết hạn

#### Step 2.5: Resend OTP (nếu chưa nhận được)
```
POST /api/auth/resend-otp
Content-Type: application/json

{
  "email": "test@example.com"
}
```

### 1.2 Login

```
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2026-01-25T14:00:00Z",
    "userId": "...",
    "email": "test@example.com",
    "fullName": "Nguyen Van A",
    "role": "User"
  }
}
```

**Possible Errors:**
- `401 Unauthorized`: Sai email/password

### 1.3 Google Login

```
POST /api/auth/google
Content-Type: application/json

{
  "googleToken": "ya29.a0AfH6SM..."
}
```

### 1.4 Refresh Token

```
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "d4f7a8b2-9c1e-4f3a-b5d6-e7f8a9b0c1d2"
}
```

### 1.5 Logout

```
POST /api/auth/logout
Authorization: Bearer {accessToken}
```

### 1.6 Forgot Password Flow

#### Step 1: Request Reset OTP
```
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}
```

#### Step 2: Reset Password with OTP
```
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "test@example.com",
  "otpCode": "654321",
  "newPassword": "NewSecurePass456!"
}
```

---

## 2. Role: USER (Driver) - Test Flows

> **Header Required cho tất cả endpoints có [Authorize]:**
> ```
> Authorization: Bearer {accessToken}
> ```

### 2.1 Vehicle Management

#### 2.1.1 Create Vehicle
```
POST /api/vehicles
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "licensePlate": "51A-12345",
  "vehicleType": "Car",
  "brand": "Toyota",
  "model": "Camry",
  "color": "Black"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Vehicle created successfully",
  "data": {
    "vehicleId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    "licensePlate": "51A-12345",
    "vehicleType": "Car",
    "brand": "Toyota",
    "model": "Camry",
    "color": "Black",
    "isActive": true,
    "createdAt": "2026-01-25T10:00:00Z"
  }
}
```

#### 2.1.2 Get My Vehicles
```
GET /api/vehicles/my-vehicles?activeOnly=true
Authorization: Bearer {accessToken}
```

#### 2.1.3 Get Vehicle by ID
```
GET /api/vehicles/{vehicleId}
Authorization: Bearer {accessToken}
```

#### 2.1.4 Update Vehicle
```
PUT /api/vehicles/{vehicleId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "licensePlate": "51A-12345",
  "vehicleType": "Car",
  "brand": "Toyota",
  "model": "Vios",
  "color": "White"
}
```

#### 2.1.5 Delete Vehicle (Soft Delete)
```
DELETE /api/vehicles/{vehicleId}
Authorization: Bearer {accessToken}
```

---

### 2.2 Browse Parking Lots (Public)

#### 2.2.1 Get All Parking Lots (Paginated)
```
GET /api/parking-lots?page=1&pageSize=10&search=quan1
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Parking lots retrieved successfully",
  "data": {
    "items": [
      {
        "parkingLotId": "...",
        "name": "Bãi xe ABC",
        "address": "123 Nguyễn Huệ, Q1",
        "description": "Bãi xe 24/7",
        "totalSlots": 100,
        "availableSlots": 45,
        "pricePerHour": 20000,
        "latitude": 10.7769,
        "longitude": 106.7009,
        "distanceKm": null,
        "status": "Approved",
        "isActive": true,
        "openTime": "06:00:00",
        "closeTime": "22:00:00"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

#### 2.2.2 Get Nearby Parking Lots (GPS-based)
```
GET /api/parking-lots/nearby?lat=10.7769&lng=106.7009&radiusKm=5&maxResults=20
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Nearby parking lots retrieved successfully",
  "data": [
    {
      "parkingLotId": "...",
      "name": "Bãi xe ABC",
      "address": "123 Nguyễn Huệ, Q1",
      "latitude": 10.7780,
      "longitude": 106.7020,
      "distanceKm": 0.45,
      "pricePerHour": 20000,
      "availableSlots": 45
    }
  ]
}
```

#### 2.2.3 Get Parking Lot Detail
```
GET /api/parking-lots/{parkingLotId}
```

---

### 2.3 Parking Locations (Map Integration)

#### 2.3.1 Find Nearby Locations
```
GET /api/locations/nearby?lat=10.7769&lng=106.7009&radius=3000
```

#### 2.3.2 Search Locations by Address
```
GET /api/locations/search?province=Hồ Chí Minh&district=Quận 1&search=parking
```

#### 2.3.3 Get Location by Parking Lot ID
```
GET /api/locations/parking-lot/{parkingLotId}
```

---

### 2.4 Booking Flow (CRITICAL)

#### 2.4.1 Create Booking
```
POST /api/bookings
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "parkingLotId": "11111111-2222-3333-4444-555555555555",
  "vehicleId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "startTime": "2026-01-25T14:00:00Z",
  "endTime": "2026-01-25T18:00:00Z",
  "notes": "Xe màu đen, biển số 51A-12345"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Booking created successfully",
  "data": {
    "bookingId": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
    "parkingLotId": "...",
    "parkingLotName": "Bãi xe ABC",
    "vehicleId": "...",
    "licensePlate": "51A-12345",
    "userId": "...",
    "startTime": "2026-01-25T14:00:00Z",
    "endTime": "2026-01-25T18:00:00Z",
    "totalHours": 4,
    "totalPrice": 80000,
    "status": "Pending",
    "notes": "Xe màu đen, biển số 51A-12345",
    "createdAt": "2026-01-25T10:30:00Z"
  }
}
```

**Booking Status Flow:**
```
Pending → Confirmed (after payment) → InProgress (after check-in) → Completed (after check-out)
                                                                  ↓
                                                              Cancelled
```

#### 2.4.2 Get My Bookings
```
GET /api/bookings/my-bookings?status=Pending&page=1&pageSize=10
Authorization: Bearer {accessToken}
```

**Status values:** `Pending`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`

#### 2.4.3 Get Booking Detail
```
GET /api/bookings/{bookingId}
Authorization: Bearer {accessToken}
```

#### 2.4.4 Update Booking (chỉ khi status = Pending)
```
PUT /api/bookings/{bookingId}
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "startTime": "2026-01-25T15:00:00Z",
  "endTime": "2026-01-25T19:00:00Z",
  "notes": "Updated note"
}
```

#### 2.4.5 Cancel Booking
```
POST /api/bookings/{bookingId}/cancel
Authorization: Bearer {accessToken}
```

**Possible Errors:**
- `400 Bad Request`: Booking đã InProgress/Completed không thể hủy

#### 2.4.6 Check-In (khi đến bãi xe)
```
POST /api/bookings/{bookingId}/check-in
Authorization: Bearer {accessToken}
```

**Requirements:**
- Booking status phải là `Confirmed`
- User phải là chủ booking

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Check-in successful",
  "data": {
    "bookingId": "...",
    "checkInTime": "2026-01-25T14:05:00Z",
    "status": "InProgress"
  }
}
```

#### 2.4.7 Check-Out (khi rời bãi xe)
```
POST /api/bookings/{bookingId}/check-out
Authorization: Bearer {accessToken}
```

**Requirements:**
- Booking status phải là `InProgress`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Check-out successful",
  "data": {
    "bookingId": "...",
    "checkInTime": "2026-01-25T14:05:00Z",
    "checkOutTime": "2026-01-25T18:10:00Z",
    "actualDurationMinutes": 245,
    "totalPrice": 85000,
    "status": "Completed"
  }
}
```

---

### 2.5 Payment Flow

#### 2.5.1 Create VNPay Payment
```
POST /api/payments/create
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "bookingId": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
  "amount": 80000,
  "description": "Thanh toán đặt chỗ đỗ xe"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Payment created successfully",
  "data": {
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=...",
    "transactionRef": "TXN123456789",
    "amount": 80000
  }
}
```

#### 2.5.2 Create SePay Payment (QR Bank Transfer)
```
POST /api/payments/sepay/create
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "bookingId": "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
  "amount": 80000,
  "description": "Thanh toán đặt chỗ đỗ xe"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "SePay payment created successfully",
  "data": {
    "orderId": "SP123456789",
    "qrCodeUrl": "https://qr.sepay.vn/img?acc=...",
    "bankAccount": "1234567890",
    "bankName": "MB Bank",
    "amount": 80000,
    "transferContent": "SP123456789",
    "expiresAt": "2026-01-25T11:00:00Z"
  }
}
```

#### 2.5.3 Check Payment Status
```
GET /api/payments/booking/{bookingId}
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Payment status retrieved",
  "data": {
    "bookingId": "...",
    "transactionId": "...",
    "amount": 80000,
    "status": "Success",
    "paymentMethod": "SePay",
    "paidAt": "2026-01-25T10:45:00Z"
  }
}
```

---

### 2.6 User to Owner Upgrade

#### 2.6.1 View Available Plans
```
GET /api/owners/plans
Authorization: Bearer {accessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "planType": "Monthly",
      "price": 299000,
      "durationDays": 30,
      "description": "Gói tháng - 299,000 VND"
    },
    {
      "planType": "Yearly",
      "price": 2990000,
      "durationDays": 365,
      "description": "Gói năm - 2,990,000 VND (tiết kiệm 2 tháng)"
    }
  ]
}
```

#### 2.6.2 Create Upgrade Request
```
POST /api/owners/upgrade-request
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "parkingLotName": "Bãi xe Nguyễn Văn A",
  "parkingLotAddress": "456 Lê Lợi, Quận 1, TP.HCM",
  "latitude": 10.7750,
  "longitude": 106.7020,
  "planType": "Monthly"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Owner upgrade request created successfully",
  "data": {
    "requestId": "...",
    "userId": "...",
    "parkingLotName": "Bãi xe Nguyễn Văn A",
    "parkingLotAddress": "456 Lê Lợi, Quận 1, TP.HCM",
    "planType": "Monthly",
    "subscriptionFee": 299000,
    "status": "PendingPayment",
    "createdAt": "2026-01-25T11:00:00Z"
  }
}
```

**Status Flow:**
```
PendingPayment → PendingApproval (after payment) → Approved → (User role changed to Owner)
                                                 ↓
                                              Rejected
```

#### 2.6.3 Get My Latest Upgrade Request
```
GET /api/owners/upgrade-request/my
Authorization: Bearer {accessToken}
```

---

## 3. Role: OWNER - Test Flows

> Owner = User đã được approve upgrade request. Role trong JWT sẽ là "Owner".

### 3.1 Manage My Parking Lots

#### 3.1.1 Get My Parking Lots
```
GET /api/parking-lots/my
Authorization: Bearer {ownerAccessToken}
```

#### 3.1.2 Create New Parking Lot
```
POST /api/parking-lots
Authorization: Bearer {ownerAccessToken}
Content-Type: application/json

{
  "name": "Bãi xe XYZ",
  "address": "789 Hai Bà Trưng, Q3",
  "description": "Bãi xe rộng rãi, có mái che",
  "totalSlots": 50,
  "pricePerHour": 15000,
  "latitude": 10.7850,
  "longitude": 106.6950,
  "openTime": "05:00:00",
  "closeTime": "23:00:00",
  "imageUrls": ["https://example.com/img1.jpg"]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Parking lot created. Waiting for admin approval.",
  "data": {
    "parkingLotId": "...",
    "status": "PendingApproval",
    "isActive": false,
    ...
  }
}
```

**Note:** Owner tạo parking lot sẽ có status = `PendingApproval`, chờ Admin duyệt.

#### 3.1.3 Update My Parking Lot
```
PUT /api/parking-lots/{parkingLotId}
Authorization: Bearer {ownerAccessToken}
Content-Type: application/json

{
  "name": "Bãi xe XYZ - Updated",
  "description": "Mô tả mới",
  "pricePerHour": 18000
}
```

#### 3.1.4 Toggle Active Status
```
PATCH /api/parking-lots/{parkingLotId}/toggle-active
Authorization: Bearer {ownerAccessToken}
```

#### 3.1.5 Delete My Parking Lot
```
DELETE /api/parking-lots/{parkingLotId}
Authorization: Bearer {ownerAccessToken}
```

#### 3.1.6 View Bookings of My Parking Lot
```
GET /api/parking-lots/{parkingLotId}/bookings?page=1&pageSize=10
Authorization: Bearer {ownerAccessToken}
```

---

### 3.2 Owner Bank Account Management

#### 3.2.1 Get My Bank Accounts
```
GET /api/owners/bank-accounts
Authorization: Bearer {ownerAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "bankAccountId": "...",
      "bankCode": "MB",
      "bankName": "MB Bank",
      "accountNumber": "1234567890",
      "accountHolderName": "NGUYEN VAN A",
      "isDefault": true,
      "isVerified": false,
      "createdAt": "2026-01-25T10:00:00Z"
    }
  ]
}
```

#### 3.2.2 Add Bank Account
```
POST /api/owners/bank-accounts
Authorization: Bearer {ownerAccessToken}
Content-Type: application/json

{
  "bankCode": "VCB",
  "bankName": "Vietcombank",
  "accountNumber": "0071000123456",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true
}
```

#### 3.2.3 Update Bank Account
```
PUT /api/owners/bank-accounts/{bankAccountId}
Authorization: Bearer {ownerAccessToken}
Content-Type: application/json

{
  "bankCode": "VCB",
  "bankName": "Vietcombank",
  "accountNumber": "0071000123456",
  "accountHolderName": "NGUYEN VAN A",
  "isDefault": true
}
```

#### 3.2.4 Delete Bank Account
```
DELETE /api/owners/bank-accounts/{bankAccountId}
Authorization: Bearer {ownerAccessToken}
```

---

## 4. Role: ADMIN - Test Flows

> Admin có full quyền trên tất cả resources.

### 4.1 Dashboard Statistics

#### 4.1.1 Users Summary
```
GET /api/admin/dashboard/users-summary
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalUsers": 1500,
    "totalOwners": 120,
    "totalDrivers": 1350,
    "newUsersToday": 15
  }
}
```

#### 4.1.2 Parking Lots Summary
```
GET /api/admin/dashboard/parking-lots-summary
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalActiveParkingLots": 85,
    "totalInactiveParkingLots": 12,
    "pendingApprovalParkingLots": 5
  }
}
```

#### 4.1.3 Revenue Today
```
GET /api/admin/dashboard/revenue-today
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalRevenue": 15000000,
    "totalCommission": 1500000,
    "netOwnerRevenue": 13500000,
    "transactionCount": 125
  }
}
```

#### 4.1.4 Bookings Summary
```
GET /api/admin/dashboard/bookings-summary
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "activeBookings": 45,
    "pendingBookings": 23,
    "completedToday": 89
  }
}
```

#### 4.1.5 Recent Activities
```
GET /api/admin/dashboard/recent-activities?limit=10
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "activityType": "NewBooking",
      "description": "Nguyen Van A đặt chỗ tại Bãi xe XYZ",
      "timestamp": "2026-01-25T10:30:00Z",
      "relatedId": "..."
    },
    {
      "activityType": "PaymentReceived",
      "description": "Thanh toán 80,000 VND từ Tran Van B",
      "timestamp": "2026-01-25T10:25:00Z",
      "relatedId": "..."
    }
  ]
}
```

---

### 4.2 User Management

#### 4.2.1 Get All Users
```
GET /api/admin/users?search=nguyen&roleName=User&isActive=true&page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

**Query Parameters:**
- `search`: Tìm theo tên/email
- `roleName`: `User`, `Owner`, `Admin`
- `isActive`: `true`/`false`
- `emailConfirmed`: `true`/`false`

#### 4.2.2 Get User by ID
```
GET /api/admin/users/{userId}
Authorization: Bearer {adminAccessToken}
```

#### 4.2.3 Update User
```
PUT /api/admin/users/{userId}
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "fullName": "Nguyen Van A Updated",
  "phone": "0901234568"
}
```

#### 4.2.4 Toggle User Active Status
```
PATCH /api/admin/users/{userId}/toggle-active
Authorization: Bearer {adminAccessToken}
```

---

### 4.3 Parking Lot Management

#### 4.3.1 Get All Parking Lots (Admin View)
```
GET /api/admin/parking-lots?search=&isActive=&status=&ownerId=&page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

**Query Parameters:**
- `status`: `PendingApproval`, `Approved`, `Rejected`, `Inactive`
- `ownerId`: Filter by owner

#### 4.3.2 Get Pending Parking Lots (for approval)
```
GET /api/admin/parking-lots/pending?page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

#### 4.3.3 Approve Parking Lot
```
POST /api/admin/parking-lots/{parkingLotId}/approve
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Parking lot approved successfully",
  "data": {
    "parkingLotId": "...",
    "status": "Approved",
    "isActive": true,
    ...
  }
}
```

#### 4.3.4 Reject Parking Lot
```
POST /api/admin/parking-lots/{parkingLotId}/reject
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "reason": "Địa chỉ không hợp lệ, vui lòng cung cấp giấy tờ chứng minh quyền sở hữu."
}
```

#### 4.3.5 Create Parking Lot (Admin can create for any owner)
```
POST /api/admin/parking-lots?ownerId={targetOwnerId}
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "name": "Bãi xe Admin tạo",
  "address": "100 ABC Street",
  "totalSlots": 100,
  "pricePerHour": 25000,
  "latitude": 10.8000,
  "longitude": 106.7000
}
```

**Note:** Parking lot do Admin tạo sẽ có status = `Approved` ngay lập tức.

#### 4.3.6 Update Parking Lot
```
PUT /api/admin/parking-lots/{parkingLotId}
Authorization: Bearer {adminAccessToken}
```

#### 4.3.7 Toggle Active Status
```
PATCH /api/admin/parking-lots/{parkingLotId}/toggle-active
Authorization: Bearer {adminAccessToken}
```

#### 4.3.8 Delete Parking Lot
```
DELETE /api/admin/parking-lots/{parkingLotId}
Authorization: Bearer {adminAccessToken}
```

#### 4.3.9 View Bookings of Parking Lot
```
GET /api/admin/parking-lots/{parkingLotId}/bookings?page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

---

### 4.4 Booking Management

#### 4.4.1 Get All Bookings
```
GET /api/admin/bookings?status=Pending&userId=&parkingLotId=&page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

#### 4.4.2 Get Booking by ID
```
GET /api/admin/bookings/{bookingId}
Authorization: Bearer {adminAccessToken}
```

#### 4.4.3 Update Booking
```
PUT /api/admin/bookings/{bookingId}
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "startTime": "2026-01-25T15:00:00Z",
  "endTime": "2026-01-25T20:00:00Z"
}
```

#### 4.4.4 Cancel Booking
```
POST /api/admin/bookings/{bookingId}/cancel
Authorization: Bearer {adminAccessToken}
```

#### 4.4.5 Check-In Booking
```
POST /api/admin/bookings/{bookingId}/check-in
Authorization: Bearer {adminAccessToken}
```

#### 4.4.6 Check-Out Booking
```
POST /api/admin/bookings/{bookingId}/check-out
Authorization: Bearer {adminAccessToken}
```

---

### 4.5 Vehicle Management

#### 4.5.1 Get Vehicle by ID
```
GET /api/admin/vehicles/{vehicleId}
Authorization: Bearer {adminAccessToken}
```

#### 4.5.2 Get Vehicles by User ID
```
GET /api/admin/vehicles/user/{userId}?activeOnly=false
Authorization: Bearer {adminAccessToken}
```

#### 4.5.3 Update Vehicle
```
PUT /api/admin/vehicles/{vehicleId}
Authorization: Bearer {adminAccessToken}
```

#### 4.5.4 Delete Vehicle
```
DELETE /api/admin/vehicles/{vehicleId}
Authorization: Bearer {adminAccessToken}
```

---

### 4.6 Location Management

#### 4.6.1 Get All Locations
```
GET /api/admin/locations?province=&district=&ward=&search=&page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

#### 4.6.2 Create Location
```
POST /api/admin/locations
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "parkingLotId": "...",
  "address": "123 ABC Street",
  "province": "Hồ Chí Minh",
  "district": "Quận 1",
  "ward": "Phường Bến Nghé",
  "latitude": 10.7769,
  "longitude": 106.7009
}
```

#### 4.6.3 Update Location
```
PUT /api/admin/locations/{locationId}
Authorization: Bearer {adminAccessToken}
```

#### 4.6.4 Delete Location
```
DELETE /api/admin/locations/{locationId}
Authorization: Bearer {adminAccessToken}
```

---

### 4.7 Owner Upgrade Request Management

#### 4.7.1 Get All Upgrade Requests
```
GET /api/admin/owners/upgrade-requests?status=PendingApproval&page=1&pageSize=20
Authorization: Bearer {adminAccessToken}
```

**Status values:** `PendingPayment`, `PendingApproval`, `Approved`, `Rejected`

#### 4.7.2 Approve Upgrade Request
```
POST /api/admin/owners/upgrade-requests/{requestId}/approve
Authorization: Bearer {adminAccessToken}
```

**Side Effects:**
- User role được đổi thành `Owner`
- Parking lot được tạo với status `PendingApproval`

#### 4.7.3 Reject Upgrade Request
```
POST /api/admin/owners/upgrade-requests/{requestId}/reject
Authorization: Bearer {adminAccessToken}
Content-Type: application/json

{
  "reason": "Thông tin bãi xe không đầy đủ, vui lòng bổ sung giấy phép kinh doanh."
}
```

---

### 4.8 Owner Bank Account Verification

#### 4.8.1 Get Bank Accounts of an Owner
```
GET /api/admin/owners/{ownerId}/bank-accounts
Authorization: Bearer {adminAccessToken}
```

#### 4.8.2 Verify Bank Account
```
POST /api/admin/owners/bank-accounts/{bankAccountId}/verify
Authorization: Bearer {adminAccessToken}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Owner bank account verified successfully",
  "data": {
    "bankAccountId": "...",
    "isVerified": true,
    "verifiedAt": "2026-01-25T11:00:00Z",
    "verifiedBy": "..."
  }
}
```

---

## 5. Error Handling & Edge Cases

### 5.1 Common Error Response Format

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detail error 1", "Detail error 2"]
}
```

### 5.2 HTTP Status Codes

| Code | Meaning | Common Scenarios |
|------|---------|------------------|
| 200 | OK | Successful GET/PUT/PATCH/DELETE |
| 201 | Created | Successful POST (resource created) |
| 400 | Bad Request | Validation failed, invalid data |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | User doesn't have permission |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Duplicate resource (e.g., email exists) |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |

### 5.3 Edge Cases to Test

#### Authentication
- [ ] Login với email chưa verify
- [ ] Login với password sai 5 lần liên tiếp
- [ ] OTP hết hạn (sau 5 phút)
- [ ] OTP sai 3 lần liên tiếp
- [ ] Refresh token đã hết hạn
- [ ] Access endpoint với token đã logout

#### Booking
- [ ] Đặt booking khi parking lot đã hết chỗ
- [ ] Đặt booking với thời gian trong quá khứ
- [ ] Đặt booking với endTime < startTime
- [ ] Cancel booking đã Completed
- [ ] Check-in booking chưa Confirmed
- [ ] Check-out booking chưa Check-in
- [ ] Double check-in cùng booking

#### Payment
- [ ] Tạo payment cho booking đã thanh toán
- [ ] Webhook với signature sai
- [ ] Webhook với amount không khớp
- [ ] Webhook duplicate (idempotency check)

#### Owner
- [ ] Tạo parking lot khi chưa là Owner
- [ ] Update parking lot của Owner khác
- [ ] Delete bank account đang là default duy nhất

#### Admin
- [ ] Approve parking lot đã Approved
- [ ] Reject upgrade request đã Approved
- [ ] Toggle active user đang là Admin

---

## 6. Payment Flow - Complete Guide

### 6.1 Booking Payment Flow (User)

```
┌─────────────────────────────────────────────────────────────────┐
│                    BOOKING PAYMENT FLOW                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. User creates booking                                         │
│     POST /api/bookings                                           │
│     → Booking created with status = "Pending"                    │
│                                                                  │
│  2. User creates payment                                         │
│     POST /api/payments/sepay/create                              │
│     → Returns QR code / payment URL                              │
│                                                                  │
│  3. User completes payment (scan QR / bank transfer)             │
│                                                                  │
│  4. SePay/VNPay sends webhook to backend                         │
│     POST /api/payments/sepay/webhook (automatic)                 │
│     → Backend verifies signature                                 │
│     → Backend verifies amount                                    │
│     → Backend updates PaymentTransaction status = "Success"      │
│     → Backend updates Booking status = "Confirmed"               │
│                                                                  │
│  5. User checks payment status                                   │
│     GET /api/payments/booking/{bookingId}                        │
│     → Returns status = "Success"                                 │
│                                                                  │
│  6. User arrives at parking lot                                  │
│     POST /api/bookings/{bookingId}/check-in                      │
│     → Booking status = "InProgress"                              │
│                                                                  │
│  7. User leaves parking lot                                      │
│     POST /api/bookings/{bookingId}/check-out                     │
│     → Booking status = "Completed"                               │
│     → Calculates actual duration & final price                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 6.2 Owner Upgrade Payment Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                 OWNER UPGRADE PAYMENT FLOW                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. User views available plans                                   │
│     GET /api/owners/plans                                        │
│     → Returns Monthly (299K) / Yearly (2.99M) options            │
│                                                                  │
│  2. User creates upgrade request                                 │
│     POST /api/owners/upgrade-request                             │
│     → Request created with status = "PendingPayment"             │
│                                                                  │
│  3. User pays subscription fee                                   │
│     (Similar to booking payment flow)                            │
│     → Request status = "PendingApproval"                         │
│                                                                  │
│  4. Admin reviews and approves                                   │
│     POST /api/admin/owners/upgrade-requests/{id}/approve         │
│     → Request status = "Approved"                                │
│     → User role changed to "Owner"                               │
│     → Parking lot created with status = "PendingApproval"        │
│                                                                  │
│  5. User logs in again to get new JWT with "Owner" role          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 6.3 Money Distribution

```
┌─────────────────────────────────────────────────────────────────┐
│                    MONEY DISTRIBUTION                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  User pays: 100,000 VND                                          │
│                                                                  │
│  Commission (10%): 10,000 VND → Platform (SmartParking)          │
│  Net Revenue (90%): 90,000 VND → Owner                           │
│                                                                  │
│  Owner receives payout to their verified bank account            │
│  (Manual process or future automated payout)                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. Sample Request/Response

### 7.1 Complete Booking Scenario

```bash
# Step 1: Login
curl -X POST https://localhost:7278/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"Test123!"}'

# Save the accessToken from response

# Step 2: Get my vehicles
curl -X GET https://localhost:7278/api/vehicles/my-vehicles \
  -H "Authorization: Bearer {accessToken}"

# Step 3: Search nearby parking lots
curl -X GET "https://localhost:7278/api/parking-lots/nearby?lat=10.7769&lng=106.7009&radiusKm=5"

# Step 4: Create booking
curl -X POST https://localhost:7278/api/bookings \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "parkingLotId": "...",
    "vehicleId": "...",
    "startTime": "2026-01-25T14:00:00Z",
    "endTime": "2026-01-25T18:00:00Z"
  }'

# Step 5: Create payment
curl -X POST https://localhost:7278/api/payments/sepay/create \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{
    "bookingId": "...",
    "amount": 80000,
    "description": "Thanh toán đặt chỗ"
  }'

# Step 6: (User scans QR and pays via bank app)

# Step 7: Check payment status
curl -X GET https://localhost:7278/api/payments/booking/{bookingId} \
  -H "Authorization: Bearer {accessToken}"

# Step 8: Check-in when arriving
curl -X POST https://localhost:7278/api/bookings/{bookingId}/check-in \
  -H "Authorization: Bearer {accessToken}"

# Step 9: Check-out when leaving
curl -X POST https://localhost:7278/api/bookings/{bookingId}/check-out \
  -H "Authorization: Bearer {accessToken}"
```

---

## Appendix: API Endpoints Summary

### Public Endpoints (No Auth Required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/register-request | Request OTP for registration |
| POST | /api/auth/verify-otp | Verify OTP and complete registration |
| POST | /api/auth/resend-otp | Resend OTP |
| POST | /api/auth/login | Login |
| POST | /api/auth/google | Google OAuth login |
| POST | /api/auth/refresh | Refresh access token |
| POST | /api/auth/forgot-password | Request password reset OTP |
| POST | /api/auth/reset-password | Reset password with OTP |
| GET | /api/parking-lots | List parking lots |
| GET | /api/parking-lots/nearby | Find nearby parking lots |
| GET | /api/parking-lots/{id} | Get parking lot detail |
| GET | /api/locations/nearby | Find nearby locations |
| GET | /api/locations/search | Search locations |
| GET | /api/locations/{id} | Get location detail |
| GET | /api/locations/parking-lot/{id} | Get location by parking lot |

### User Endpoints (Auth Required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/logout | Logout |
| GET | /api/vehicles/my-vehicles | Get my vehicles |
| GET | /api/vehicles/{id} | Get vehicle detail |
| POST | /api/vehicles | Create vehicle |
| PUT | /api/vehicles/{id} | Update vehicle |
| DELETE | /api/vehicles/{id} | Delete vehicle |
| GET | /api/bookings/my-bookings | Get my bookings |
| GET | /api/bookings/{id} | Get booking detail |
| POST | /api/bookings | Create booking |
| PUT | /api/bookings/{id} | Update booking |
| POST | /api/bookings/{id}/cancel | Cancel booking |
| POST | /api/bookings/{id}/check-in | Check-in |
| POST | /api/bookings/{id}/check-out | Check-out |
| GET | /api/bookings/history | **NEW** Get booking history (completed only) |
| GET | /api/bookings/{id}/invoice | **NEW** Get invoice for completed booking |
| PUT | /api/bookings/{id}/extend | **NEW** Extend booking time |
| POST | /api/payments/create | Create VNPay payment |
| POST | /api/payments/sepay/create | Create SePay payment |
| GET | /api/payments/booking/{id} | Get payment status |
| GET | /api/payments/history | **NEW** Get payment history |
| GET | /api/users/profile | **NEW** Get my profile |
| PUT | /api/users/profile | **NEW** Update my profile |
| GET | /api/owners/plans | Get owner subscription plans |
| POST | /api/owners/upgrade-request | Create upgrade request |
| GET | /api/owners/upgrade-request/my | Get my upgrade request |

### Owner Endpoints (Owner Role Required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/parking-lots/my | Get my parking lots |
| POST | /api/parking-lots | Create parking lot |
| PUT | /api/parking-lots/{id} | Update parking lot |
| PATCH | /api/parking-lots/{id}/toggle-active | Toggle active |
| DELETE | /api/parking-lots/{id} | Delete parking lot |
| GET | /api/parking-lots/{id}/bookings | Get bookings of my lot |
| GET | /api/owners/bank-accounts | Get my bank accounts |
| POST | /api/owners/bank-accounts | Add bank account |
| PUT | /api/owners/bank-accounts/{id} | Update bank account |
| DELETE | /api/owners/bank-accounts/{id} | Delete bank account |
| GET | /api/owners/dashboard | **NEW** Owner dashboard (stats) |
| GET | /api/owners/earnings | **NEW** Owner earnings summary |
| GET | /api/owners/earnings/history | **NEW** Owner earnings history |
| GET | /api/owners/payouts | **NEW** Owner payout history |
| GET | /api/owners/subscription | **NEW** Subscription status |
| POST | /api/owners/subscription/renew | **NEW** Renew subscription |
| GET | /api/owners/parking-lots/{id}/statistics | **NEW** Parking lot statistics |

### Admin Endpoints (Admin Role Required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/admin/dashboard/users-summary | Users statistics |
| GET | /api/admin/dashboard/parking-lots-summary | Parking lots statistics |
| GET | /api/admin/dashboard/revenue-today | Revenue statistics |
| GET | /api/admin/dashboard/bookings-summary | Bookings statistics |
| GET | /api/admin/dashboard/recent-activities | Recent activities |
| GET | /api/admin/users | List all users |
| GET | /api/admin/users/{id} | Get user detail |
| PUT | /api/admin/users/{id} | Update user |
| PATCH | /api/admin/users/{id}/toggle-active | Toggle user active |
| GET | /api/admin/parking-lots | List all parking lots |
| GET | /api/admin/parking-lots/pending | List pending parking lots |
| GET | /api/admin/parking-lots/{id} | Get parking lot detail |
| POST | /api/admin/parking-lots | Create parking lot |
| PUT | /api/admin/parking-lots/{id} | Update parking lot |
| PATCH | /api/admin/parking-lots/{id}/toggle-active | Toggle active |
| POST | /api/admin/parking-lots/{id}/approve | Approve parking lot |
| POST | /api/admin/parking-lots/{id}/reject | Reject parking lot |
| DELETE | /api/admin/parking-lots/{id} | Delete parking lot |
| GET | /api/admin/parking-lots/{id}/bookings | Get bookings |
| GET | /api/admin/bookings | List all bookings |
| GET | /api/admin/bookings/{id} | Get booking detail |
| PUT | /api/admin/bookings/{id} | Update booking |
| POST | /api/admin/bookings/{id}/cancel | Cancel booking |
| POST | /api/admin/bookings/{id}/check-in | Check-in booking |
| POST | /api/admin/bookings/{id}/check-out | Check-out booking |
| GET | /api/admin/vehicles/{id} | Get vehicle detail |
| GET | /api/admin/vehicles/user/{userId} | Get user's vehicles |
| PUT | /api/admin/vehicles/{id} | Update vehicle |
| DELETE | /api/admin/vehicles/{id} | Delete vehicle |
| GET | /api/admin/locations | List all locations |
| GET | /api/admin/locations/{id} | Get location detail |
| POST | /api/admin/locations | Create location |
| PUT | /api/admin/locations/{id} | Update location |
| DELETE | /api/admin/locations/{id} | Delete location |
| GET | /api/admin/owners/upgrade-requests | List upgrade requests |
| POST | /api/admin/owners/upgrade-requests/{id}/approve | Approve request |
| POST | /api/admin/owners/upgrade-requests/{id}/reject | Reject request |
| GET | /api/admin/owners/{ownerId}/bank-accounts | Get owner's bank accounts |
| POST | /api/admin/owners/bank-accounts/{id}/verify | Verify bank account |
| GET | /api/admin/transactions | **NEW** List all transactions |
| GET | /api/admin/transactions/{id} | **NEW** Get transaction detail |
| POST | /api/admin/transactions/{id}/refund | **NEW** Process refund |
| GET | /api/admin/reports/revenue | **NEW** Revenue report |
| GET | /api/admin/reports/bookings | **NEW** Bookings report |
| GET | /api/admin/settings | **NEW** Get system settings |
| PUT | /api/admin/settings | **NEW** Update system settings |

---

## Appendix B: New Endpoints Detail (v2.0)

### User: Profile Management

```
GET /api/users/profile
Authorization: Bearer {token}

Response:
{
  "userId": "...",
  "fullName": "...",
  "email": "...",
  "phone": "...",
  "roleName": "User",
  "totalBookings": 10,
  "completedBookings": 8,
  "totalVehicles": 2,
  "totalParkingLots": 0
}
```

```
PUT /api/users/profile
Authorization: Bearer {token}
Body: { "fullName": "New Name", "phone": "0909123456" }
```

### User: Booking History & Invoice

```
GET /api/bookings/history?page=1&pageSize=10
Authorization: Bearer {token}

Response:
{
  "items": [{
    "bookingId": "...",
    "parkingLotName": "...",
    "totalAmount": 80000,
    "paymentStatus": "Success",
    "completedAt": "..."
  }],
  "totalCount": 25
}
```

```
GET /api/bookings/{id}/invoice
Authorization: Bearer {token}

Response:
{
  "invoiceNumber": "INV-ABC12345-20260125",
  "customerName": "...",
  "parkingLotName": "...",
  "totalAmount": 80000,
  "paymentMethod": "SePay",
  "paidAt": "..."
}
```

### Owner: Dashboard & Earnings

```
GET /api/owners/dashboard
Authorization: Bearer {ownerToken}

Response:
{
  "totalParkingLots": 3,
  "activeParkingLots": 2,
  "totalBookings": 150,
  "activeBookings": 5,
  "totalEarnings": 5000000,
  "earningsToday": 200000,
  "earningsThisMonth": 1500000
}
```

```
GET /api/owners/earnings
GET /api/owners/earnings/history?months=12
GET /api/owners/subscription
GET /api/owners/parking-lots/{id}/statistics
```

### Admin: Transactions & Reports

```
GET /api/admin/transactions?status=Success&fromDate=2026-01-01&toDate=2026-01-31
Authorization: Bearer {adminToken}

Response:
{
  "items": [{
    "paymentId": "...",
    "userName": "...",
    "amount": 80000,
    "paymentMethod": "SePay",
    "paymentStatus": "Success"
  }]
}
```

```
POST /api/admin/transactions/{id}/refund
Authorization: Bearer {adminToken}
Body: { "refundAmount": 80000, "reason": "Customer request" }
```

```
GET /api/admin/reports/revenue?fromDate=2026-01-01&toDate=2026-01-31&period=daily
GET /api/admin/reports/bookings?fromDate=2026-01-01&toDate=2026-01-31
GET /api/admin/settings
```

### User: Favorites

```
GET /api/parking-lots/favorites?page=1&pageSize=10
Authorization: Bearer {token}

Response:
{
  "items": [{
    "favoriteId": "...",
    "parkingLotId": "...",
    "parkingLotName": "Bãi xe ABC",
    "parkingLotAddress": "123 Nguyen Hue",
    "pricePerHour": 20000,
    "averageRating": 4.5,
    "reviewCount": 25,
    "favoritedAt": "..."
  }]
}
```

```
POST /api/parking-lots/{parkingLotId}/favorite
Authorization: Bearer {token}
```

```
DELETE /api/parking-lots/{parkingLotId}/favorite
Authorization: Bearer {token}
```

```
GET /api/parking-lots/{parkingLotId}/favorite/check
Authorization: Bearer {token}
Response: { "data": true/false }
```

### User & Public: Reviews

```
GET /api/parking-lots/{parkingLotId}/reviews?page=1&pageSize=10
(Public - no auth required)

Response:
{
  "items": [{
    "reviewId": "...",
    "userName": "Nguyen Van A",
    "rating": 5,
    "comment": "Great parking lot!",
    "createdAt": "..."
  }]
}
```

```
GET /api/parking-lots/{parkingLotId}/reviews/summary
(Public)

Response:
{
  "parkingLotName": "Bãi xe ABC",
  "averageRating": 4.5,
  "totalReviews": 100,
  "fiveStarCount": 60,
  "fourStarCount": 25,
  "threeStarCount": 10,
  "twoStarCount": 3,
  "oneStarCount": 2
}
```

```
POST /api/parking-lots/{parkingLotId}/reviews
Authorization: Bearer {token}
Body: {
  "bookingId": "...",
  "rating": 5,
  "comment": "Great experience!"
}
```

```
PUT /api/parking-lots/reviews/{reviewId}
Authorization: Bearer {token}
Body: { "rating": 4, "comment": "Updated comment" }
```

```
DELETE /api/parking-lots/reviews/{reviewId}
Authorization: Bearer {token}
```

```
GET /api/users/reviews?page=1&pageSize=10
Authorization: Bearer {token}
(Get my reviews)
```

### User: Notifications

```
GET /api/notifications?isRead=false&page=1&pageSize=20
Authorization: Bearer {token}

Response:
{
  "items": [{
    "notificationId": "...",
    "title": "Booking Confirmed",
    "message": "Your booking at ABC parking lot is confirmed",
    "type": "Booking",
    "isRead": false,
    "createdAt": "..."
  }]
}
```

```
GET /api/notifications/summary
Authorization: Bearer {token}

Response:
{
  "totalCount": 50,
  "unreadCount": 5,
  "recentNotifications": [...]
}
```

```
PATCH /api/notifications/{id}/read
Authorization: Bearer {token}
```

```
PATCH /api/notifications/read-all
Authorization: Bearer {token}
```

### Admin: Review Moderation

```
GET /api/admin/reviews?parkingLotId={id}&rating=1&page=1&pageSize=20
Authorization: Bearer {adminToken}
```

```
DELETE /api/admin/reviews/{reviewId}
Authorization: Bearer {adminToken}
```

### Admin: Broadcast Notifications

```
POST /api/admin/notifications/send
Authorization: Bearer {adminToken}
Body: {
  "userId": "...",
  "title": "Important Notice",
  "message": "Your subscription is expiring soon",
  "type": "Warning"
}
```

```
POST /api/admin/notifications/broadcast
Authorization: Bearer {adminToken}
Body: {
  "title": "System Maintenance",
  "message": "System will be under maintenance on 2026-01-30",
  "type": "System"
}

Response: { "data": 1500, "message": "Notification broadcasted to 1500 users" }
```

---

## Appendix C: Complete Endpoints List (v3.0)

### User Endpoints (Authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/users/profile | Get my profile |
| PUT | /api/users/profile | Update my profile |
| GET | /api/users/reviews | Get my reviews |
| GET | /api/parking-lots/favorites | Get my favorites |
| POST | /api/parking-lots/{id}/favorite | Add to favorites |
| DELETE | /api/parking-lots/{id}/favorite | Remove from favorites |
| GET | /api/notifications | Get my notifications |
| GET | /api/notifications/summary | Get notification summary |
| PATCH | /api/notifications/{id}/read | Mark as read |
| PATCH | /api/notifications/read-all | Mark all as read |

### Public Endpoints (No Auth)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/parking-lots/{id}/reviews | Get parking lot reviews |
| GET | /api/parking-lots/{id}/reviews/summary | Get reviews summary |

### Owner Endpoints (Owner Role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/owners/dashboard | Owner dashboard stats |
| GET | /api/owners/earnings | Earnings summary |
| GET | /api/owners/earnings/history | Earnings history |
| GET | /api/owners/payouts | Payout history |
| GET | /api/owners/subscription | Subscription status |
| POST | /api/owners/subscription/renew | Renew subscription |
| GET | /api/owners/parking-lots/{id}/statistics | Lot statistics |

### Admin Endpoints (Admin Role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/admin/transactions | List transactions |
| GET | /api/admin/transactions/{id} | Get transaction |
| POST | /api/admin/transactions/{id}/refund | Process refund |
| GET | /api/admin/reports/revenue | Revenue report |
| GET | /api/admin/reports/bookings | Bookings report |
| GET | /api/admin/settings | Get settings |
| PUT | /api/admin/settings | Update settings |
| GET | /api/admin/reviews | List all reviews |
| DELETE | /api/admin/reviews/{id} | Delete review |
| POST | /api/admin/notifications/send | Send notification |
| POST | /api/admin/notifications/broadcast | Broadcast to all |

---

**Document Version:** 3.0  
**Last Updated:** 2026-01-25  
**Author:** SmartParking Backend Team
