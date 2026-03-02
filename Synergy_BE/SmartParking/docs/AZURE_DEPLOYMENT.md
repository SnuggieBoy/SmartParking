# Hướng dẫn Deploy SmartParking Backend lên Azure

Tham khảo cấu hình TechStore (`TechStore.API`) đã deploy thành công.

---

## 1. Chuẩn bị trước khi deploy

### 1.1 Azure Resources cần tạo

| Resource | Mục đích |
|----------|----------|
| **App Service** (Web App) | Host API .NET 8 |
| **Azure SQL Database** | Database SmartParkingDB |
| **Resource Group** | Gom tất cả resources |

### 1.2 Cấu hình cần có

- Connection string Azure SQL
- JWT SecretKey (ít nhất 32 ký tự)
- Cloudinary: CloudName, ApiKey, ApiSecret (nếu dùng upload ảnh)

---

## 2. Các bước deploy

### Bước 1: Tạo Azure SQL Database

1. Portal Azure > Create resource > Azure SQL Database
2. Chọn Server mới hoặc dùng server có sẵn
3. Database name: `SmartParkingDB`
4. Lưu connection string dạng:
   ```
   Server=tcp:xxx.database.windows.net,1433;Database=SmartParkingDB;User ID=xxx;Password=xxx;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
   ```

### Bước 2: Chạy Migration

```bash
cd Synergy_BE/SmartParking/src/SmartParking.API
dotnet ef database update --project ../SmartParking.Infrastructure --startup-project .
```

*(Cập nhật connection string trong appsettings.Development.json trước khi chạy)*

### Bước 3: Tạo App Service

1. Portal Azure > Create resource > Web App
2. Runtime: **.NET 8 (LTS)**
3. OS: Windows
4. Region: Southeast Asia (hoặc gần bạn)
5. App Service Plan: B1 trở lên (Free F1 có giới hạn)

### Bước 4: Cấu hình Application Settings (Azure Portal)

Vào App Service > Configuration > Application settings, thêm:

| Name | Value | Ghi chú |
|------|-------|---------|
| `ConnectionStrings__DefaultConnection` | *(connection string Azure SQL)* | Dấu `__` thay cho `:` |
| `JwtSettings__SecretKey` | *(chuỗi bí mật ≥32 ký tự)* | **Bắt buộc** |
| `CloudinarySettings__CloudName` | *(cloud name)* | Nếu dùng upload ảnh |
| `CloudinarySettings__ApiKey` | *(api key)* | |
| `CloudinarySettings__ApiSecret` | *(api secret)* | |
| `CORS__AllowedOrigins` | `*` hoặc `https://your-app.vercel.app` | Cho phép frontend gọi API |

### Bước 5: Publish từ Visual Studio / Rider

1. Right-click `SmartParking.API` > **Publish**
2. Target: **Azure** > **Azure App Service (Windows)**
3. Chọn App Service đã tạo (hoặc tạo mới)
4. Publish

**Hoặc dùng Publish Profile có sẵn:**

- File: `Properties/PublishProfiles/SmartParking-Azure-WebDeploy.pubxml`
- Cập nhật `YOUR_APP_NAME` thành tên App Service của bạn
- Hoặc download Publish Profile từ Azure Portal và thay thế

### Bước 6: Chạy Migration trên Azure

Sau khi deploy, cần chạy migration với connection string Production. Có 2 cách:

**Cách 1:** Dùng Azure Cloud Shell hoặc local với connection string Production:
```bash
$env:ConnectionStrings__DefaultConnection="Server=tcp:xxx.database.windows.net,..."
dotnet ef database update --project ../SmartParking.Infrastructure --startup-project .
```

**Cách 2:** Thêm migration vào startup (nếu muốn auto-migrate khi app khởi động).

---

## 3. Kiểm tra sau deploy

1. Mở `https://YOUR_APP.azurewebsites.net` → Phải thấy Swagger UI
2. Test endpoint `GET /api/parking-lots/nearby?lat=21&lng=105&radiusKm=5`
3. Test login/register qua Swagger

---

## 4. Mobile App cấu hình

Trong `.env` hoặc `app.json` của Expo:

```
EXPO_PUBLIC_API_URL=https://YOUR_APP.azurewebsites.net
```

---

## 5. Lưu ý bảo mật

- **Không** commit `appsettings.Production.json` có secret thật vào Git
- Dùng Azure Key Vault cho production (nâng cao)
- JWT SecretKey: dùng `openssl rand -base64 32` để tạo

---

## 6. So sánh với TechStore

| Mục | TechStore | SmartParking |
|-----|-----------|--------------|
| Swagger Production | Bật | Bật (đã cập nhật) |
| CORS | Từ config | Từ config (đã cập nhật) |
| Connection string | Azure SQL | Azure SQL |
| Publish profile | Có | Có (template) |
