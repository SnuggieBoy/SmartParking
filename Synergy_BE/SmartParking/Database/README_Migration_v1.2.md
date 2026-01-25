# Database Migration v1.2 - CheckInTime & CheckOutTime

## 📝 Overview

Migration v1.2 thêm 2 cột mới vào bảng `Bookings` để tracking thời gian check-in/check-out chính xác.

## 🔄 Changes

### Before (v1.1)
```sql
Bookings (
    BookingId,
    UserId,
    ParkingLotId,
    VehicleId,
    BookingTime,      -- Dùng chung (không rõ ràng)
    StartTime,
    EndTime,
    Status,
    TotalAmount,
    CreatedAt,
    UpdatedAt,        -- Dùng chung (không rõ ràng)
    RowVersion
)
```

### After (v1.2)
```sql
Bookings (
    ...,
    CheckInTime,      ✅ NEW - Thời gian check-in thực tế
    CheckOutTime,     ✅ NEW - Thời gian check-out thực tế
    ...
)
```

## 🚀 How to Run

### Option 1: SQL Server Management Studio (SSMS)
1. Open SSMS
2. Connect to your SQL Server instance
3. Open `Migration_AddCheckInCheckOutTime.sql`
4. Execute (F5)
5. Check output messages for success

### Option 2: Command Line (sqlcmd)
```bash
sqlcmd -S YOUR_SERVER -d SmartParkingDB -i Migration_AddCheckInCheckOutTime.sql
```

### Option 3: Azure Data Studio
1. Open Azure Data Studio
2. Connect to database
3. Open `Migration_AddCheckInCheckOutTime.sql`
4. Run (Ctrl+Shift+E)

## ✅ Expected Output

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

Summary of changes:
1. ✓ CheckInTime column added (DATETIME2 NULL)
2. ✓ CheckOutTime column added (DATETIME2 NULL)
3. ✓ Existing data migrated (if any)

Next steps:
- Update your C# entity models
- Rebuild the application
- Test check-in/check-out endpoints

Completion time: 2026-01-25 12:45:00
```

## 🔍 Verification Query

Chạy query sau để xác nhận migration thành công:

```sql
USE SmartParkingDB;
GO

-- Check columns exist
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Bookings'
  AND COLUMN_NAME IN ('CheckInTime', 'CheckOutTime');

-- Should return:
-- CheckInTime  | datetime2 | YES
-- CheckOutTime | datetime2 | YES
```

## 📊 Data Migration Logic

Script tự động migrate dữ liệu cũ (nếu có):

1. **CheckInTime**: Copy từ `BookingTime` cho bookings có status `InProgress` hoặc `Completed`
2. **CheckOutTime**: Copy từ `UpdatedAt` cho bookings có status `Completed`

```sql
-- Bookings with status InProgress/Completed
UPDATE Bookings
SET CheckInTime = BookingTime
WHERE BookingTime IS NOT NULL 
  AND CheckInTime IS NULL
  AND Status IN ('InProgress', 'Completed');

-- Completed bookings only
UPDATE Bookings
SET CheckOutTime = UpdatedAt
WHERE UpdatedAt IS NOT NULL 
  AND CheckOutTime IS NULL
  AND Status = 'Completed';
```

## 🔄 Impact on Application Code

### Updated Files

#### 1. Entity Model
`src/SmartParking.Domain/Entities/Booking.cs`
```csharp
public partial class Booking
{
    // ... existing properties ...
    
    public DateTime? CheckInTime { get; set; }      // ✅ NEW
    public DateTime? CheckOutTime { get; set; }     // ✅ NEW
    
    // ... rest ...
}
```

#### 2. DbContext Configuration
`src/SmartParking.Infrastructure/DbContext/SmartParkingDBContext.cs`
```csharp
modelBuilder.Entity<Booking>(entity =>
{
    // ... existing config ...
    entity.Property(e => e.CheckInTime);
    entity.Property(e => e.CheckOutTime);
    // ...
});
```

#### 3. BookingService
`src/SmartParking.Application/Services/BookingService.cs`
```csharp
// Check-in
booking.CheckInTime = now;  // ✅ Uses new column

// Check-out
booking.CheckOutTime = now; // ✅ Uses new column
```

## 🧪 Testing

### 1. Run Migration
```bash
sqlcmd -S (Local) -d SmartParkingDB -i Migration_AddCheckInCheckOutTime.sql
```

### 2. Rebuild Application
```bash
cd src/SmartParking.API
dotnet build
```

### 3. Test Check-In/Out Endpoints

**Check-In:**
```http
POST https://localhost:7278/api/bookings/{bookingId}/check-in
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "success": true,
  "data": {
    "bookingId": "guid",
    "status": "InProgress",
    "checkInTime": "2026-01-25T12:30:00Z"
  }
}
```

**Check-Out:**
```http
POST https://localhost:7278/api/bookings/{bookingId}/check-out
Authorization: Bearer YOUR_TOKEN
```

**Response:**
```json
{
  "success": true,
  "data": {
    "bookingId": "guid",
    "status": "Completed",
    "checkOutTime": "2026-01-25T14:30:00Z",
    "totalAmount": 50000.00
  }
}
```

## ⚠️ Important Notes

1. **Nullable Columns**: Both `CheckInTime` and `CheckOutTime` are **nullable** (optional)
2. **Backward Compatible**: Existing bookings work fine (columns are NULL)
3. **Idempotent**: Script can be run multiple times safely (checks column existence)
4. **No Data Loss**: Does not drop or modify existing columns
5. **Zero Downtime**: Can be run on production (just adds columns)

## 🔙 Rollback (If Needed)

If you need to rollback this migration:

```sql
USE SmartParkingDB;
GO

-- Remove CheckInTime column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bookings') AND name = 'CheckInTime')
BEGIN
    ALTER TABLE Bookings DROP COLUMN CheckInTime;
    PRINT '✓ CheckInTime column removed';
END

-- Remove CheckOutTime column
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Bookings') AND name = 'CheckOutTime')
BEGIN
    ALTER TABLE Bookings DROP COLUMN CheckOutTime;
    PRINT '✓ CheckOutTime column removed';
END

PRINT 'Rollback complete';
GO
```

## 📋 Migration History

| Version | Date | Changes |
|---------|------|---------|
| v1.0 | 2026-01-25 | Initial schema |
| v1.1 | 2026-01-25 | Added Vehicles table, updated Bookings |
| **v1.2** | **2026-01-25** | **Added CheckInTime/CheckOutTime to Bookings** |

---

**Status**: ✅ Ready to Run  
**Risk Level**: 🟢 Low (additive change only)  
**Downtime**: ⚡ Zero  
**Rollback**: ✅ Available
