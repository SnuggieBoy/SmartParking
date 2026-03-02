namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// Response after creating SePay payment
/// Contains QR code (URL VietQR chuẩn SePay) and bank transfer information
/// </summary>
public sealed record SePayPaymentResponseDto(
    string OrderId,
    string? QrCodeBase64,      // Base64 PNG (fallback, khi không dùng SePay URL)
    string? QrCodeUrl,         // URL VietQR chuẩn từ qr.sepay.vn - app ngân hàng quét được
    string BankCode,
    string BankAccount,
    string AccountName,
    string TransferContent,
    decimal Amount,
    string Status
);
