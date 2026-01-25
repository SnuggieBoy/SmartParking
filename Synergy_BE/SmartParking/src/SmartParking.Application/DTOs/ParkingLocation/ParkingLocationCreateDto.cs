namespace SmartParking.Application.DTOs.ParkingLocation;

/// <summary>
/// DTO for creating a new parking location.
/// Used by Admin to add new locations to the map.
/// </summary>
public sealed record ParkingLocationCreateDto(
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    string Province,
    string District,
    string Ward,
    string? Street,
    string? Area,
    string? FullAddress,
    int TotalSlots,
    int AvailableSlots,
    decimal PricePerHour
);
