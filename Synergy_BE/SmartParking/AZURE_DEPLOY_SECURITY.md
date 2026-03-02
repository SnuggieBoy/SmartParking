# Cấu hình bảo mật khi deploy SmartParking lên Azure

## ⚠️ Trước khi publish

Các file sau **KHÔNG** được đưa lên production (đã cấu hình exclude):
- `appsettings.Development.json` – chứa secret thật (DB, JWT, Google, SePay, Email)

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

### 3. Application Settings – SePay

**Lưu ý:** Azure dùng **2 dấu gạch dưới** `__` (không phải `_`).

| Name | Value |
|------|-------|
| `SePay__ApiKey` | API key từ SePay |
| `SePay__WebhookSecret` | Webhook secret từ SePay |
| `SePay__MerchantId` | Merchant ID từ SePay |
| `SePay__Bank__AccountNumber` | Số tài khoản |
| `SePay__Bank__AccountName` | Tên tài khoản |
| `SePay__Urls__ReturnUrl` | `https://smartparkingexe.azurewebsites.net/payment/sepay-return` |
| `SePay__Urls__WebhookUrl` | `https://smartparkingexe.azurewebsites.net/api/payments/sepay/webhook` |

### 4. SePay Webhook – CẤU HÌNH BẮT BUỘC (ví không cập nhật nếu thiếu)

**Quan trọng:** Chuyển khoản SePay thành công nhưng ví không cập nhật → thường do **chưa cấu hình Webhook trong SePay**.

1. Đăng nhập [my.sepay.vn](https://my.sepay.vn) → **WebHooks** → **+ Add webhooks**
2. Điền:
   - **Name:** SmartParking
   - **Event:** Money in (Tiền vào)
   - **Webhook URL:** `https://smartparkingexe.azurewebsites.net/api/payments/sepay/webhook`
   - **Authentication:** API Key → nhập **Secret Key** (từ Thông tin đơn vị) hoặc **ApiKey**
3. **Payment Code:** Bỏ chọn "Ignore if transaction content does not contain payment code" HOẶC cấu hình Payment Code nhận dạng `SMARTPARKING` / `SP_`
4. **Bank account:** Chọn đúng tài khoản nhận tiền (hoặc để trống nếu nhận mọi TK)
5. Lưu và kiểm tra **WebHooks Log** sau khi CK thử

**Azure App Settings:** `SePay__ApiKey` hoặc `SePay__WebhookSecret` phải khớp với key dùng khi tạo webhook.

---

## Kiểm tra sau khi deploy

1. Vào **Configuration** > **Connection strings** – đảm bảo `DefaultConnection` đã được set.
2. Vào **Application settings** – đảm bảo `JwtSettings__SecretKey` có giá trị thật (≥ 32 ký tự).
3. Bấm **Save** và **Restart** app sau khi thay đổi cấu hình.
