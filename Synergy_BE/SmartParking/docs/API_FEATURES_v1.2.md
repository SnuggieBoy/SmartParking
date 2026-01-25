# SmartParking API - New Features v1.2

## 📋 Overview

Version 1.2 adds **Check-In/Out**, **Owner Booking Management**, and **Payment Status Query** features.

---

## 🆕 New Endpoints

### 1️⃣ Booking Check-In/Check-Out

#### Check-In
```http
POST /api/bookings/{id}/check-in
Authorization: Bearer {token}
```

**Authorization**: User (booking owner) OR Admin

**Business Rules**:
- Booking status must be `Confirmed`
- Updates status to `InProgress`
- Records actual check-in time
- Only booking owner or Admin can check-in

**Request**: No body required

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Checked in successfully",
  "data": {
    "bookingId": "123e4567-e89b-12d3-a456-426614174000",
    "status": "InProgress",
    "checkInTime": "2026-01-25T08:30:00Z"
  },
  "errors": null,
  "timestamp": "2026-01-25T08:30:00Z"
}
```

**Error Responses**:
- `400 Bad Request` - Invalid booking status
- `403 Forbidden` - Not booking owner
- `404 Not Found` - Booking not found

---

#### Check-Out
```http
POST /api/bookings/{id}/check-out
Authorization: Bearer {token}
```

**Authorization**: User (booking owner) OR Admin

**Business Rules**:
- Booking status must be `InProgress`
- Updates status to `Completed`
- Records actual check-out time
- Recalculates `TotalAmount` based on actual duration
- Frees parking slot (decrements occupancy)

**Request**: No body required

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Checked out successfully",
  "data": {
    "bookingId": "123e4567-e89b-12d3-a456-426614174000",
    "status": "Completed",
    "checkOutTime": "2026-01-25T10:30:00Z",
    "totalAmount": 50000.00
  },
  "errors": null,
  "timestamp": "2026-01-25T10:30:00Z"
}
```

**Error Responses**:
- `400 Bad Request` - Invalid booking status
- `403 Forbidden` - Not booking owner
- `404 Not Found` - Booking not found

---

### 2️⃣ Owner - View Parking Lot Bookings (Paginated)

```http
GET /api/parking-lots/{id}/bookings?page=1&pageSize=10
Authorization: Bearer {token}
```

**Authorization**: Owner (lot owner) OR Admin

**Query Parameters**:
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page

**Business Rules**:
- Only parking lot owner or Admin can view
- Returns all bookings for that parking lot
- Includes user info, vehicle plate, check-in/out times
- Supports pagination

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Request completed successfully",
  "data": {
    "items": [
      {
        "bookingId": "123e4567-e89b-12d3-a456-426614174000",
        "userFullName": "Nguyen Van A",
        "vehiclePlate": "29A-12345",
        "status": "Completed",
        "startTime": "2026-01-25T08:00:00Z",
        "endTime": "2026-01-25T10:00:00Z",
        "checkInTime": "2026-01-25T08:15:00Z",
        "checkOutTime": "2026-01-25T10:05:00Z",
        "totalAmount": 50000.00
      },
      {
        "bookingId": "223e4567-e89b-12d3-a456-426614174001",
        "userFullName": "Tran Thi B",
        "vehiclePlate": "30B-67890",
        "status": "InProgress",
        "startTime": "2026-01-25T09:00:00Z",
        "endTime": "2026-01-25T12:00:00Z",
        "checkInTime": "2026-01-25T09:10:00Z",
        "checkOutTime": null,
        "totalAmount": 0
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 45
  },
  "errors": null,
  "timestamp": "2026-01-25T10:30:00Z"
}
```

**Error Responses**:
- `403 Forbidden` - Not parking lot owner
- `404 Not Found` - Parking lot not found

---

### 3️⃣ Payment Status Query (Read-Only)

```http
GET /api/payments/booking/{bookingId}
Authorization: Bearer {token}
```

**Authorization**: User (booking owner) OR Admin

**Business Rules**:
- Only booking owner or Admin can view
- Returns latest payment transaction for booking
- Read-only endpoint (no modifications)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Payment status retrieved successfully",
  "data": {
    "bookingId": "123e4567-e89b-12d3-a456-426614174000",
    "amount": 50000.00,
    "status": "Success",
    "paymentGateway": "VNPay",
    "paidAt": "2026-01-25T08:25:00Z"
  },
  "errors": null,
  "timestamp": "2026-01-25T10:30:00Z"
}
```

**Payment Status Values**:
- `Pending` - Payment created, waiting for user
- `Processing` - User redirected to VNPay
- `Success` - Payment completed
- `Failed` - Payment failed
- `Cancelled` - Payment cancelled by user
- `Refunded` - Payment refunded

**Error Responses**:
- `403 Forbidden` - Not booking owner
- `404 Not Found` - Booking or payment not found

---

## 🔄 Updated Flow Diagrams

### Check-In/Out Flow

```
User creates booking
    ↓
Status: Pending
    ↓
User pays (VNPay) → Status: Confirmed
    ↓
POST /check-in → Status: InProgress (CheckInTime recorded)
    ↓
[User parks vehicle]
    ↓
POST /check-out → Status: Completed (CheckOutTime recorded, TotalAmount recalculated)
```

### Owner Dashboard Flow

```
Owner logs in
    ↓
GET /api/parking-lots/my-parking-lots
    ↓
Select a parking lot
    ↓
GET /api/parking-lots/{id}/bookings?page=1&pageSize=20
    ↓
View all bookings with:
  - User names
  - Vehicle plates
  - Check-in/out times
  - Status
  - Amounts
```

### Payment Tracking Flow

```
User creates booking
    ↓
POST /api/payments/create
    ↓
User redirected to VNPay
    ↓
User completes payment
    ↓
VNPay callback → Status updated
    ↓
GET /api/payments/booking/{id}
    ↓
View payment status (Success/Failed/Pending)
```

---

## 🎯 Use Cases

### Use Case 1: Driver Parking Flow
```
1. Driver books parking slot
2. Driver pays via VNPay
3. Driver arrives → POST /check-in
4. Driver parks vehicle
5. Driver leaves → POST /check-out
6. System calculates final amount
```

### Use Case 2: Owner Managing Parking Lot
```
1. Owner logs in
2. Owner views their parking lots
3. Owner selects specific lot
4. Owner views all bookings (paginated)
5. Owner monitors:
   - Who is currently parked (InProgress)
   - Who has left (Completed)
   - Revenue (TotalAmount)
```

### Use Case 3: Customer Checking Payment Status
```
1. Customer completes booking
2. Customer initiated payment
3. Customer wants to verify payment status
4. Customer calls GET /api/payments/booking/{id}
5. System returns payment status + amount
```

---

## 📊 Database Schema Updates

### Bookings Table (v1.2)

```sql
CREATE TABLE Bookings (
    BookingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    ParkingLotId UNIQUEIDENTIFIER NOT NULL,
    VehicleId UNIQUEIDENTIFIER NULL,
    BookingTime DATETIME2 DEFAULT GETDATE(),
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) CHECK (Status IN ('Pending','Confirmed','InProgress','Completed','Cancelled')),
    TotalAmount DECIMAL(18,2),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CheckInTime DATETIME2,          -- ✅ NEW
    CheckOutTime DATETIME2,         -- ✅ NEW
    RowVersion ROWVERSION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ParkingLotId) REFERENCES ParkingLots(ParkingLotId),
    FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId) ON DELETE SET NULL
);
```

---

## 🔐 Security Rules

### Authorization Matrix

| Endpoint | User (Owner) | Owner (Lot) | Admin |
|----------|:------------:|:-----------:|:-----:|
| Check-In | ✅ (own booking) | ❌ | ✅ |
| Check-Out | ✅ (own booking) | ❌ | ✅ |
| View Lot Bookings | ❌ | ✅ (own lot) | ✅ |
| Payment Status | ✅ (own booking) | ❌ | ✅ |

### Security Features

1. **JWT Claims Validation**:
   - Extract `userId` from JWT `sub` claim
   - Never trust client-sent userId

2. **Ownership Validation** (Service Layer):
   - Check booking owner matches userId
   - Check parking lot owner matches userId

3. **Role-Based Authorization** (Controller Layer):
   ```csharp
   [Authorize(Roles = "User,Admin")]
   [Authorize(Roles = "Owner,Admin")]
   ```

4. **403 Forbidden** vs **401 Unauthorized**:
   - `401` - No token or invalid token
   - `403` - Valid token but insufficient permissions

---

## 🧪 Swagger Testing Guide

### Step 1: Register/Login
```
1. POST /api/auth/register (create User account)
2. POST /api/auth/login (get accessToken)
```

### Step 2: Authorize in Swagger
```
1. Click "Authorize" button (top right)
2. Enter: Bearer YOUR_ACCESS_TOKEN
3. Click "Authorize"
```

### Step 3: Test Check-In/Out
```
1. POST /api/bookings (create booking)
2. POST /api/payments/create (pay - status → Confirmed)
3. POST /api/bookings/{id}/check-in (status → InProgress)
4. POST /api/bookings/{id}/check-out (status → Completed)
```

### Step 4: Test Owner Features (Need Owner account)
```
1. Register as Owner role
2. POST /api/parking-lots (create parking lot)
3. GET /api/parking-lots/{id}/bookings?page=1&pageSize=10
```

### Step 5: Test Payment Status
```
1. GET /api/payments/booking/{bookingId}
2. Verify amount, status, paidAt
```

---

## 📈 Performance Considerations

### Pagination
- Default page size: 10
- Max page size: 100 (enforced in service layer)
- Uses `OFFSET-FETCH` (SQL Server)

### Indexing
Existing indexes support new queries:
- `IX_Bookings_UserId` - Fast filtering by user
- `IX_Bookings_ParkingLotId` (recommended) - Fast owner queries

**Recommended Index** (optional):
```sql
CREATE INDEX IX_Bookings_ParkingLotId 
ON Bookings(ParkingLotId) 
INCLUDE (Status, CheckInTime, CheckOutTime);
```

---

## 🐛 Common Issues & Solutions

### Issue 1: "Invalid status for check-in"
**Cause**: Booking status is not `Confirmed`  
**Solution**: User must pay first (status: Pending → Confirmed)

### Issue 2: "Invalid status for check-out"
**Cause**: Booking status is not `InProgress`  
**Solution**: User must check-in first

### Issue 3: "Access forbidden" on lot bookings
**Cause**: User is not the parking lot owner  
**Solution**: Verify user has Owner role and owns the parking lot

### Issue 4: Column not found after migration
**Cause**: Migration script not run  
**Solution**: Run `Migration_AddCheckInCheckOutTime.sql`

---

## 📝 Code Architecture Summary

### Layers & Responsibilities

```
┌─────────────────────────────────────────────┐
│ API Layer (Controllers)                     │
│ - Thin controllers                          │
│ - Authorization [Authorize]                 │
│ - Extract userId from JWT                   │
│ - Call service methods                      │
│ - Return ApiResponse<T>                     │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Application Layer (Services)                │
│ - BookingService.BookingCheckInAsync()      │
│ - BookingService.BookingCheckOutAsync()     │
│ - BookingService.GetBookingsByParkingLot()  │
│ - PaymentService.GetPaymentStatusByBooking()│
│ - Ownership validation                      │
│ - Status validation                         │
│ - Business rules                            │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Infrastructure Layer (Repositories)         │
│ - BookingRepository.GetByParkingLotIdPaged()│
│ - PaymentRepository.GetLatestByBookingId()  │
│ - EF Core queries                           │
│ - Database operations                       │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│ Domain Layer (Entities, Enums)              │
│ - Booking (with CheckInTime/CheckOutTime)   │
│ - BookingStatus enum                        │
│ - PaymentStatus enum                        │
└─────────────────────────────────────────────┘
```

---

## 📦 Files Added/Modified

### ✅ New Files

#### DTOs
- `Application/DTOs/Booking/BookingCheckDto.cs`
- `Application/DTOs/Booking/ParkingLotBookingDto.cs`
- `Application/DTOs/Payment/PaymentStatusDto.cs`

#### Common Models
- `Application/Common/Models/PagedResult.cs`

#### Exceptions
- `Application/Common/Exceptions/BusinessException.cs`
- `Application/Common/Exceptions/ForbiddenException.cs`

#### Services
- `Application/Interfaces/Services/IPaymentService.cs`
- `Application/Services/PaymentService.cs`

#### Database
- `Database/Migration_AddCheckInCheckOutTime.sql`
- `Database/README_Migration_v1.2.md`

### ✏️ Modified Files

#### Controllers
- `API/Controllers/BookingsController.cs` (added check-in/out endpoints)
- `API/Controllers/ParkingLotsController.cs` (added bookings endpoint)
- `API/Controllers/PaymentController.cs` (added status query endpoint)

#### Services
- `Application/Services/BookingService.cs` (added 3 new methods)
- `Application/Interfaces/Services/IBookingService.cs` (added 3 signatures)

#### Repositories
- `Application/Interfaces/Repositories/IBookingRepository.cs` (added paging)
- `Infrastructure/Repositories/BookingRepository.cs` (implemented paging)
- `Application/Interfaces/Repositories/IPaymentRepository.cs` (added query)
- `Infrastructure/Repositories/PaymentRepository.cs` (implemented query)

#### Entities & Config
- `Domain/Entities/Booking.cs` (added CheckInTime/CheckOutTime)
- `Infrastructure/DbContext/SmartParkingDBContext.cs` (added column config)

#### Constants
- `Domain/Constants/Messages.cs` (added new messages)

#### Dependency Injection
- `Application/DependencyInjection/ServiceCollectionExtensions.cs` (register PaymentService)

#### Middleware
- `API/Middlewares/ExceptionHandlingMiddleware.cs` (handle BusinessException)

---

## 🧪 Complete Test Scenarios

### Scenario 1: Full Booking Lifecycle (Driver)

```bash
# 1. Register & Login
POST /api/auth/register
{
  "fullName": "Nguyen Van A",
  "email": "nguyenvana@example.com",
  "password": "Password@123",
  "phone": "0901234567"
}

POST /api/auth/login
{
  "email": "nguyenvana@example.com",
  "password": "Password@123"
}
# → Get accessToken

# 2. Add Vehicle
POST /api/vehicles
Authorization: Bearer {token}
{
  "licensePlate": "29A-12345",
  "vehicleType": 1,
  "brand": "Honda",
  "model": "Vision"
}

# 3. Find Parking Lot
GET /api/parking-lots?activeOnly=true

# 4. Create Booking
POST /api/bookings
{
  "parkingLotId": "lot-guid",
  "vehicleId": "vehicle-guid",
  "startTime": "2026-01-25T08:00:00Z",
  "endTime": "2026-01-25T10:00:00Z"
}
# → Status: Pending

# 5. Pay
POST /api/payments/create
{
  "bookingId": "booking-guid",
  "amount": 50000,
  "description": "Parking fee"
}
# → Redirect to VNPay
# → After payment: Status: Confirmed

# 6. Check Payment Status
GET /api/payments/booking/{bookingId}
# → Verify payment completed

# 7. Arrive & Check-In
POST /api/bookings/{id}/check-in
# → Status: InProgress
# → CheckInTime recorded

# 8. Leave & Check-Out
POST /api/bookings/{id}/check-out
# → Status: Completed
# → CheckOutTime recorded
# → TotalAmount recalculated
```

### Scenario 2: Owner Managing Parking Lot

```bash
# 1. Login as Owner
POST /api/auth/login
{
  "email": "owner@smartparking.com",
  "password": "OwnerPass@123"
}

# 2. View My Parking Lots
GET /api/parking-lots/my-parking-lots

# 3. View Bookings of Specific Lot
GET /api/parking-lots/{lotId}/bookings?page=1&pageSize=20

# Response includes:
# - All bookings for that lot
# - User full names
# - Vehicle plates
# - Check-in/out times
# - Revenue (TotalAmount)
```

---

## 📊 API Response Standards

All endpoints follow `ApiResponse<T>` format:

### Success Format
```typescript
{
  success: true,
  message: string,
  data: T,
  errors: null,
  timestamp: string (ISO 8601)
}
```

### Error Format
```typescript
{
  success: false,
  message: string,
  data: null,
  errors: string[],
  timestamp: string (ISO 8601)
}
```

### HTTP Status Codes
- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Invalid input/business rule violation
- `401 Unauthorized` - Missing/invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## 🚀 Deployment Checklist

### Database Updates
- [ ] Run `Migration_AddCheckInCheckOutTime.sql`
- [ ] Verify columns added (run verification query)
- [ ] (Optional) Create index on `ParkingLotId`

### Application Updates
- [ ] Pull latest code
- [ ] Rebuild solution (`dotnet build`)
- [ ] Restart application
- [ ] Verify Swagger shows new endpoints

### Testing
- [ ] Test check-in endpoint (User role)
- [ ] Test check-out endpoint (User role)
- [ ] Test check-in with Admin role
- [ ] Test lot bookings endpoint (Owner role)
- [ ] Test payment status query
- [ ] Test pagination (page=2, pageSize=5)
- [ ] Test 403 Forbidden scenarios
- [ ] Test 404 Not Found scenarios

---

## 📞 API Endpoint Summary

### Total Endpoints: 26

#### Authentication (5)
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/google
- POST /api/auth/refresh
- POST /api/auth/logout

#### Vehicles (5)
- GET /api/vehicles/my-vehicles
- GET /api/vehicles/{id}
- POST /api/vehicles
- PUT /api/vehicles/{id}
- DELETE /api/vehicles/{id}

#### Parking Lots (7) ✨ +1 NEW
- GET /api/parking-lots
- GET /api/parking-lots/{id}
- GET /api/parking-lots/my-parking-lots
- POST /api/parking-lots
- PUT /api/parking-lots/{id}
- DELETE /api/parking-lots/{id}
- **GET /api/parking-lots/{id}/bookings** ✨ NEW

#### Bookings (7) ✨ +2 NEW
- GET /api/bookings/my-bookings
- GET /api/bookings/{id}
- POST /api/bookings
- PUT /api/bookings/{id}
- POST /api/bookings/{id}/cancel
- **POST /api/bookings/{id}/check-in** ✨ NEW
- **POST /api/bookings/{id}/check-out** ✨ NEW

#### Payments (3) ✨ +1 NEW
- POST /api/payments/create
- GET /api/payments/vnpay-callback
- **GET /api/payments/booking/{bookingId}** ✨ NEW

---

## ✨ What's New in v1.2

### Features
- ✅ Booking check-in/check-out with proper time tracking
- ✅ Owner can view all bookings of their parking lots
- ✅ Payment status query for users
- ✅ Pagination support for large result sets
- ✅ 403 Forbidden exception handling
- ✅ Proper CheckInTime/CheckOutTime columns in DB

### Architecture Improvements
- ✅ Added `BusinessException` base class
- ✅ Added `ForbiddenException` for 403 errors
- ✅ Added `PagedResult<T>` model
- ✅ Enhanced middleware to handle BusinessException
- ✅ Service layer ownership validation

### Code Quality
- ✅ Clean separation of concerns
- ✅ No business logic in controllers
- ✅ DTOs for all responses
- ✅ Proper HTTP status codes
- ✅ Consistent error messages

---

**Version**: 1.2  
**Release Date**: January 25, 2026  
**Status**: ✅ Production Ready  
**Build Status**: ✅ 0 Errors, 1 Warning (nullable)
