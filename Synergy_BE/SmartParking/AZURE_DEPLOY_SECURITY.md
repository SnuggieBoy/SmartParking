# Cấu hình bảo mật khi deploy SmartParking lên Azure

## ⚠️ Trước khi publish

Các file sau **KHÔNG** được đưa lên production (đã cấu hình exclude):
- `appsettings.Development.json` – chứa secret thật (DB, JWT, Google, VnPay, SePay, Email)

File `appsettings.json` chỉ chứa placeholder. **Bắt buộc** cấu hình trong Azure Portal.

---

## Cấu hình trong Azure Portal

Vào **SmartParkingExe** > **Configuration** > **Application settings** và **Connection strings**.

### 1. Connection Strings (bắt buộc)

| Name | Value | Type |
|------|-------|------|
| `DefaultConnection` | `Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=SmartParkingDB;User ID=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` | SQLAzure |

### 2. Application Settings (bắt buộc)

| Name | Value |
|------|-------|
| `JwtSettings__SecretKey` | Chuỗi bí mật ≥ 32 ký tự (dùng cho JWT) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### 3. Application Settings – VNPay (sandbox mới)

**Lưu ý:** Azure dùng **2 dấu gạch dưới** `__` (không phải `_`). Nếu đã tạo `VnPay_TmnCode` thì xóa và tạo lại thành `VnPay__TmnCode`.

| Name | Value |
|------|-------|
| `VnPay__TmnCode` | `0Q5Y0WQ7` |
| `VnPay__HashSecret` | `T7NHMKVRL4N9FEHDMUU1I52ZC9NBZQIM` |
| `VnPay__PaymentUrl` | `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html` |
| `VnPay__ReturnUrl` | `https://smartparkingexe.azurewebsites.net/api/payments/vnpay-callback` |
| `SePay__ApiKey` | API key từ SePay |
| `SePay__WebhookSecret` | Webhook secret từ SePay |
| `SePay__MerchantId` | Merchant ID từ SePay |
| `SePay__Bank__AccountNumber` | Số tài khoản |
| `SePay__Bank__AccountName` | Tên tài khoản |
| `SePay__Urls__ReturnUrl` | `https://smartparkingexe.azurewebsites.net/payment/sepay-return` |
| `SePay__Urls__WebhookUrl` | `https://smartparkingexe.azurewebsites.net/api/payments/sepay/webhook` |

### 4. Deploy lần đầu chưa có VNPay

Nếu chưa cấu hình VNPay, thêm:
| Name | Value |
|------|-------|
| `SKIP_VNPAY_VALIDATION` | `true` |

Sau khi cấu hình xong VNPay, xóa setting này.

---

## Kiểm tra sau khi deploy

1. Vào **Configuration** > **Connection strings** – đảm bảo `DefaultConnection` đã được set.
2. Vào **Application settings** – đảm bảo `JwtSettings__SecretKey` có giá trị thật (≥ 32 ký tự).
3. Bấm **Save** và **Restart** app sau khi thay đổi cấu hình.
