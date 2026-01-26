# 🚀 QUICK START - TESTING ALL 4 PHASES

**Date:** 2026-01-26  
**Status:** All phases implemented, ready for testing

---

## ⚡ **QUICK SETUP (5 MINUTES)**

### **1. Run SQL Migrations (2 minutes)**

**In SSMS, execute in order:**

```sql
-- Migration 3: Payment
:r "E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking\Database\Migration_Phase3_PaymentEnhancements.sql"

-- Migration 4: Map/Location
:r "E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking\Database\Migration_Phase4_LocationAndGeo.sql"
```

**Expected:** Both migrations complete successfully with "Migration completed successfully!"

---

### **2. Start Application (30 seconds)**

```bash
cd "E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking\src\SmartParking.API"
dotnet run
```

**Expected:** App starts on `http://localhost:5070` and `https://localhost:7278`

---

### **3. Open Swagger (10 seconds)**

Navigate to: `https://localhost:7278/swagger`

---

## 🧪 **TESTING WORKFLOW**

### **Step 1: Authentication (Phase 0)**

1. **Register New User:**
   ```
   POST /api/auth/register-request
   {
     "email": "test@example.com",
     "password": "Test123!@#"
   }
   ```
   - Check email for OTP
   - Copy OTP code

2. **Verify OTP:**
   ```
   POST /api/auth/verify-otp
   {
     "email": "test@example.com",
     "otpCode": "123456"
   }
   ```
   - Save the `accessToken` and `refreshToken`

3. **Login:**
   ```
   POST /api/auth/login
   {
     "email": "test@example.com",
     "password": "Test123!@#"
   }
   ```
   - Save tokens

4. **Authorize in Swagger:**
   - Click "Authorize" button
   - Enter: `Bearer {accessToken}`
   - Click "Authorize"

---

### **Step 2: Create Parking Lot (Phase 1)**

**As Owner (or Admin):**

1. **Create Parking Lot:**
   ```
   POST /api/parking-lots
   {
     "name": "Downtown Parking",
     "address": "123 Main Street, Ho Chi Minh City",
     "totalCapacity": 50,
     "pricePerHour": 10000
   }
   ```
   - Save `parkingLotId`

2. **View All Parking Lots:**
   ```
   GET /api/parking-lots?page=1&pageSize=10&isActive=true
   ```

3. **View My Parking Lots:**
   ```
   GET /api/parking-lots/my
   ```

---

### **Step 3: Create Location (Phase 4)**

**As Admin:**

1. **Create Location:**
   ```
   POST /api/locations
   {
     "parkingLotId": "{parkingLotId from Step 2}",
     "latitude": 10.762622,
     "longitude": 106.660172,
     "province": "Ho Chi Minh",
     "district": "District 1",
     "ward": "Ben Nghe",
     "street": "123 Main Street",
     "fullAddress": "123 Main Street, Ben Nghe, District 1, Ho Chi Minh"
   }
   ```
   - Save `locationId`

2. **Find Nearby Locations:**
   ```
   GET /api/locations/nearby?lat=10.762622&lng=106.660172&radius=3000
   ```
   - Should return your location with distance

---

### **Step 4: Create Booking (Phase 2)**

**As User:**

1. **Create Booking:**
   ```
   POST /api/bookings
   {
     "parkingLotId": "{parkingLotId}",
     "startTime": "2026-01-27T10:00:00Z",
     "endTime": "2026-01-27T12:00:00Z"
   }
   ```
   - Save `bookingId`

2. **View My Bookings:**
   ```
   GET /api/bookings/my-bookings?page=1&pageSize=10
   ```

3. **Check-In:**
   ```
   POST /api/bookings/{bookingId}/check-in
   ```

4. **Check-Out:**
   ```
   POST /api/bookings/{bookingId}/check-out
   ```
   - Should return calculated `totalAmount`

---

### **Step 5: Create Payment (Phase 3)**

**As User:**

1. **Create Payment:**
   ```
   POST /api/payments/create
   {
     "bookingId": "{bookingId}",
     "amount": 20000,
     "description": "Parking fee"
   }
   ```
   - Returns `paymentUrl` - this is the VNPay payment URL

2. **Query Payment Status:**
   ```
   GET /api/payments/booking/{bookingId}
   ```

---

## ✅ **VERIFICATION CHECKLIST**

After testing, verify:

- [ ] All migrations executed successfully
- [ ] Application starts without errors
- [ ] Swagger UI loads correctly
- [ ] Authentication endpoints work
- [ ] Parking lot CRUD works
- [ ] Location search works (nearby + address)
- [ ] Booking lifecycle works (create → check-in → check-out)
- [ ] Payment URL generation works
- [ ] Authorization works (Owner can't access other's lots)
- [ ] Soft delete works (deleted items don't appear)
- [ ] Pagination works on all list endpoints

---

## 🐛 **TROUBLESHOOTING**

### **Issue: Migration fails**
- Check SQL Server connection
- Verify database exists
- Check for existing columns/indexes

### **Issue: Build fails**
- Run `dotnet clean`
- Run `dotnet restore`
- Run `dotnet build`

### **Issue: App won't start**
- Check `appsettings.json` for JWT secret
- Verify database connection string
- Check if ports 5070/7278 are available

### **Issue: 401 Unauthorized**
- Verify token is valid
- Check token expiration
- Re-login to get new token

---

## 📞 **SUPPORT**

If you encounter issues:
1. Check application logs
2. Check database directly (SSMS)
3. Verify all migrations ran successfully
4. Check Swagger for detailed error messages

---

**Happy Testing!** 🎉
