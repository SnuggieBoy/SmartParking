namespace SmartParking.Application.DTOs.Payment;

public sealed record PaymentStatusDto(
    Guid BookingId,
    decimal Amount,
    string Status,
    string PaymentGateway,
    DateTime? PaidAt
);

