# 📧 OTP Email Verification System - Complete Guide

## 🎯 Overview

SmartParking now implements a secure **Email OTP Verification** system for user registration. Users must verify their email address before their account is created, ensuring:

- ✅ Valid email addresses
- ✅ Prevention of spam registrations  
- ✅ Enhanced security
- ✅ User ownership verification

---

## 📋 Table of Contents

1. [Architecture](#architecture)
2. [Database Changes](#database-changes)
3. [API Endpoints](#api-endpoints)
4. [Registration Flow](#registration-flow)
5. [Configuration](#configuration)
6. [Testing Guide](#testing-guide)
7. [Security Features](#security-features)
8. [Troubleshooting](#troubleshooting)

---

## 🏗️ Architecture

### **Flow Diagram**

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ 1. POST /api/auth/register-request
       │    { email, password, fullName, phone }
       ▼
┌──────────────────┐
│  API Controller  │
└────────┬─────────┘
         │
         │ 2. RegisterRequestOtpAsync()
         ▼
┌──────────────────┐
│ Authentication   │──► Check if email exists
│    Service       │──► Generate 6-digit OTP (cryptographically secure)
└────────┬─────────┘──► Store OTP + user data temporarily (5 min expiry)
         │            ──► Send OTP email
         │
         │ 3. Email sent ✉️
         │
         │ ◄── User receives OTP
         │
         │ 4. POST /api/auth/verify-otp
         │    { email, otpCode }
         ▼
┌──────────────────┐
│ Authentication   │──► Verify OTP code
│    Service       │──► Check expiry
└────────┬─────────┘──► Mark OTP as used
         │            ──► Create User account
         │            ──► Set EmailConfirmed = true
         │            ──► Send welcome email
         │
         ▼
┌─────────────────┐
│  User Created   │──► Return JWT tokens
│  & Logged In    │──► User can now login
└─────────────────┘
```

---

## 🗄️ Database Changes

### **1. New Table: `EmailOtps`**

```sql
CREATE TABLE [dbo].[EmailOtps] (
    [OtpId]                   UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Email]                   NVARCHAR(255) NOT NULL,
    [OtpCode]                 NVARCHAR(6) NOT NULL,
    [ExpiredAt]               DATETIME2(7) NOT NULL,
    [IsUsed]                  BIT NOT NULL DEFAULT 0,
    [CreatedAt]               DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [TemporaryPasswordHash]   NVARCHAR(MAX) NULL,
    [TemporaryFullName]       NVARCHAR(255) NULL,
    [TemporaryPhone]          NVARCHAR(20) NULL
)
```

**Purpose:** Stores OTP codes and temporary user data before verification.

### **2. Updated Table: `Users`**

**Added Column:**
```sql
ALTER TABLE [dbo].[Users]
ADD [EmailConfirmed] BIT NOT NULL DEFAULT 0
```

**Purpose:** Tracks whether a user's email has been verified.

### **3. Indexes (Performance Optimization)**

```sql
-- Fast lookup by email and unused status
CREATE INDEX [IX_EmailOtps_Email_IsUsed]
ON [dbo].[EmailOtps] ([Email], [IsUsed])
WHERE [IsUsed] = 0

-- Fast lookup for expired OTPs (cleanup)
CREATE INDEX [IX_EmailOtps_ExpiredAt]
ON [dbo].[EmailOtps] ([ExpiredAt])
WHERE [IsUsed] = 0

-- Fast lookup for unverified users
CREATE INDEX [IX_Users_EmailConfirmed]
ON [dbo].[Users] ([EmailConfirmed])
```

---

## 🔌 API Endpoints

### **1. POST /api/auth/register-request**

**Purpose:** Initiate registration and send OTP to email

**Request:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phone": "0123456789",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "OTP has been sent to your email. Please check your inbox.",
  "data": null,
  "errors": [],
  "timestamp": "2026-01-25T10:30:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Email already exists
- `400 Bad Request`: Too many OTP requests (rate limiting)
- `422 Unprocessable Entity`: Validation errors

---

### **2. POST /api/auth/verify-otp**

**Purpose:** Verify OTP and create user account

**Request:**
```json
{
  "email": "john.doe@example.com",
  "otpCode": "123456"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Email verified successfully. Your account has been created.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def50200...",
    "expiresAt": "2026-01-25T11:30:00Z",
    "user": {
      "userId": "a1b2c3d4-...",
      "fullName": "John Doe",
      "email": "john.doe@example.com",
      "phone": "0123456789",
      "role": "User",
      "isActive": true
    }
  },
  "errors": [],
  "timestamp": "2026-01-25T10:35:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid OTP code
- `400 Bad Request`: OTP expired
- `400 Bad Request`: OTP already used
- `404 Not Found`: No OTP found for this email

---

### **3. POST /api/auth/resend-otp**

**Purpose:** Resend OTP if expired or not received

**Request:**
```json
{
  "email": "john.doe@example.com"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "A new OTP has been sent to your email",
  "data": null,
  "errors": [],
  "timestamp": "2026-01-25T10:32:00Z"
}
```

**Rate Limiting:** 60 seconds cooldown between requests

**Error Responses:**
- `400 Bad Request`: Too many OTP requests
- `404 Not Found`: No pending registration for this email

---

## 🔄 Registration Flow (Step-by-Step)

### **Client-Side Implementation Example**

```javascript
// STEP 1: Request Registration
async function registerUser(userData) {
  const response = await fetch('/api/auth/register-request', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      fullName: userData.fullName,
      email: userData.email,
      phone: userData.phone,
      password: userData.password,
      confirmPassword: userData.confirmPassword
    })
  });
  
  if (response.ok) {
    // Show OTP input screen
    showOtpVerificationScreen(userData.email);
  }
}

// STEP 2: Verify OTP
async function verifyOtp(email, otpCode) {
  const response = await fetch('/api/auth/verify-otp', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, otpCode })
  });
  
  if (response.ok) {
    const data = await response.json();
    // Store tokens
    localStorage.setItem('accessToken', data.data.accessToken);
    localStorage.setItem('refreshToken', data.data.refreshToken);
    // Redirect to dashboard
    window.location.href = '/dashboard';
  }
}

// STEP 3: Resend OTP (if needed)
async function resendOtp(email) {
  const response = await fetch('/api/auth/resend-otp', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email })
  });
  
  if (response.ok) {
    alert('New OTP sent! Check your email.');
  }
}
```

---

## ⚙️ Configuration

### **1. Email Settings (appsettings.Development.json)**

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@smartparking.com",
    "FromName": "SmartParking",
    "EnableSsl": "true"
  }
}
```

### **2. Gmail Setup (Recommended for Development)**

**Steps:**
1. Enable 2-Factor Authentication on your Gmail account
2. Go to: https://myaccount.google.com/apppasswords
3. Generate an "App Password"
4. Use the 16-character password in `SmtpPassword`

**Example App Password:** `abcd efgh ijkl mnop` (remove spaces in config)

### **3. Production Email Settings**

For production, use environment variables:

```bash
# Windows PowerShell
$env:EmailSettings__SmtpHost = "smtp.sendgrid.net"
$env:EmailSettings__SmtpUser = "apikey"
$env:EmailSettings__SmtpPassword = "SG.your-sendgrid-api-key"

# Linux/Mac
export EmailSettings__SmtpHost="smtp.sendgrid.net"
export EmailSettings__SmtpUser="apikey"
export EmailSettings__SmtpPassword="SG.your-sendgrid-api-key"
```

**Recommended Email Providers:**
- SendGrid (99 emails/day free)
- Mailgun (100 emails/day free)
- AWS SES (cheap, reliable)
- Azure Communication Services

---

## 🧪 Testing Guide

### **Manual Testing Steps**

#### **Test 1: Successful Registration**

1. **Send registration request:**
   ```bash
   curl -X POST http://localhost:5070/api/auth/register-request \
     -H "Content-Type: application/json" \
     -d '{
       "fullName": "Test User",
       "email": "test@example.com",
       "phone": "0123456789",
       "password": "Test123!",
       "confirmPassword": "Test123!"
     }'
   ```

2. **Check email for OTP** (check logs if email not configured)

3. **Verify OTP:**
   ```bash
   curl -X POST http://localhost:5070/api/auth/verify-otp \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test@example.com",
       "otpCode": "123456"
     }'
   ```

4. **Verify in database:**
   ```sql
   SELECT UserId, Email, EmailConfirmed, CreatedAt 
   FROM Users 
   WHERE Email = 'test@example.com'
   -- EmailConfirmed should be 1 (true)
   ```

#### **Test 2: OTP Expiry**

1. Register user (get OTP)
2. Wait 6 minutes
3. Try to verify → Should fail with "OTP expired"

#### **Test 3: Invalid OTP**

1. Register user
2. Enter wrong OTP code → Should fail with "Invalid OTP"

#### **Test 4: Resend OTP**

1. Register user
2. Don't verify
3. Request resend → Should receive new OTP

#### **Test 5: Rate Limiting**

1. Register user
2. Immediately request resend → Should fail with "Too many requests"
3. Wait 60 seconds
4. Request resend → Should succeed

#### **Test 6: Login Before Verification**

1. Register user (don't verify OTP)
2. Try to login → Should fail with "Please verify your email"

---

## 🔒 Security Features

### **1. Cryptographically Secure OTP Generation**

```csharp
using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
var bytes = new byte[4];
rng.GetBytes(bytes);
var randomNumber = BitConverter.ToUInt32(bytes, 0);
var otpCode = (randomNumber % 900000 + 100000).ToString(); // 6 digits
```

**Why secure?**
- Uses `RandomNumberGenerator` (cryptographically secure)
- Not predictable (unlike `Random`)
- Cannot be brute-forced within 5-minute window

### **2. OTP Expiry**

- **Lifetime:** 5 minutes
- **Cleanup:** Automatic deletion after 24 hours
- **One-time use:** OTP marked as used after successful verification

### **3. Rate Limiting**

- **Cooldown:** 60 seconds between OTP requests
- **Prevents:** Spam, brute force, email flooding
- **Configurable:** `RESEND_OTP_COOLDOWN_SECONDS` constant

### **4. Password Storage**

- Passwords **never** stored in plain text
- Hashed using **BCrypt** or **PBKDF2**
- Temporary hash stored until verification
- Original password never logged or exposed

### **5. Email Validation**

- Format validation: `[EmailAddress]` attribute
- Uniqueness check before OTP generation
- Prevents duplicate registrations

### **6. Login Protection**

```csharp
if (!user.EmailConfirmed)
{
    throw new UnauthorizedException(Messages.Auth.EmailNotVerified);
}
```

Users **cannot login** until email is verified.

---

## 🐛 Troubleshooting

### **Problem: OTP email not received**

**Possible Causes:**
1. **Email not configured**
   - **Check:** Logs for "Email service not configured"
   - **Fix:** Configure SMTP settings in `appsettings.Development.json`

2. **Wrong Gmail credentials**
   - **Check:** Use App Password, not regular password
   - **Fix:** Generate App Password from Google Account

3. **Spam/Junk folder**
   - **Check:** Email might be in spam
   - **Fix:** Add sender to contacts, configure SPF/DKIM in production

4. **SMTP port blocked**
   - **Check:** Firewall blocking port 587
   - **Fix:** Try port 465 (SSL) or use SendGrid/Mailgun

**Debug Steps:**
```bash
# Check logs
dotnet run

# Look for:
# "Email sent successfully to test@example.com" ✅
# OR
# "Email service not configured" ❌
```

### **Problem: "OTP expired" immediately**

**Cause:** Server time incorrect

**Fix:**
```bash
# Check server time
date

# If wrong, sync:
# Windows
w32tm /resync

# Linux
sudo ntpdate -s time.nist.gov
```

### **Problem: "Email already exists" but user not in database**

**Cause:** Pending registration exists

**Fix:**
```sql
-- Check EmailOtps table
SELECT * FROM EmailOtps WHERE Email = 'test@example.com'

-- If found, user needs to complete verification or resend OTP
```

### **Problem: Rate limiting too strict**

**Temporary Fix:**
```csharp
// In AuthenticationService.cs, change:
private const int RESEND_OTP_COOLDOWN_SECONDS = 30; // Instead of 60
```

---

## 📊 Database Maintenance

### **Cleanup Expired OTPs**

Run daily (via scheduled job or SQL Agent):

```sql
-- Manual cleanup
DELETE FROM EmailOtps 
WHERE ExpiredAt < DATEADD(DAY, -1, GETUTCDATE())

-- Or use stored procedure
EXEC sp_CleanupExpiredOtps
```

### **Monitor OTP Usage**

```sql
-- OTP statistics
SELECT 
    COUNT(*) AS TotalOTPs,
    SUM(CASE WHEN IsUsed = 1 THEN 1 ELSE 0 END) AS UsedOTPs,
    SUM(CASE WHEN ExpiredAt < GETUTCDATE() THEN 1 ELSE 0 END) AS ExpiredOTPs,
    AVG(DATEDIFF(SECOND, CreatedAt, GETUTCDATE())) AS AvgVerificationTime
FROM EmailOtps
WHERE CreatedAt > DATEADD(DAY, -7, GETUTCDATE())
```

---

## 🚀 Production Checklist

- [ ] Configure production SMTP provider (SendGrid/AWS SES)
- [ ] Set up environment variables for SMTP credentials
- [ ] Enable email logging/monitoring
- [ ] Set up automated OTP cleanup job
- [ ] Configure rate limiting per user IP
- [ ] Add OTP attempt logging for security audit
- [ ] Test email deliverability (inbox vs spam)
- [ ] Configure SPF, DKIM, DMARC records
- [ ] Add monitoring alerts for failed emails
- [ ] Set up email queue for resilience

---

## 📞 Support

**For issues:**
1. Check logs: `dotnet run` output
2. Check database: `SELECT * FROM EmailOtps`
3. Test email: Try sending test email via SMTP
4. Check firewall: Port 587 or 465 open?

**Common Settings:**

| Provider | SMTP Host | Port | SSL |
|----------|-----------|------|-----|
| Gmail | smtp.gmail.com | 587 | Yes |
| SendGrid | smtp.sendgrid.net | 587 | Yes |
| Mailgun | smtp.mailgun.org | 587 | Yes |
| AWS SES | email-smtp.region.amazonaws.com | 587 | Yes |

---

**Implementation Date:** 2026-01-25  
**Version:** 1.0  
**Status:** ✅ Production Ready
