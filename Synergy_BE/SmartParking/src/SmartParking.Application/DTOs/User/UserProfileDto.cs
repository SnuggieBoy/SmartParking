namespace SmartParking.Application.DTOs.User;

/// <summary>
/// User profile DTO (for user viewing their own profile)
/// </summary>
public sealed record UserProfileDto(
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string? AvatarUrl,
    string RoleName,
    bool EmailConfirmed,
    DateTime? CreatedAt,
    // Statistics
    int TotalBookings,
    int CompletedBookings,
    int TotalVehicles,
    int TotalParkingLots // For owners
);

/// <summary>
/// DTO for user updating their own profile
/// </summary>
public sealed record UpdateUserProfileDto(
    string? FullName,
    string? Phone,
    string? AvatarUrl
);

/// <summary>
/// DTO for booking history (completed bookings)
/// </summary>
public sealed record BookingHistoryDto(
    Guid BookingId,
    string ParkingLotName,
    string ParkingLotAddress,
    string? VehiclePlate,
    DateTime StartTime,
    DateTime EndTime,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    decimal TotalAmount,
    string PaymentStatus,
    DateTime CompletedAt
);

/// <summary>
/// DTO for payment history
/// </summary>
public sealed record PaymentHistoryDto(
    Guid PaymentId,
    Guid? BookingId,
    string? ParkingLotName,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    string TransactionRef,
    DateTime CreatedAt,
    DateTime? PaidAt,
    // For owner subscription payments
    string? PaymentPurpose
);

/// <summary>
/// DTO for invoice/receipt
/// </summary>
public sealed record InvoiceDto(
    // Invoice info
    string InvoiceNumber,
    DateTime InvoiceDate,
    
    // Customer info
    Guid UserId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    
    // Booking info
    Guid BookingId,
    string ParkingLotName,
    string ParkingLotAddress,
    string? VehiclePlate,
    DateTime StartTime,
    DateTime EndTime,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    int DurationMinutes,
    
    // Payment info
    decimal PricePerHour,
    decimal SubTotal,
    decimal Commission,
    decimal TotalAmount,
    string PaymentMethod,
    string PaymentStatus,
    string TransactionRef,
    DateTime? PaidAt,
    
    // Owner info
    string OwnerName,
    string? OwnerBankAccount,
    string? OwnerBankName
);
