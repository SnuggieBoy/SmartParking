# 🔐 ADMIN API ENDPOINTS - COMPLETE REFERENCE

**SmartParking Backend API** - Tất cả endpoints dành cho Admin Dashboard

---

## 📋 **OVERVIEW**

Tất cả Admin endpoints có prefix: `/api/admin/`

**Security:** Tất cả endpoints yêu cầu:
- ✅ Authentication (Bearer Token)
- ✅ Role: **Admin** only

---

## 📊 **1. DASHBOARD ENDPOINTS**

### **1.1. Users Summary**
```
GET /api/admin/dashboard/users-summary
```
**Response:**
```json
{
  "success": true,
  "data": {
    "totalUsers": 120,
    "totalHosts": 40,
    "totalDrivers": 80,
    "newUsersToday": 3
  }
}
```

---

### **1.2. Parking Lots Summary**
```
GET /api/admin/dashboard/parking-lots-summary
```
**Response:**
```json
{
  "success": true,
  "data": {
    "totalActiveParkingLots": 25,
    "totalInactiveParkingLots": 5
  }
}
```

---

### **1.3. Revenue Today**
```
GET /api/admin/dashboard/revenue-today
```
**Response:**
```json
{
  "success": true,
  "data": {
    "date": "2026-01-28",
    "totalCommission": 150000,
    "totalRevenue": 1000000,
    "totalCompletedBookings": 12
  }
}
```

---

### **1.4. Bookings Summary**
```
GET /api/admin/dashboard/bookings-summary
```
**Response:**
```json
{
  "success": true,
  "data": {
    "activeBookings": 7,
    "pendingBookings": 3,
    "completedToday": 12
  }
}
```

---

### **1.5. Recent Activities**
```
GET /api/admin/dashboard/recent-activities?limit=10
```
**Query Parameters:**
- `limit` (default: 10): Số activities trả về

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "type": "BookingCreated",
      "description": "Tài xế Nguyễn Văn A đặt chỗ tại Bãi Xe Đại Học FPT",
      "relatedId": "booking-guid",
      "createdAt": "2026-01-28T10:15:00Z",
      "userName": "Nguyễn Văn A",
      "parkingLotName": "Bãi Xe Đại Học FPT",
      "amount": 50000,
      "status": "Pending"
    },
    {
      "type": "PaymentSucceeded",
      "description": "Thanh toán thành công 50,000 VNĐ cho booking tại Bãi Xe Đại Học FPT",
      "relatedId": "payment-guid",
      "createdAt": "2026-01-28T10:20:00Z",
      "userName": "Nguyễn Văn A",
      "parkingLotName": "Bãi Xe Đại Học FPT",
      "amount": 50000,
      "status": "Success"
    },
    {
      "type": "UserRegistered",
      "description": "User Trần Văn B vừa đăng ký tài khoản",
      "relatedId": "user-guid",
      "createdAt": "2026-01-28T09:50:00Z",
      "userName": "Trần Văn B",
      "parkingLotName": null,
      "amount": null,
      "status": null
    }
  ]
}
```

---

## 👥 **2. USER MANAGEMENT**

### **2.1. Get All Users**
```
GET /api/admin/users?search=&roleName=&isActive=&emailConfirmed=&page=1&pageSize=20
```
**Query Parameters:**
- `search`: Tìm kiếm theo tên/email/phone
- `roleName`: Filter theo role (User, Owner, Admin)
- `isActive`: Filter theo trạng thái active (true/false)
- `emailConfirmed`: Filter theo email confirmed (true/false)
- `page`: Số trang (default: 1)
- `pageSize`: Số item mỗi trang (default: 20)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "guid",
        "fullName": "Nguyễn Văn A",
        "email": "user@example.com",
        "phone": "0123456789",
        "roleName": "User",
        "isActive": true,
        "emailConfirmed": true,
        "createdAt": "2026-01-01T00:00:00Z",
        "totalBookings": 5,
        "totalParkingLots": 0
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 120
  }
}
```

---

### **2.2. Get User By ID**
```
GET /api/admin/users/{id}
```

---

### **2.3. Update User**
```
PUT /api/admin/users/{id}
Body: {
  "fullName": "New Name",
  "phone": "0987654321",
  "isActive": true,
  "roleId": 2
}
```

---

### **2.4. Toggle User Active Status**
```
PATCH /api/admin/users/{id}/toggle-active
```

---

## 🅿️ **3. PARKING LOT MANAGEMENT**

### **3.1. Get All Parking Lots**
```
GET /api/admin/parking-lots?search=&isActive=&status=&ownerId=&page=1&pageSize=20
```
**Query Parameters:**
- `search`: Tìm kiếm theo tên/địa chỉ
- `isActive`: Filter theo active (true/false)
- `status`: Filter theo status
- `ownerId`: Filter theo owner
- `page`, `pageSize`: Pagination

---

### **3.2. Get Parking Lot By ID**
```
GET /api/admin/parking-lots/{id}
```

---

### **3.3. Create Parking Lot**
```
POST /api/admin/parking-lots?ownerId={optional}
Body: {
  "name": "Bãi Xe ABC",
  "address": "123 Đường XYZ",
  "totalCapacity": 50,
  "pricePerHour": 5000
}
```
**Note:** Admin có thể chỉ định `ownerId` trong query, nếu không thì dùng admin's ID

---

### **3.4. Update Parking Lot**
```
PUT /api/admin/parking-lots/{id}
Body: {
  "name": "Updated Name",
  "address": "Updated Address",
  "totalCapacity": 60,
  "pricePerHour": 6000,
  "isActive": true
}
```

---

### **3.5. Toggle Active Status**
```
PATCH /api/admin/parking-lots/{id}/toggle-active
```

---

### **3.6. Delete Parking Lot**
```
DELETE /api/admin/parking-lots/{id}
```

---

### **3.7. Get Bookings for Parking Lot**
```
GET /api/admin/parking-lots/{id}/bookings?page=1&pageSize=20
```

---

## 📅 **4. BOOKING MANAGEMENT**

### **4.1. Get All Bookings**
```
GET /api/admin/bookings?status=&userId=&parkingLotId=&page=1&pageSize=20
```
**Query Parameters:**
- `status`: Filter theo status (Pending, Confirmed, Active, Completed, Cancelled)
- `userId`: Filter theo user
- `parkingLotId`: Filter theo parking lot
- `page`, `pageSize`: Pagination

---

### **4.2. Get Booking By ID**
```
GET /api/admin/bookings/{id}
```

---

### **4.3. Update Booking**
```
PUT /api/admin/bookings/{id}
Body: {
  "startTime": "2026-01-28T10:00:00Z",
  "endTime": "2026-01-28T12:00:00Z"
}
```

---

### **4.4. Cancel Booking**
```
POST /api/admin/bookings/{id}/cancel
```

---

### **4.5. Check-In Booking**
```
POST /api/admin/bookings/{id}/check-in
```

---

### **4.6. Check-Out Booking**
```
POST /api/admin/bookings/{id}/check-out
```

---

## 📍 **5. PARKING LOCATION MANAGEMENT**

### **5.1. Get All Locations**
```
GET /api/admin/locations?province=&district=&ward=&search=&page=1&pageSize=20
```

---

### **5.2. Get Location By ID**
```
GET /api/admin/locations/{id}
```

---

### **5.3. Create Location**
```
POST /api/admin/locations
Body: {
  "parkingLotId": "guid",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "province": "TP. Hồ Chí Minh",
  "district": "Quận 1",
  "ward": "Phường Bến Nghé"
}
```

---

### **5.4. Update Location**
```
PUT /api/admin/locations/{id}
Body: {
  "latitude": 10.762622,
  "longitude": 106.660172,
  "province": "Updated Province",
  ...
}
```

---

### **5.5. Delete Location**
```
DELETE /api/admin/locations/{id}
```

---

## 🚗 **6. VEHICLE MANAGEMENT**

### **6.1. Get Vehicle By ID**
```
GET /api/admin/vehicles/{id}
```

---

### **6.2. Get Vehicles By User ID**
```
GET /api/admin/vehicles/user/{userId}?activeOnly=false
```

---

### **6.3. Update Vehicle**
```
PUT /api/admin/vehicles/{id}
Body: {
  "licensePlate": "30A-12345",
  "vehicleType": "Car",
  "brand": "Toyota",
  "model": "Camry",
  "color": "White"
}
```

---

### **6.4. Delete Vehicle**
```
DELETE /api/admin/vehicles/{id}
```

---

## 🔑 **AUTHENTICATION**

Tất cả endpoints yêu cầu **Bearer Token** trong header:

```
Authorization: Bearer {your-jwt-token}
```

**Lấy token:**
```
POST /api/auth/login
Body: {
  "email": "admin@example.com",
  "password": "password"
}
```

---

## 📝 **RESPONSE FORMAT**

Tất cả responses theo format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "timestamp": "2026-01-28T10:00:00Z"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Detail 1", "Detail 2"],
  "timestamp": "2026-01-28T10:00:00Z"
}
```

---

## 🎯 **QUICK REFERENCE**

| Resource | Endpoints | Count |
|----------|-----------|-------|
| **Dashboard** | `/api/admin/dashboard/*` | 5 |
| **Users** | `/api/admin/users` | 4 |
| **Parking Lots** | `/api/admin/parking-lots` | 7 |
| **Bookings** | `/api/admin/bookings` | 6 |
| **Locations** | `/api/admin/locations` | 5 |
| **Vehicles** | `/api/admin/vehicles` | 4 |
| **TOTAL** | | **31 endpoints** |

---

## 🧪 **TESTING**

### **Swagger UI:**
```
https://localhost:7278/swagger
```

### **Postman Collection:**
Tất cả endpoints có thể test qua Swagger UI hoặc Postman với:
- Base URL: `https://localhost:7278`
- Auth: Bearer Token từ `/api/auth/login`

---

## 📚 **RELATED DOCUMENTATION**

- **Mobile Dev Guide**: `docs/MOBILE_DEV_API_GUIDE.md`
- **SePay Integration**: `docs/SEPAY_INTEGRATION_GUIDE.md`
- **Testing Guide**: `TESTING_GUIDE.md`

---

**Happy coding!** 🚀
