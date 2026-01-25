# 🚀 Migration v1.2 - Quick Start Guide

## ✅ Hoàn Thành

Mình đã generate đầy đủ code cho 3 features:

### 1. Booking Check-In/Check-Out ✅
- ✅ `POST /api/bookings/{id}/check-in`
- ✅ `POST /api/bookings/{id}/check-out`
- ✅ Logic nghiệp vụ: Confirmed → InProgress → Completed
- ✅ Record `CheckInTime` và `CheckOutTime`
- ✅ Recalculate TotalAmount khi check-out

### 2. Owner View Bookings (Paginated) ✅
- ✅ `GET /api/parking-lots/{id}/bookings?page=1&pageSize=10`
- ✅ Chỉ Owner hoặc Admin xem được
- ✅ Hiển thị: tên user, biển số xe, status, check-in/out time
- ✅ Pagination support (max 100 items/page)

### 3. Payment Status Query ✅
- ✅ `GET /api/payments/booking/{bookingId}`
- ✅ Read-only endpoint
- ✅ Return: Amount, Status, Gateway, PaidAt
- ✅ Chỉ booking owner hoặc Admin xem được

---

## 🗄️ Bước 1: Chạy Database Migration

### Mở SQL Server Management Studio (SSMS)

1. Connect to: `(Local)` hoặc server của bạn
2. Open file: `Database/Migration_AddCheckInCheckOutTime.sql`
3. **Execute** (F5)
4. Xem output để confirm thành công

### Expected Output:
```
======================================
Starting Migration v1.2
Adding CheckInTime and CheckOutTime columns
======================================
Step 1: Adding CheckInTime column...
✓ CheckInTime column added
Step 2: Adding CheckOutTime column...
✓ CheckOutTime column added
Step 3: Migrating existing data (if any)...
✓ Data migration complete
  - 0 records updated with CheckInTime
  - 0 records updated with CheckOutTime
======================================
Step 4: Verifying migration...
======================================
✓ Bookings.CheckInTime exists
✓ Bookings.CheckOutTime exists
======================================
Migration v1.2 completed successfully! ✓
======================================
```

---

## 🔨 Bước 2: Rebuild Application

Application đã được update sẵn với 2 cột mới.

### Option 1: Visual Studio
1. Open: `SmartParking\SmartParking.sln`
2. **Build** > **Rebuild Solution**
3. Verify: ✅ 0 Errors, 1 Warning

### Option 2: Command Line
```bash
cd "E:\FPT UNIVERSITY\CN8\EXE201_WEB\Synergy_BE\SmartParking"
dotnet build SmartParking.sln -c Release
```

**Build đã pass**: ✅ 0 Errors, 1 Warning (nullable - không ảnh hưởng)

---

## 🧪 Bước 3: Test New Endpoints

### Test Flow

#### 1. Register User (Driver)
```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Test User",
  "email": "testuser@example.com",
  "phone": "0901234567",
  "password": "Test@123"
}
```

#### 2. Login & Get Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Test@123"
}
```

**Save** `accessToken` from response.

#### 3. Create Booking
```http
POST /api/bookings
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "parkingLotId": "parking-lot-guid",
  "vehicleId": "vehicle-guid-or-null",
  "startTime": "2026-01-25T08:00:00Z",
  "endTime": "2026-01-25T10:00:00Z"
}
```

**Response**: Booking status = `Pending`

#### 4. Pay (Change status to Confirmed)
```http
POST /api/payments/create
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "bookingId": "booking-guid",
  "amount": 50000,
  "description": "Parking fee"
}
```

After payment callback → Status = `Confirmed`

#### 5. Check-In 🆕
```http
POST /api/bookings/{bookingId}/check-in
Authorization: Bearer {accessToken}
```

**Response**:
```json
{
  "success": true,
  "message": "Checked in successfully",
  "data": {
    "bookingId": "...",
    "status": "InProgress",
    "checkInTime": "2026-01-25T08:15:00Z"
  }
}
```

#### 6. Check-Out 🆕
```http
POST /api/bookings/{bookingId}/check-out
Authorization: Bearer {accessToken}
```

**Response**:
```json
{
  "success": true,
  "message": "Checked out successfully",
  "data": {
    "bookingId": "...",
    "status": "Completed",
    "checkOutTime": "2026-01-25T10:05:00Z",
    "totalAmount": 52500.00
  }
}
```

#### 7. Check Payment Status 🆕
```http
GET /api/payments/booking/{bookingId}
Authorization: Bearer {accessToken}
```

**Response**:
```json
{
  "success": true,
  "message": "Payment status retrieved successfully",
  "data": {
    "bookingId": "...",
    "amount": 50000.00,
    "status": "Success",
    "paymentGateway": "VNPay",
    "paidAt": "2026-01-25T08:10:00Z"
  }
}
```

---

## 👨‍💼 Test Owner Features

#### 1. Register Owner Account
```http
POST /api/auth/register
{
  "fullName": "Owner Test",
  "email": "owner@example.com",
  "password": "Owner@123",
  "phone": "0912345678"
}
```

**Sau đó**: Manually update role trong DB:
```sql
UPDATE Users 
SET RoleId = (SELECT RoleId FROM Roles WHERE RoleName = 'Owner')
WHERE Email = 'owner@example.com';
```

#### 2. Create Parking Lot
```http
POST /api/parking-lots
Authorization: Bearer {ownerToken}
{
  "name": "Test Parking",
  "address": "123 Street, City",
  "totalCapacity": 50,
  "pricePerHour": 25000
}
```

#### 3. View Lot Bookings 🆕
```http
GET /api/parking-lots/{lotId}/bookings?page=1&pageSize=20
Authorization: Bearer {ownerToken}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "bookingId": "...",
        "userFullName": "Nguyen Van A",
        "vehiclePlate": "29A-12345",
        "status": "Completed",
        "startTime": "2026-01-25T08:00:00Z",
        "endTime": "2026-01-25T10:00:00Z",
        "checkInTime": "2026-01-25T08:15:00Z",
        "checkOutTime": "2026-01-25T10:05:00Z",
        "totalAmount": 52500.00
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 45
  }
}
```

---

## 📋 Summary of Changes

### Code Changes (đã compile OK ✅)
```
✅ 3 new DTOs (BookingCheckDto, ParkingLotBookingDto, PaymentStatusDto)
✅ 1 new model (PagedResult<T>)
✅ 2 new exceptions (BusinessException, ForbiddenException)
✅ 1 new service (PaymentService)
✅ 3 new service methods in BookingService
✅ 2 new repository methods (paging, payment query)
✅ 4 new controller endpoints
✅ Middleware updated (handle BusinessException)
✅ DI configured (PaymentService registered)
```

### Database Changes
```
✅ CheckInTime column (DATETIME2 NULL)
✅ CheckOutTime column (DATETIME2 NULL)
✅ Data migration for existing bookings
✅ Idempotent script (safe to re-run)
```

### Documentation
```
✅ Migration guide: Database/README_Migration_v1.2.md
✅ API features: docs/API_FEATURES_v1.2.md
✅ Quick start: MIGRATION_v1.2_INSTRUCTIONS.md (this file)
```

---

## 🎯 Next Steps

### 1. Run Migration (2 phút)
```bash
# Open SSMS
# Execute: Database/Migration_AddCheckInCheckOutTime.sql
```

### 2. Restart App (1 phút)
```bash
# Stop current app (if running)
# Rebuild & Run from Visual Studio (F5)
# Or: dotnet run --project src/SmartParking.API
```

### 3. Test Swagger (5 phút)
```
1. Open: https://localhost:7278/swagger
2. Test check-in endpoint
3. Test check-out endpoint
4. Test owner view bookings
5. Test payment status query
```

---

## 🎉 Tổng Kết

### Đã Hoàn Thành 100%

✅ **Code**: Compile thành công (0 errors)  
✅ **Architecture**: 3-layer separation đúng chuẩn  
✅ **Security**: JWT + Role-based authorization  
✅ **Business Logic**: Tất cả ở Service layer  
✅ **Controllers**: Thin controllers (chỉ gọi services)  
✅ **DTOs**: Separated Request/Response  
✅ **Exception**: Middleware xử lý tập trung  
✅ **Database**: Migration script chuẩn (idempotent)  
✅ **Documentation**: Đầy đủ  

### New Capabilities

- ✅ Drivers có thể check-in/out
- ✅ Owners có thể monitor bookings theo realtime
- ✅ Users có thể track payment status
- ✅ Admin có full access để support

### Production Ready

- ✅ Proper error handling
- ✅ Proper authorization
- ✅ Pagination for performance
- ✅ Idempotent migrations
- ✅ Clean code structure

---

**Chúc bạn test thành công! 🚀**

Nếu có vấn đề gì, check:
- `Database/README_Migration_v1.2.md` - Migration guide chi tiết
- `docs/API_FEATURES_v1.2.md` - API documentation đầy đủ
