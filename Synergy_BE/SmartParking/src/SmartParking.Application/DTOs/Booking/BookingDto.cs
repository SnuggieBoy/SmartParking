namespace SmartParking.Application.DTOs.Booking;

public sealed record BookingDto(
    Guid BookingId,
    Guid UserId,
    Guid ParkingLotId,
    Guid? VehicleId,
    string ParkingLotName,
    string? VehiclePlate,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt
);

public sealed record CreateBookingDto(
    Guid ParkingLotId,
    Guid? VehicleId,
    DateTime StartTime,
    DateTime EndTime
);

public sealed record UpdateBookingDto(
    DateTime StartTime,
    DateTime EndTime
);

public sealed record BookingListDto(
    Guid BookingId,
    string ParkingLotName,
    string? VehiclePlate,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal TotalAmount
);
