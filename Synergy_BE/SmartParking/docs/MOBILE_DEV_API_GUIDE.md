# 📱 MOBILE DEV - API INTEGRATION GUIDE

**SmartParking Backend API** - Hướng dẫn tích hợp cho Mobile Developer

---

## 🚀 **QUICK START**

### **1. Base URL**

**Development (Local):**
```
http://YOUR_PC_IP:5070
https://YOUR_PC_IP:7278
```

**Lưu ý:**
- Thay `YOUR_PC_IP` bằng IP thật của máy chạy backend (ví dụ: `192.168.1.100`)
- Tìm IP: 
  - Windows: `ipconfig` → tìm `IPv4 Address`
  - Mac/Linux: `ifconfig` hoặc `ip addr`

**Production (khi deploy):**
```
https://api.smartparking.com
```
*(Sẽ update sau khi deploy)*

---

## 📋 **ENDPOINTS CẦN DÙNG**

### **1. Lấy danh sách bãi gửi xe**

**Endpoint:**
```
GET /api/parking-lots
```

**Query Parameters:**
- `search` (optional): Tìm kiếm theo tên/địa chỉ
- `isActive` (optional): `true`/`false`
- `status` (optional): `"Active"`, `"Inactive"`, etc.
- `page` (default: 1): Số trang
- `pageSize` (default: 10): Số item mỗi trang

**Example Request:**
```http
GET http://192.168.1.100:5070/api/parking-lots?page=1&pageSize=20
```

**Response:**
```json
{
  "success": true,
  "message": "Parking lots retrieved successfully",
  "data": {
    "items": [
      {
        "parkingLotId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "ownerName": "John Doe",
        "name": "Bãi Xe Đại Học FPT",
        "address": "Lô E2a-7, Đường D1, Long Thạnh Mỹ, TP. Thủ Đức",
        "totalCapacity": 100,
        "availableCapacity": 45,
        "currentOccupancy": 55,
        "pricePerHour": 5000,
        "status": "Active",
        "isActive": true,
        "createdAt": "2026-01-27T10:00:00Z",
        "updatedAt": "2026-01-27T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 25
  }
}
```

---

### **2. Lấy chi tiết 1 bãi gửi xe**

**Endpoint:**
```
GET /api/parking-lots/{id}
```

**Example Request:**
```http
GET http://192.168.1.100:5070/api/parking-lots/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**
```json
{
  "success": true,
  "message": "Parking lot retrieved successfully",
  "data": {
    "parkingLotId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ownerName": "John Doe",
    "name": "Bãi Xe Đại Học FPT",
    "address": "Lô E2a-7, Đường D1, Long Thạnh Mỹ, TP. Thủ Đức",
    "totalCapacity": 100,
    "availableCapacity": 45,
    "currentOccupancy": 55,
    "pricePerHour": 5000,
    "status": "Active",
    "isActive": true,
    "createdAt": "2026-01-27T10:00:00Z",
    "updatedAt": "2026-01-27T10:00:00Z"
  }
}
```

---

## 🗺️ **CHỨC NĂNG DẪN ĐƯỜNG (NAVIGATION)**

### **Flow:**

1. **Lấy vị trí hiện tại của user** (GPS từ thiết bị)
2. **Lấy danh sách bãi** từ API (có `latitude` / `longitude`)
3. **User chọn bãi** → Bấm nút "Dẫn đường"
4. **Mở Google Maps** với destination = tọa độ bãi

---

### **Step 1: Lấy vị trí user (Mobile)**

**Flutter:**
```dart
import 'package:geolocator/geolocator.dart';

Future<Position> getCurrentLocation() async {
  bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
  if (!serviceEnabled) {
    throw Exception('Location services are disabled.');
  }

  LocationPermission permission = await Geolocator.checkPermission();
  if (permission == LocationPermission.denied) {
    permission = await Geolocator.requestPermission();
    if (permission == LocationPermission.denied) {
      throw Exception('Location permissions are denied');
    }
  }

  Position position = await Geolocator.getCurrentPosition();
  return position; // position.latitude, position.longitude
}
```

**React Native:**
```javascript
import Geolocation from '@react-native-community/geolocation';

Geolocation.getCurrentPosition(
  (position) => {
    const lat = position.coords.latitude;
    const lng = position.coords.longitude;
  },
  (error) => {
    console.error(error);
  }
);
```

---

### **Step 2: Parse response từ API**

**Model (TypeScript/Dart):**
```typescript
interface ParkingLot {
  parkingLotId: string;
  name: string;
  address: string;
  latitude: number | null;   // ⚠️ Có thể null
  longitude: number | null;   // ⚠️ Có thể null
  totalCapacity: number;
  availableCapacity: number;
  pricePerHour: number;
  isActive: boolean;
}
```

**Lưu ý:**
- `latitude` / `longitude` có thể là `null` nếu bãi chưa được cấu hình tọa độ
- **Chỉ hiển thị nút "Dẫn đường"** khi `latitude != null && longitude != null`

---

### **Step 3: Mở Google Maps**

**Universal URL (hoạt động trên mọi platform):**
```
https://www.google.com/maps/dir/?api=1&destination={lat},{lng}&travelmode=driving
```

**Flutter:**
```dart
import 'package:url_launcher/url_launcher.dart';

Future<void> openGoogleMaps(double lat, double lng) async {
  final url = 'https://www.google.com/maps/dir/?api=1&destination=$lat,$lng&travelmode=driving';
  final uri = Uri.parse(url);
  
  if (await canLaunchUrl(uri)) {
    await launchUrl(uri, mode: LaunchMode.externalApplication);
  } else {
    throw Exception('Could not launch Google Maps');
  }
}

// Usage:
openGoogleMaps(parkingLot.latitude!, parkingLot.longitude!);
```

**React Native:**
```javascript
import { Linking } from 'react-native';

const openGoogleMaps = (lat, lng) => {
  const url = `https://www.google.com/maps/dir/?api=1&destination=${lat},${lng}&travelmode=driving`;
  Linking.openURL(url).catch(err => console.error('Error opening maps:', err));
};

// Usage:
openGoogleMaps(parkingLot.latitude, parkingLot.longitude);
```

**Native Android (Kotlin):**
```kotlin
fun openGoogleMaps(lat: Double, lng: Double) {
    val uri = "https://www.google.com/maps/dir/?api=1&destination=$lat,$lng&travelmode=driving"
    val intent = Intent(Intent.ACTION_VIEW, Uri.parse(uri))
    intent.setPackage("com.google.android.apps.maps")
    try {
        startActivity(intent)
    } catch (e: ActivityNotFoundException) {
        // Fallback to browser
        val browserIntent = Intent(Intent.ACTION_VIEW, Uri.parse(uri))
        startActivity(browserIntent)
    }
}
```

**Native iOS (Swift):**
```swift
func openGoogleMaps(lat: Double, lng: Double) {
    let urlString = "https://www.google.com/maps/dir/?api=1&destination=\(lat),\(lng)&travelmode=driving"
    if let url = URL(string: urlString) {
        if UIApplication.shared.canOpenURL(url) {
            UIApplication.shared.open(url)
        }
    }
}
```

---

## 🧪 **TESTING**

### **1. Test API từ Postman/Browser:**

```http
GET http://192.168.1.100:5070/api/parking-lots
```

**Hoặc dùng Swagger UI:**
```
http://192.168.1.100:5070/swagger
```

### **2. Test trên Mobile:**

**Android Emulator:**
- Dùng `10.0.2.2` thay vì `localhost`:
  ```
  http://10.0.2.2:5070/api/parking-lots
  ```

**iOS Simulator:**
- Dùng `localhost` hoặc IP thật của máy:
  ```
  http://192.168.1.100:5070/api/parking-lots
  ```

**Physical Device:**
- Đảm bảo mobile và PC cùng WiFi
- Dùng IP thật của PC:
  ```
  http://192.168.1.100:5070/api/parking-lots
  ```

---

## ⚠️ **TROUBLESHOOTING**

### **Lỗi: Connection Refused / Timeout**

**Nguyên nhân:**
1. Backend chưa chạy
2. Firewall chặn port
3. IP không đúng
4. Mobile và PC khác mạng WiFi

**Giải pháp:**
1. **Kiểm tra backend đang chạy:**
   ```bash
   # Backend dev chạy:
   dotnet run --project src/SmartParking.API
   ```
   → Phải thấy log: `Now listening on: http://0.0.0.0:5070`

2. **Kiểm tra firewall:**
   - Windows: Mở `Windows Defender Firewall` → Allow port `5070` và `7278`
   - Mac: System Preferences → Security → Firewall → Allow incoming connections

3. **Kiểm tra IP:**
   ```bash
   # Windows:
   ipconfig
   
   # Mac/Linux:
   ifconfig
   ```
   → Tìm `IPv4 Address` (ví dụ: `192.168.1.100`)

4. **Test từ browser trên PC:**
   ```
   http://localhost:5070/swagger
   ```
   → Nếu không mở được → Backend chưa chạy hoặc port sai

---

### **Lỗi: latitude/longitude = null**

**Nguyên nhân:**
- Bãi chưa được cấu hình tọa độ trong database

**Giải pháp:**
- **Backend dev** cần update bãi với `latitude` / `longitude` qua API:
  ```
  PUT /api/parking-lots/{id}
  Body: {
    "latitude": 10.762622,
    "longitude": 106.660172,
    ...
  }
  ```

---

## 📚 **TÀI LIỆU THAM KHẢO**

- **Google Maps URL Scheme:** https://developers.google.com/maps/documentation/urls/get-started
- **Flutter Geolocator:** https://pub.dev/packages/geolocator
- **React Native Geolocation:** https://github.com/react-native-community/react-native-geolocation

---

## 📞 **LIÊN HỆ**

Nếu gặp vấn đề:
1. Kiểm tra backend đang chạy
2. Kiểm tra IP và port
3. Kiểm tra Swagger UI: `http://YOUR_IP:5070/swagger`
4. Liên hệ backend dev để được hỗ trợ

---

**Good luck!** 🚀
