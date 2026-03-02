namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// Request to create wallet top-up payment (VNPay or SePay)
/// </summary>
public sealed record WalletTopUpRequestDto(
    decimal Amount,
    string Description
);
