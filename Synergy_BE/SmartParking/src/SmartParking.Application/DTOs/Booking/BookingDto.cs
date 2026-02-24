using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Booking;

/// <summary>
/// Full booking details response DTO
/// </summary>
public sealed record BookingResponseDto(
    Guid BookingId,
    Guid UserId,
    Guid ParkingLotId,
    Guid? VehicleId,
    string ParkingLotName,
    string ParkingLotAddress,
    string? VehiclePlate,
    DateTime BookingTime,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal TotalAmount,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO for creating a new booking
/// </summary>
public sealed record CreateBookingDto(
    [Required(ErrorMessage = "Parking lot ID is required")]
    Guid ParkingLotId,
    
    Guid? VehicleId,
    
    [Required(ErrorMessage = "Start time is required")]
    DateTime StartTime,
    
    [Required(ErrorMessage = "End time is required")]
    DateTime EndTime
)
{
    public bool IsValid()
    {
        return EndTime > StartTime && StartTime >= DateTime.UtcNow;
    }
}

/// <summary>
/// DTO for updating booking time
/// </summary>
public sealed record UpdateBookingDto(
    [Required(ErrorMessage = "Start time is required")]
    DateTime StartTime,
    
    [Required(ErrorMessage = "End time is required")]
    DateTime EndTime
)
{
    public bool IsValid()
    {
        return EndTime > StartTime;
    }
}

/// <summary>
/// DTO for booking list (summary)
/// </summary>
public sealed record BookingListDto(
    Guid BookingId,
    string ParkingLotName,
    string? VehiclePlate,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    decimal TotalAmount,
    DateTime? CheckInTime,
    DateTime? CheckOutTime
);

// Backward compatibility + detail fields for GetById
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
    DateTime CreatedAt,
    DateTime? CheckInTime = null,
    DateTime? CheckOutTime = null
);

/// <summary>
/// DTO for extending booking time
/// </summary>
public sealed record ExtendBookingDto(
    [Required(ErrorMessage = "New end time is required")]
    DateTime NewEndTime
);

/// <summary>
/// DTO for extension request (owner sees when approving)
/// </summary>
public sealed record ExtensionRequestDto(
    Guid ExtensionRequestId,
    Guid BookingId,
    string ParkingLotName,
    string UserFullName,
    string? VehiclePlate,
    DateTime CurrentEndTime,
    DateTime RequestedEndTime,
    int AvailableSlots,
    int TotalCapacity,
    DateTime CreatedAt
);
