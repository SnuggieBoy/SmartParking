namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// Request to create SePay payment
/// </summary>
public sealed record CreateSePayPaymentDto(
    Guid BookingId,
    decimal Amount,
    string Description
);
