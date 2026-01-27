namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// Response after creating SePay payment
/// Contains QR code and bank transfer information
/// </summary>
public sealed record SePayPaymentResponseDto(
    string OrderId,
    string QrCodeBase64,
    string BankCode,
    string BankAccount,
    string AccountName,
    string TransferContent,
    decimal Amount,
    string Status
);
