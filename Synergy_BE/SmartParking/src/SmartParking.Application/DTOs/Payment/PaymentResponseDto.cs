namespace SmartParking.Application.DTOs.Payment;

public sealed record PaymentResponseDto(
    string PaymentUrl,
    string TxnRef
);
