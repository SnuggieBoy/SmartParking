namespace SmartParking.Application.DTOs.Booking;

public sealed record BookingCheckInResponseDto(
    Guid BookingId,
    string Status,
    DateTime CheckInTime
);

public sealed record BookingCheckOutResponseDto(
    Guid BookingId,
    string Status,
    DateTime CheckOutTime,
    decimal TotalAmount
);

