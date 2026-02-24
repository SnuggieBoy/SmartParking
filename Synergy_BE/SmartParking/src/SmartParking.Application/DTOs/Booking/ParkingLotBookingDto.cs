namespace SmartParking.Application.DTOs.Booking;

public sealed record ParkingLotBookingDto(
    Guid BookingId,
    string ParkingLotName,
    string UserFullName,
    string? VehiclePlate,
    string Status,
    DateTime StartTime,
    DateTime EndTime,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    decimal TotalAmount,
    string? PaymentStatus
);

