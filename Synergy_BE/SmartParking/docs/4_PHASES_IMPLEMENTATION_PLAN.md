# 🏗️ SMARTPARKING - 4-PHASE IMPLEMENTATION PLAN

**Date:** 2026-01-26  
**Status:** In Progress  
**Architecture:** Clean Architecture (Controller → Service → Repository)

---

## 📊 **PROGRESS OVERVIEW**

```
✅ PHASE 0: Authentication & Authorization (COMPLETED 100%)
🔄 PHASE 1: Parking Lot Management (IN PROGRESS - 2/7 completed)
⏳ PHASE 2: Booking System (PENDING)
⏳ PHASE 3: Payment Integration (PENDING)
⏳ PHASE 4: Map & Location Services (PENDING)
```

---

## ✅ **PHASE 0: AUTHENTICATION & AUTHORIZATION (COMPLETED)**

### **Implemented:**
- ✅ OTP-based Registration (register-request → verify-otp)
- ✅ Email Verification (mandatory, EmailConfirmed flag)
- ✅ Local Login (email + password)
- ✅ Google OAuth Login
- ✅ JWT Token Management (Access + Refresh tokens)
- ✅ Password Reset with OTP (forgot-password → reset-password)
- ✅ Email Service (SMTP with HTML templates)
- ✅ Role-based Authorization (User, Owner, Admin)
- ✅ Policy-based Authorization
- ✅ Security Features (rate limiting, token revocation, etc.)

### **Endpoints (9 total):**
1. `POST /api/auth/register-request` - Send registration OTP
2. `POST /api/auth/verify-otp` - Complete registration
3. `POST /api/auth/resend-otp` - Resend OTP
4. `POST /api/auth/login` - Local login
5. `POST /api/auth/google` - Google OAuth
6. `POST /api/auth/forgot-password` - Send password reset OTP
7. `POST /api/auth/reset-password` - Reset password
8. `POST /api/auth/refresh` - Refresh token
9. `POST /api/auth/logout` - Logout

---

## 🔄 **PHASE 1: PARKING LOT MANAGEMENT**

### **Objective:**
Complete CRUD operations for parking lot management with proper authorization and soft delete support.

### **Database Changes:**
```sql
-- File: Migration_Phase1_ParkingLotEnhancements.sql

ALTER TABLE ParkingLots ADD:
- IsActive BIT (default 1)
- UpdatedAt DATETIME2(7)
- CreatedBy UNIQUEIDENTIFIER
- UpdatedBy UNIQUEIDENTIFIER
- IsDeleted BIT (default 0)
- DeletedAt DATETIME2(7)
- DeletedBy UNIQUEIDENTIFIER

Indexes:
- IX_ParkingLots_IsActive
- IX_ParkingLots_IsDeleted
- IX_ParkingLots_OwnerId_IsDeleted
```

### **Entities:**
```csharp
ParkingLot:
- Added audit fields (CreatedBy, UpdatedBy, UpdatedAt)
- Added soft delete (IsDeleted, DeletedAt, DeletedBy)
- Added IsActive flag
- Changed nullable to non-nullable where appropriate
```

### **DTOs:**
```csharp
1. ParkingLotResponseDto - Full details with owner info
2. CreateParkingLotDto - Create request with validation
3. UpdateParkingLotDto - Update request
4. ParkingLotFilterDto - Search/filter parameters
```

### **Business Rules:**
1. Only Owner or Admin can create parking lots
2. Only parking lot owner or Admin can update/delete
3. Soft delete instead of hard delete
4. Auto-populate audit fields (CreatedBy, UpdatedBy, timestamps)
5. Status validation: "Active", "Inactive", "Maintenance"
6. Capacity validation: CurrentOccupancy ≤ TotalCapacity
7. Price validation: > 0

### **APIs (7 endpoints):**
```
GET    /api/parking-lots                [Public]        List all (paginated, filtered)
GET    /api/parking-lots/{id}           [Public]        Get by ID
GET    /api/parking-lots/my             [Owner, Admin]  Get my parking lots
POST   /api/parking-lots                [Owner, Admin]  Create
PUT    /api/parking-lots/{id}           [Owner, Admin]  Update
DELETE /api/parking-lots/{id}           [Owner, Admin]  Soft delete
PATCH  /api/parking-lots/{id}/activate  [Owner, Admin]  Toggle IsActive
```

### **Current Status:**
- ✅ Entity enhanced
- ✅ SQL migration created
- 🔄 DTOs created (need to complete)
- ⏳ Repository (needs enhancement for pagination)
- ⏳ Service (needs soft delete logic)
- ⏳ Controller (needs new endpoints)
- ⏳ DbContext configuration

---

## ⏳ **PHASE 2: BOOKING SYSTEM**

### **Objective:**
Full booking lifecycle with slot management, check-in/check-out, and concurrency control.

### **Database Changes:**
```sql
ALTER TABLE Bookings ADD:
- VehicleId UNIQUEIDENTIFIER (FK to Vehicles)
- TotalAmount DECIMAL(18,2) (calculated field)
- CheckInTime DATETIME2(7) (actual check-in)
- CheckOutTime DATETIME2(7) (actual check-out)
- IsDeleted BIT
- CreatedBy, UpdatedBy, DeletedBy

Status values:
- Pending → Confirmed → CheckedIn → CheckedOut → Completed
- Cancelled (any time before CheckedIn)
```

### **Business Rules:**
1. Check slot availability before booking (atomic operation)
2. Prevent double-booking (concurrency control with RowVersion)
3. Auto-calculate TotalAmount based on duration
4. Check-in within 30 minutes of StartTime
5. Auto-cancel if no-show after 30 minutes
6. Update ParkingLot.CurrentOccupancy on check-in/check-out
7. Cannot cancel after check-in

### **APIs (7+ endpoints):**
```
POST   /api/bookings                    [User]          Create booking
GET    /api/bookings/my                 [User]          My bookings (paginated)
GET    /api/bookings/{id}               [User, Owner]   Get details
POST   /api/bookings/{id}/check-in      [User]          Check-in
POST   /api/bookings/{id}/check-out     [User]          Check-out
PUT    /api/bookings/{id}/cancel        [User]          Cancel booking
GET    /api/parking-lots/{id}/bookings  [Owner, Admin]  Lot bookings
```

---

## ⏳ **PHASE 3: PAYMENT INTEGRATION**

### **Objective:**
VNPay payment gateway integration with callback handling and transaction tracking.

### **Database Changes:**
```sql
ALTER TABLE PaymentTransactions ADD:
- RefundAmount DECIMAL(12,2)
- RefundReason NVARCHAR(500)
- RefundedAt DATETIME2(7)
- IsDeleted BIT
- Metadata NVARCHAR(MAX) (JSON for additional data)

Payment Status:
- Pending → Processing → Completed
- Failed, Refunded, PartiallyRefunded
```

### **Business Rules:**
1. Create payment after booking confirmed
2. VNPay signature validation (HMAC-SHA512)
3. Idempotent callback handling (VnpTxnRef unique)
4. Auto-update booking status on payment success
5. Support refund (Admin only)
6. Payment timeout: 15 minutes
7. Webhook security (IP whitelist, signature)

### **VNPay Flow:**
```
1. User creates booking
2. Backend generates VNPay URL
3. User redirects to VNPay
4. User pays
5. VNPay redirects back (return_url)
6. VNPay sends IPN (instant payment notification)
7. Backend verifies signature
8. Update payment & booking status
```

### **APIs (6 endpoints):**
```
POST   /api/payments/create             [User]          Create VNPay payment
GET    /api/payments/vnpay-return       [Public]        VNPay return callback
POST   /api/payments/vnpay-ipn          [Public]        VNPay IPN webhook
GET    /api/payments/{id}               [User, Admin]   Get payment status
GET    /api/payments/my                 [User]          My payments
POST   /api/payments/{id}/refund        [Admin]         Refund payment
```

---

## ⏳ **PHASE 4: MAP & LOCATION SERVICES**

### **Objective:**
Geospatial search for finding nearest parking lots based on GPS coordinates.

### **Database Changes:**
```sql
CREATE TABLE ParkingLocations:
- LocationId UNIQUEIDENTIFIER PK
- ParkingLotId UNIQUEIDENTIFIER FK
- Latitude DECIMAL(10,7) NOT NULL
- Longitude DECIMAL(10,7) NOT NULL
- Province NVARCHAR(100)
- District NVARCHAR(100)
- Ward NVARCHAR(100)
- Street NVARCHAR(255)
- FullAddress NVARCHAR(500)

Indexes:
- Spatial index on (Latitude, Longitude)
- Composite index for province/district/ward
```

### **Geo Algorithm:**
```
Haversine Formula:
a = sin²(Δlat/2) + cos(lat1) × cos(lat2) × sin²(Δlong/2)
c = 2 × atan2(√a, √(1−a))
distance = R × c (R = Earth radius = 6371 km)

Optimization:
1. Bounding Box pre-filter (SQL)
2. Haversine calculation (C#)
3. Sort by distance
4. Apply radius filter
```

### **Business Rules:**
1. Validate coordinates: Lat ∈ [-90, 90], Lng ∈ [-180, 180]
2. Default radius: 3000m (3km)
3. Max radius: 50000m (50km)
4. Return only active parking lots with available slots
5. Sort by distance (ascending)
6. Pagination support

### **APIs (5 endpoints):**
```
GET    /api/locations/nearby            [Public]        Find by radius
GET    /api/locations/search            [Public]        Search by address
GET    /api/locations/{id}              [Public]        Location details
POST   /api/locations                   [Admin]         Create location
PUT    /api/locations/{id}              [Admin]         Update location
```

---

## 🗂️ **FILES TO CREATE/UPDATE**

### **Phase 1 (Parking Lot):**
- [x] Domain/Entities/ParkingLot.cs (enhanced)
- [x] Application/DTOs/ParkingLot/ParkingLotDto.cs (enhanced)
- [ ] Application/Interfaces/Repositories/IParkingLotRepository.cs (add pagination)
- [ ] Infrastructure/Repositories/ParkingLotRepository.cs (soft delete, pagination)
- [ ] Application/Interfaces/Services/IParkingLotService.cs (add methods)
- [ ] Application/Services/ParkingLotService.cs (enhance with soft delete)
- [ ] API/Controllers/ParkingLotsController.cs (add endpoints)
- [ ] Infrastructure/DbContext/SmartParkingDBContext.cs (ParkingLot config)
- [x] Database/Migration_Phase1_ParkingLotEnhancements.sql

### **Phase 2 (Booking):**
- [ ] Domain/Entities/Booking.cs (add fields)
- [ ] Application/DTOs/Booking/*.cs (Create, CheckIn, CheckOut, Filter)
- [ ] Application/Interfaces/Repositories/IBookingRepository.cs
- [ ] Infrastructure/Repositories/BookingRepository.cs
- [ ] Application/Interfaces/Services/IBookingService.cs
- [ ] Application/Services/BookingService.cs (slot logic)
- [ ] API/Controllers/BookingsController.cs
- [ ] Database/Migration_Phase2_BookingEnhancements.sql

### **Phase 3 (Payment):**
- [ ] Domain/Entities/PaymentTransaction.cs (add refund fields)
- [ ] Application/DTOs/Payment/*.cs (Create, Callback, Refund)
- [ ] Application/Interfaces/Repositories/IPaymentRepository.cs
- [ ] Infrastructure/Repositories/PaymentRepository.cs
- [ ] Application/Interfaces/Services/IVNPayService.cs
- [ ] Infrastructure/Services/VNPayService.cs (hash, signature)
- [ ] API/Controllers/PaymentsController.cs
- [ ] Database/Migration_Phase3_PaymentEnhancements.sql

### **Phase 4 (Map/Location):**
- [ ] Domain/Entities/ParkingLocation.cs (NEW)
- [ ] Application/DTOs/Location/*.cs (Nearby, Search)
- [ ] Application/Common/Helpers/GeoDistanceHelper.cs
- [ ] Application/Interfaces/Repositories/IParkingLocationRepository.cs
- [ ] Infrastructure/Repositories/ParkingLocationRepository.cs
- [ ] Application/Interfaces/Services/IParkingLocationService.cs
- [ ] Application/Services/ParkingLocationService.cs
- [ ] API/Controllers/LocationsController.cs
- [ ] Database/Migration_Phase4_LocationAndGeo.sql

---

## 🎯 **IMPLEMENTATION ORDER:**

```
Current: Phase 1 - Task 2/7 in progress

Next Steps:
1. Complete Phase 1 (5 more tasks)
2. Run SQL Migration Phase 1
3. Test Phase 1 endpoints
4. Move to Phase 2
5. ... continue sequentially
```

---

## 📝 **NOTES:**

- Each phase builds on previous phases
- All phases follow Clean Architecture
- All endpoints have proper authorization
- All operations support soft delete
- All list endpoints support pagination
- All timestamps in UTC
- All price fields use DECIMAL(18,2) or (12,2)

---

**This document will be updated as implementation progresses.**
