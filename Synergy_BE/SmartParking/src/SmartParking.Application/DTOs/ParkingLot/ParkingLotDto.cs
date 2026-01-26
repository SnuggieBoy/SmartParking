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
    int TotalCapacity,
    int AvailableCapacity,
    int CurrentOccupancy,
    decimal PricePerHour,
    string Status,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
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
    decimal PricePerHour
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
    
    bool IsActive
);

/// <summary>
/// DTO for filtering Parking Lots
/// </summary>
public sealed record ParkingLotFilterDto(
    string? SearchTerm,
    bool? IsActive,
    string? Status,
    Guid? OwnerId,
    int Page = 1,
    int PageSize = 10
);

// Backward compatibility alias
public sealed record ParkingLotDto(
    Guid ParkingLotId,
    string Name,
    string Address,
    int TotalCapacity,
    int AvailableCapacity,
    int CurrentOccupancy,
    decimal PricePerHour,
    string Status,
    DateTime? CreatedAt
);
