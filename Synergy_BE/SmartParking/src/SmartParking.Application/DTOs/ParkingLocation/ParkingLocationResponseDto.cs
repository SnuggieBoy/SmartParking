namespace SmartParking.Application.DTOs.ParkingLocation;

/// <summary>
/// DTO for parking location response.
/// Includes optional distance field for nearby search results.
/// </summary>
public sealed record ParkingLocationResponseDto(
    Guid Id,
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    string Province,
    string District,
    string Ward,
    string? FullAddress,
    int AvailableSlots,
    int TotalSlots,
    decimal PricePerHour,
    bool IsActive,
    double? Distance = null  // Distance in meters (only populated for nearby search)
);
