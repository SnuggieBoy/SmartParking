using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Location;

/// <summary>
/// Response DTO for parking location with distance
/// </summary>
public sealed record ParkingLocationResponseDto(
    Guid LocationId,
    Guid ParkingLotId,
    string ParkingLotName,
    decimal Latitude,
    decimal Longitude,
    string? Province,
    string? District,
    string? Ward,
    string? Street,
    string? FullAddress,
    double? DistanceInMeters
);

/// <summary>
/// DTO for nearby location search
/// </summary>
public sealed record NearbyLocationRequestDto(
    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    decimal Latitude,
    
    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    decimal Longitude,
    
    [Range(100, 50000, ErrorMessage = "Radius must be between 100 and 50000 meters")]
    double RadiusInMeters = 3000
);

/// <summary>
/// DTO for address-based search
/// </summary>
public sealed record SearchLocationRequestDto(
    string? Province,
    string? District,
    string? Ward,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
);

/// <summary>
/// DTO for creating/updating location
/// </summary>
public sealed record CreateLocationDto(
    [Required(ErrorMessage = "Parking lot ID is required")]
    Guid ParkingLotId,
    
    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    decimal Latitude,
    
    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    decimal Longitude,
    
    string? Province,
    string? District,
    string? Ward,
    string? Street,
    string? Area,
    string? FullAddress
);

public sealed record UpdateLocationDto(
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    decimal? Latitude,
    
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    decimal? Longitude,
    
    string? Province,
    string? District,
    string? Ward,
    string? Street,
    string? Area,
    string? FullAddress
);
