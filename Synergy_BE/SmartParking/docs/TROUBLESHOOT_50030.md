# Xử lý lỗi 500.30 - ASP.NET Core app failed to start

## Bước 1: Bật hiển thị lỗi chi tiết

Vào **Azure Portal** → **SmartParkingExe** → **Configuration** → **App settings** → **+ Add**:

| Name | Value |
|------|-------|
| `ASPNETCORE_DETAILEDERRORS` | `true` |

**Save** → **Restart** app → Refresh trang. Lỗi chi tiết sẽ hiện trên màn hình.

---

## Bước 2: Kiểm tra các nguyên nhân thường gặp

### 1. Connection string chưa đúng

- **Tab Connection strings** (không phải App settings): thêm `DefaultConnection`
- **Type**: SQLAzure
- **Value**: `Server=tcp:techstore-prm393.database.windows.net,1433;Initial Catalog=SmartParkingDB;User ID=prm393;Password=Passmon393;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

### 2. JWT Secret chưa cấu hình

- **Tab App settings**: thêm `JwtSettings__SecretKey` (2 dấu gạch dưới `__`)
- **Value**: chuỗi ≥ 32 ký tự (ví dụ từ appsettings.Development.json)

### 3. VNPay validation fail (Production)

Nếu chưa cấu hình VNPay, thêm:

| Name | Value |
|------|-------|
| `SKIP_VNPAY_VALIDATION` | `true` |

### 4. ASPNETCORE_ENVIRONMENT

Đảm bảo có: `ASPNETCORE_ENVIRONMENT` = `Production`

---

## Bước 3: Xem log chi tiết (nếu vẫn lỗi)

1. Vào **Advanced Tools** (Kudu): `https://smartparkingexe.scm.azurewebsites.net`
2. **Debug console** → **CMD**
3. `cd LogFiles` → xem file log mới nhất
4. Hoặc **Log stream** trong Azure Portal để xem log real-time
