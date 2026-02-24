# SmartParking - Azure Deployment Checklist

> **Trước khi deploy API + DB lên Azure**, kiểm tra các mục sau.

---

## 1. Database (Azure SQL)

### 1.1 Connection String

- [ ] Cập nhật `ConnectionStrings:DefaultConnection` trong `appsettings.json` (hoặc dùng biến môi trường)
- [ ] Dạng: `Server=tcp:YOUR-SERVER.database.windows.net,1433;Database=SmartParking;User ID=xxx;Password=xxx;Encrypt=True;TrustServerCertificate=False;`

### 1.2 Migration

Chạy các migration (nếu dùng EF Core Migrations):

```bash
dotnet ef database update --project src/SmartParking.Infrastructure --startup-project src/SmartParking.API
```

Hoặc chạy thủ công các script trong `docs/`:
- [ ] `UserWallet_Migration.sql` (nếu có bảng UserWallet)
- [ ] `ExtensionRequest_Migration.sql` (nếu có bảng ExtensionRequest)

---

## 2. Secrets & Config (Application Settings)

### 2.1 JWT

- [ ] `JwtSettings:SecretKey` – **≥ 32 ký tự**, dùng cho production
- [ ] `JwtSettings:ValidIssuer` – URL API (ví dụ: `https://your-api.azurewebsites.net`)
- [ ] `JwtSettings:ValidAudience` – giống ValidIssuer hoặc frontend URL

### 2.2 VNPay

- [ ] `VnPay:TmnCode`
- [ ] `VnPay:HashSecret`
- [ ] `VnPay:ReturnUrl` – **phải trỏ về URL Azure**, ví dụ:
  ```
  https://your-api.azurewebsites.net/api/payments/vnpay-callback
  ```

### 2.3 SePay (nếu dùng)

- [ ] `SePay:WebhookSecret`
- [ ] Webhook URL cấu hình trên SePay dashboard trỏ về:
  ```
  https://your-api.azurewebsites.net/api/payments/sepay/webhook
  ```

### 2.4 Email (OTP, Reset Password)

- [ ] SMTP hoặc Email provider config cho môi trường production

---

## 3. API Deployment (Azure App Service)

### 3.1 CORS

Hiện code dùng `SetIsOriginAllowed(origin => true)` – **nên giới hạn** khi deploy:

```csharp
// Thay vì origin => true:
.WithOrigins("https://your-web.azurewebsites.net", "https://yourdomain.com")
.AllowAnyMethod()
.AllowAnyHeader();
```

### 3.2 ValidateSecrets

- [ ] Trong `Program.cs`, `ValidateSecrets()` đang kiểm tra JWT + VNPay – đảm bảo các secrets đã set trong Application Settings

### 3.3 HTTPS

- [ ] Azure App Service mặc định bật HTTPS
- [ ] Trong `launchSettings.json` dev dùng HTTP – production sẽ dùng HTTPS

---

## 4. Chức năng cần kiểm tra kỹ

### 4.1 Tìm kiếm bãi gửi xe

| API | Ghi chú |
|-----|---------|
| `GET /api/parking-lots/nearby?lat=&lng=&radiusKm=5&maxResults=20` | Haversine, chỉ bãi có Latitude/Longitude |
| `GET /api/parking-lots?search=&isActive=true&status=Approved` | List phân trang |
| `GET /api/locations/nearby` | Bán kính mét |
| `GET /api/locations/search?province=&district=&ward=` | Theo địa chỉ |

**Lưu ý:** Owner phải nhập **Latitude, Longitude** khi tạo bãi để bãi xuất hiện trong tìm kiếm nearby.

### 4.2 Booking flow

- [ ] Wallet: User pay → Owner/Admin approve → Check-in → Check-out
- [ ] VNPay/SePay: User tạo payment URL → thanh toán → callback auto Confirmed → Check-in → Check-out

### 4.3 Owner Upgrade flow

1. `POST /api/owners/upgrade-request` (không cần PaymentTransactionId)
2. `POST /api/payments/owner-subscription/vnpay` hoặc `sepay` với `ownerUpgradeRequestId`
3. Thanh toán → callback chuyển status sang `PendingApproval`
4. Admin: `POST /api/admin/owners/upgrade-requests/{id}/approve`

---

## 5. Postman Full Flow Test

Import file `docs/SmartParking_FullFlow_A2Z.postman_collection.json` và test tuần tự:

1. Auth: Register → Verify OTP → Login  
2. Tìm bãi: Nearby / List / Detail  
3. Vehicle: Add  
4. Booking: Create → Pay (Wallet/VNPay/SePay) → Owner approve (nếu Wallet) → Check-in → Check-out  
5. Owner Upgrade: Plans → Create request → Pay subscription → Admin approve  
6. Owner: Create parking lot → Admin approve  
7. Admin: Dashboard, approve requests  

**Variables:** Đổi `baseUrl` sang `https://your-api.azurewebsites.net` khi test production.

---

## 6. Backup & Rollback

- [ ] Backup DB trước khi migration
- [ ] Slot deployment để rollback nhanh nếu lỗi

---

## 7. Monitoring

- [ ] Application Insights (nếu bật)
- [ ] Log streaming trên Azure Portal
- [ ] Health check endpoint (nếu có)

---

*Cập nhật: 2026-02-23*
