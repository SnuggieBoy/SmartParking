namespace SmartParking.Application.DTOs.Payment;

public sealed record CreatePaymentRequestDto(
    Guid BookingId,
    decimal Amount,
    string Description
);
