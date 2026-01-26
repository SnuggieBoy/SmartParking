# 🚀 QUICK START: OTP Email Verification

## ✅ Implementation Complete!

**Status:** ✅ Build Successful (0 Warnings, 0 Errors)

---

## 📦 What's Been Implemented

### **1. Database Changes**
- ✅ New table: `EmailOtps` (stores OTP codes)
- ✅ New column: `Users.EmailConfirmed` 
- ✅ Indexes for performance
- ✅ Cleanup stored procedure

### **2. Backend Components**
- ✅ `EmailOtp` entity
- ✅ `IEmailOtpRepository` + implementation
- ✅ `IEmailService` + SMTP email service
- ✅ OTP generation (cryptographically secure)
- ✅ Registration flow with OTP verification
- ✅ Rate limiting (60s cooldown)
- ✅ Auto-cleanup of expired OTPs

### **3. API Endpoints**
- ✅ `POST /api/auth/register-request` - Send OTP
- ✅ `POST /api/auth/verify-otp` - Verify and create account
- ✅ `POST /api/auth/resend-otp` - Resend OTP

### **4. Security Features**
- ✅ Cryptographically secure OTP (6 digits)
- ✅ 5-minute expiry
- ✅ One-time use only
- ✅ Rate limiting
- ✅ Email confirmation required for login
- ✅ Password hashing (never stored plain text)

---

## 🎯 Quick Setup (3 Steps)

### **STEP 1: Run SQL Migration**

```sql
-- In SSMS, execute:
USE SmartParkingDB
GO

-- Run the migration script:
```

**File:** `Database/Migration_AddEmailOtpVerification.sql`

**Expected Output:**
```
✓ EmailConfirmed column added successfully
✓ EmailOtps table created successfully
✓ Index IX_Users_EmailConfirmed created
✓ Index IX_EmailOtps_Email_IsUsed created
MIGRATION COMPLETED SUCCESSFULLY!
```

**Verify:**
```sql
-- Check table exists
SELECT * FROM EmailOtps
-- Should return 0 rows (empty table)

-- Check Users column
SELECT TOP 1 UserId, Email, EmailConfirmed FROM Users
-- EmailConfirmed column should exist
```

---

### **STEP 2: Configure Email (Optional for Testing)**

#### **Option A: Gmail (Recommended for Development)**

**`appsettings.Development.json`:**
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password-here",
    "FromEmail": "noreply@smartparking.com",
    "FromName": "SmartParking",
    "EnableSsl": "true"
  }
}
```

**Get Gmail App Password:**
1. Enable 2FA: https://myaccount.google.com/security
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Copy 16-character password (remove spaces)

#### **Option B: Skip Email (Check Logs)**

Leave `SmtpHost` empty. OTP will be logged to console:

```
Email service not configured. Email would be sent to: test@example.com
Email Subject: Verify Your SmartParking Account
SMTP settings missing. Configure in appsettings.json
```

**Find OTP in database:**
```sql
SELECT Email, OtpCode, ExpiredAt, CreatedAt
FROM EmailOtps
WHERE Email = 'test@example.com'
ORDER BY CreatedAt DESC
```

---

### **STEP 3: Start Application**

```bash
cd "E:\FPT UNIVERSITY\CN8\EXE201_BE\Synergy_BE\SmartParking\src\SmartParking.API"
dotnet run
```

**Expected Output:**
```
✅ Now listening on: https://localhost:7278
✅ Now listening on: http://localhost:5070
Application started. Press Ctrl+C to shut down.
```

**Open Swagger:**
```
https://localhost:7278/swagger
```

---

## 🧪 Test the Flow

### **Test 1: Register with OTP**

#### **Step 1: Request Registration**

**Endpoint:** `POST /api/auth/register-request`

**Request:**
```json
{
  "fullName": "Test User",
  "email": "test@smartparking.com",
  "phone": "0123456789",
  "password": "Test123!",
  "confirmPassword": "Test123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "OTP has been sent to your email. Please check your inbox.",
  "data": null
}
```

**Check Email or Console:**
- **With Email:** Check inbox for OTP (6 digits)
- **Without Email:** Check console logs or database:

```sql
SELECT OtpCode FROM EmailOtps 
WHERE Email = 'test@smartparking.com' 
AND IsUsed = 0
ORDER BY CreatedAt DESC
```

#### **Step 2: Verify OTP**

**Endpoint:** `POST /api/auth/verify-otp`

**Request:**
```json
{
  "email": "test@smartparking.com",
  "otpCode": "123456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Email verified successfully. Your account has been created.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2026-01-25T12:00:00Z",
    "user": {
      "userId": "...",
      "fullName": "Test User",
      "email": "test@smartparking.com",
      "role": "User",
      "isActive": true
    }
  }
}
```

**Verify in Database:**
```sql
-- User should exist with EmailConfirmed = 1
SELECT UserId, Email, EmailConfirmed, CreatedAt
FROM Users
WHERE Email = 'test@smartparking.com'

-- OTP should be marked as used
SELECT Email, OtpCode, IsUsed, ExpiredAt
FROM EmailOtps
WHERE Email = 'test@smartparking.com'
```

#### **Step 3: Login**

**Endpoint:** `POST /api/auth/login`

```json
{
  "email": "test@smartparking.com",
  "password": "Test123!"
}
```

**Should succeed!** ✅

---

### **Test 2: Resend OTP**

**Endpoint:** `POST /api/auth/resend-otp`

```json
{
  "email": "test@smartparking.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "A new OTP has been sent to your email"
}
```

**Note:** 60-second cooldown between requests.

---

### **Test 3: Error Cases**

#### **Wrong OTP:**
```json
{
  "email": "test@smartparking.com",
  "otpCode": "999999"
}
```
**Response:** `400 Bad Request - "Invalid OTP code"`

#### **Expired OTP:**
Wait 6 minutes, then verify
**Response:** `400 Bad Request - "OTP code has expired"`

#### **Login Before Verification:**
Register but don't verify, try to login
**Response:** `401 Unauthorized - "Please verify your email before logging in"`

---

## 📊 Monitor in Database

### **Check Pending Registrations:**
```sql
SELECT 
    Email,
    OtpCode,
    ExpiredAt,
    IsUsed,
    DATEDIFF(SECOND, CreatedAt, GETUTCDATE()) AS SecondsAgo
FROM EmailOtps
WHERE IsUsed = 0
ORDER BY CreatedAt DESC
```

### **Check Verified Users:**
```sql
SELECT 
    FullName,
    Email,
    EmailConfirmed,
    CreatedAt
FROM Users
WHERE EmailConfirmed = 1
ORDER BY CreatedAt DESC
```

### **OTP Statistics:**
```sql
SELECT 
    COUNT(*) AS TotalOTPs,
    SUM(CASE WHEN IsUsed = 1 THEN 1 ELSE 0 END) AS VerifiedOTPs,
    SUM(CASE WHEN IsUsed = 0 THEN 1 ELSE 0 END) AS PendingOTPs,
    AVG(DATEDIFF(MINUTE, CreatedAt, ExpiredAt)) AS AvgExpiryMinutes
FROM EmailOtps
```

---

## 📚 Full Documentation

**Detailed Guide:** `docs/OTP_EMAIL_VERIFICATION_GUIDE.md`

Includes:
- Complete architecture diagram
- Security features explained
- Production deployment checklist
- Troubleshooting guide
- Email provider comparison
- Rate limiting configuration
- And more!

---

## 🎉 You're Ready!

### **New Registration Flow:**
```
User → Register Request → Receive OTP → Verify OTP → Account Created ✅
```

### **Old Flow (Deprecated):**
```
User → Register → Account Created ❌ (No verification)
```

### **API Endpoints Available:**
- ✅ `/api/auth/register-request` - NEW: Send OTP
- ✅ `/api/auth/verify-otp` - NEW: Verify OTP
- ✅ `/api/auth/resend-otp` - NEW: Resend OTP
- ℹ️ `/api/auth/register` - DEPRECATED (hidden from Swagger)
- ✅ `/api/auth/login` - Updated (checks EmailConfirmed)
- ✅ `/api/auth/google` - Works as before (auto-verified)
- ✅ `/api/auth/change-password` - Works as before

---

## ⚠️ Important Notes

1. **Email Required:** Users MUST verify email to login
2. **Google OAuth:** Auto-verified (skips OTP)
3. **Rate Limiting:** 60s cooldown between OTP requests
4. **OTP Expiry:** 5 minutes
5. **One-Time Use:** Each OTP can only be used once
6. **Cleanup:** Expired OTPs auto-deleted after 24 hours

---

## 🔧 Troubleshooting

### **"Email service not configured"**
→ Normal in development if SMTP not configured
→ Check logs for OTP code or query database

### **"Too many OTP requests"**
→ Wait 60 seconds between requests
→ Or adjust `RESEND_OTP_COOLDOWN_SECONDS` in code

### **"OTP expired"**
→ Request new OTP via `/api/auth/resend-otp`

### **Build Errors**
→ Run: `dotnet clean && dotnet build`
→ Check all TODOs completed (all marked ✅)

---

**Implementation Date:** 2026-01-25  
**Build Status:** ✅ **0 Errors, 0 Warnings**  
**Status:** 🚀 **Ready for Production Testing**
