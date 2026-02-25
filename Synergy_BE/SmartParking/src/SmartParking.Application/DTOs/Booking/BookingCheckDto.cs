namespace SmartParking.Application.DTOs.Booking;

/// <summary>Preview checkout: hiển thị trước khi user/owner xác nhận checkout sớm.</summary>
public sealed record BookingCheckOutPreviewDto(
    Guid BookingId,
    decimal PaidAmount,
    decimal ActualCharge,
    decimal RefundAmount,
    bool IsEarlyCheckout,
    double ActualHours,
    string Message
);

public sealed record BookingCheckInResponseDto(
    Guid BookingId,
    string Status,
    DateTime CheckInTime
);

public sealed record BookingCheckOutResponseDto(
    Guid BookingId,
    string Status,
    DateTime CheckOutTime,
    decimal TotalAmount,
    decimal RefundAmount = 0
);

