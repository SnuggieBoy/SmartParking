# Checklist Deploy: SePay + Nạp tiền ví (Wallet Top-up)

> Kiểm tra trước khi deploy các tính năng SePay và nạp tiền ví qua VNPay/SePay.

---

## 1. Database – **KHÔNG cần migration mới**

Các thay đổi SePay + WalletTopUp **không yêu cầu** thêm cột hay bảng mới:

| Bảng | Trạng thái |
|------|------------|
| **PaymentTransactions** | Đã có `BookingId` NULL, `OwnerUpgradeRequestId` NULL, `PaymentType` (NVARCHAR) – dùng giá trị `"WalletTopUp"` |
| **WalletTransactions** | Đã có `Type = "TopUp"` – không đổi |
| **UserWallets** | Không đổi |

**Nếu DB chưa chạy migration Subscription:** Chạy trước:
```sql
-- Database/20260203_UpdatePaymentTransactionForSubscriptions.sql
```

---

## 2. Cấu hình Production (appsettings / Environment Variables)

### 2.1 SePay (bắt buộc nếu dùng SePay)

| Key | Mô tả | Ví dụ |
|-----|-------|-------|
| `SePay:Enabled` | Bật SePay | `true` |
| `SePay:MerchantId` | Mã đơn vị | `SP-LIVE-LT7B9BA4` |
| `SePay:WebhookSecret` | Secret Key (API Key) | `spsk_live_xxx` |
| `SePay:ApiKey` | Cùng với WebhookSecret | `spsk_live_xxx` |
| `SePay:Bank:Code` | Mã ngân hàng | `MB` |
| `SePay:Bank:AccountNumber` | Số tài khoản | `0986515253` |
| `SePay:Bank:AccountName` | Tên chủ tài khoản | `LE TRONG HIEU` |

**SePay Dashboard – Webhook URL (bắt buộc):**
```
https://YOUR_APP.azurewebsites.net/api/payments/sepay/webhook
```

---

## 3. Các API mới

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| POST | `/api/wallet/topup-sepay` | Tạo thông tin chuyển khoản SePay để nạp ví |

---

## 4. Mobile / Web – URL cần cập nhật

- **API Base URL:** Trỏ về `https://YOUR_APP.azurewebsites.net`
- **Deep link (mobile):** `synergy://payment-result` hoặc custom scheme

---

## 5. Kiểm tra sau deploy

1. **SePay Webhook:** Gửi test từ SePay Dashboard (nếu có) hoặc chuyển khoản thử.
2. **Nạp ví SePay:** App → Ví → Nạp tiền → SePay → Chuyển khoản → Kiểm tra webhook log.
3. **Nạp ví VNPay:** App → Ví → Nạp tiền → VNPay → Thanh toán → Kiểm tra callback.
4. **Booking SePay:** Đặt chỗ → Thanh toán SePay → Chuyển khoản → Kiểm tra booking chuyển Confirmed.

---

*Cập nhật: 2026-02-23*
