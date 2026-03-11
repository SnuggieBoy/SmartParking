using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.ParkingLot;

/// <summary>
/// Response DTO for Parking Lot
/// </summary>
public sealed record ParkingLotResponseDto(
    Guid ParkingLotId,
    Guid OwnerId,
    string OwnerName,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    decimal? DistanceKm,
    int TotalCapacity,
    int AvailableCapacity,
    int CurrentOccupancy,
    decimal PricePerHour,
    string Status,
    bool IsActive,
    string? RejectReason,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    /// <summary>VN admin code for province - for edit form pre-fill and filtering</summary>
    int? ProvinceCode = null,
    string? Province = null,
    int? WardCode = null,
    string? Ward = null,
    /// <summary>Detailed street/house number - for edit form pre-fill</summary>
    string? Street = null,
    /// <summary>Optional note/area - for edit form pre-fill</summary>
    string? Area = null
);

/// <summary>
/// DTO for creating a new Parking Lot
/// </summary>
public sealed record CreateParkingLotDto(
    [Required(ErrorMessage = "Parking lot name is required")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
    string Name,
    
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(255, ErrorMessage = "Address must not exceed 255 characters")]
    string Address,
    
    [Range(1, 10000, ErrorMessage = "Total capacity must be between 1 and 10000")]
    int TotalCapacity,

    [Range(0.01, 1000000, ErrorMessage = "Price per hour must be between 0.01 and 1000000")]
    decimal PricePerHour,

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    decimal? Latitude,

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    decimal? Longitude,

    /// <summary>
    /// VN admin codes (recommended for exact filtering)
    /// </summary>
    int? ProvinceCode = null,

    /// <summary>
    /// Optional structured location - Vietnam admin units (for filtering).
    /// These are stored into ParkingLocations table (Province/Ward/Street/Area/FullAddress).
    /// </summary>
    [MaxLength(100)]
    string? Province = null,

    int? WardCode = null,

    [MaxLength(100)]
    string? Ward = null,

    [MaxLength(255)]
    string? Street = null,

    /// <summary>
    /// Optional note / area hint (e.g. "near gate A")
    /// </summary>
    [MaxLength(255)]
    string? Area = null,

    /// <summary>
    /// Optional main image URL (from Cloudinary upload)
    /// </summary>
    [MaxLength(500)]
    string? ImageUrl = null
);

/// <summary>
/// DTO for updating an existing Parking Lot
/// </summary>
public sealed record UpdateParkingLotDto(
    [Required(ErrorMessage = "Parking lot name is required")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
    string Name,
    
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(255, ErrorMessage = "Address must not exceed 255 characters")]
    string Address,
    
    [Range(1, 10000, ErrorMessage = "Total capacity must be between 1 and 10000")]
    int TotalCapacity,
    
    [Range(0.01, 1000000, ErrorMessage = "Price per hour must be between 0.01 and 1000000")]
    decimal PricePerHour,

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    decimal? Latitude,

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    decimal? Longitude,

    bool IsActive,

    int? ProvinceCode = null,

    [MaxLength(100)]
    string? Province = null,

    int? WardCode = null,

    [MaxLength(100)]
    string? Ward = null,

    [MaxLength(255)]
    string? Street = null,

    [MaxLength(255)]
    string? Area = null,

    /// <summary>
    /// Optional main image URL (from Cloudinary upload)
    /// </summary>
    [MaxLength(500)]
    string? ImageUrl = null
);

/// <summary>
/// DTO for filtering Parking Lots
/// </summary>
public sealed record ParkingLotFilterDto(
    string? SearchTerm,
    bool? IsActive,
    string? Status,
    int? ProvinceCode,
    string? Province,
    int? WardCode,
    string? Ward,
    Guid? OwnerId,
    int Page = 1,
    int PageSize = 10
);

// Backward compatibility alias
public sealed record ParkingLotDto(
    Guid ParkingLotId,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    int TotalCapacity,
    int AvailableCapacity,
    int CurrentOccupancy,
    decimal PricePerHour,
    string Status,
    DateTime? CreatedAt
);
