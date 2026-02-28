# SmartParking - Deployment Security Checklist

> **QUAN TRỌNG**: Trước khi deploy (VNPay, production), kiểm tra toàn bộ mục dưới đây.

---

## 1. Backend - Secrets phải dùng Environment Variables / User Secrets

### Không được commit vào Git:
- `JwtSettings:SecretKey`
- `CloudinarySettings:ApiSecret` (và ApiKey nếu nhạy cảm)
- `VnPay:TmnCode`, `VnPay:HashSecret`
- `SePay:ApiKey`, `SePay:WebhookSecret`, `SePay:Bank:AccountNumber`
- `GoogleOAuth:ClientSecret`
- `EmailSettings:SmtpPassword`
- `ConnectionStrings:DefaultConnection` (chứa password DB)

### Cách cấu hình khi deploy:

**Option A - Environment Variables (Docker, Azure, VPS):**
```bash
# JWT
JwtSettings__SecretKey=your-64-char-secret-key-here

# Cloudinary
CloudinarySettings__CloudName=dogogoyuj
CloudinarySettings__ApiKey=334596456671864
CloudinarySettings__ApiSecret=your-cloudinary-api-secret

# VNPay (Production)
VnPay__TmnCode=your-vnpay-merchant-code
VnPay__HashSecret=your-vnpay-hash-secret
VnPay__ReturnUrl=https://yourdomain.com/api/payments/vnpay-callback

# Database
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;

# Email
EmailSettings__SmtpHost=smtp.gmail.com
EmailSettings__SmtpUser=your-email
EmailSettings__SmtpPassword=your-app-password
```

**Option B - User Secrets (Development local):**
```bash
cd Synergy_BE/SmartParking/src/SmartParking.API
dotnet user-secrets set "CloudinarySettings:ApiSecret" "your-secret"
dotnet user-secrets set "CloudinarySettings:ApiKey" "334596456671864"
dotnet user-secrets set "JwtSettings:SecretKey" "your-jwt-secret-min-32-chars"
dotnet user-secrets set "VnPay:HashSecret" "your-vnpay-secret"
dotnet user-secrets set "VnPay:TmnCode" "your-tmn-code"
# Hoặc dùng file .env (load bằng dotenv) - xem .env.example
```

---

## 2. VNPay Production

- Đổi `PaymentUrl` sang production: `https://vnpayment.vn/paymentv2/vpcpay.html`
- `ReturnUrl` phải là HTTPS và domain thật (VD: `https://api.yourdomain.com/api/payments/vnpay-callback`)
- `TmnCode` và `HashSecret` lấy từ VNPay merchant portal
- **Không** commit TmnCode, HashSecret vào Git

---

## 3. Mobile (Expo)

- `EXPO_PUBLIC_API_URL` - URL backend production (HTTPS)
- `EXPO_PUBLIC_API_URL_WEB` - URL cho web build
- `EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID` - Google OAuth (có thể public)
- **Lưu ý**: Biến `EXPO_PUBLIC_*` được embed vào build - không dùng cho secret!

---

## 4. Web (Vite)

- `VITE_APP_API_URL` - URL backend (HTTPS khi production)
- Không lưu token/secret trong code - dùng backend API

---

## 5. CORS & AllowedHosts

- Production: cập nhật `AllowedHosts` trong appsettings
- CORS: hạn chế `SetIsOriginAllowed` - chỉ cho phép domain thật, không dùng `origin => true` khi production

---

## 6. HTTPS

- Production bắt buộc HTTPS
- JWT cookie nếu dùng: `Secure=true`, `SameSite=Strict`

---

## 7. File đã thêm vào .gitignore

- `appsettings.Development.json` - chứa secrets dev
- `appsettings.Production.json`
- `.env` (tất cả project)

**Nếu appsettings.Development.json đã từng commit:** chạy để xóa khỏi Git (giữ file local):
```bash
git rm --cached "Synergy_BE/SmartParking/src/SmartParking.API/appsettings.Development.json"
git commit -m "Stop tracking appsettings.Development.json (contains secrets)"
```

---

## 8. Kiểm tra trước deploy

- [ ] Đã xóa/ẩn tất cả secret khỏi appsettings.json commit
- [ ] VNPay ReturnUrl đúng domain production
- [ ] Database connection string dùng env var
- [ ] JWT SecretKey >= 32 ký tự, random
- [ ] Cloudinary ApiSecret từ env
- [ ] Email SMTP password từ env
- [ ] appsettings.Development.json không bị commit (đã .gitignore)
