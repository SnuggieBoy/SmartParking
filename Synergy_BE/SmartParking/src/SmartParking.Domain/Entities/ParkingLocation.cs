#nullable enable
using System;

namespace SmartParking.Domain.Entities;

/// <summary>
/// Parking location entity for geospatial search and map integration
/// </summary>
public partial class ParkingLocation
{
    public Guid LocationId { get; set; }

    public Guid ParkingLotId { get; set; }

    // Geographic coordinates
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    // Address components
    /// <summary>
    /// Vietnam Province/City code (from provinces.open-api.vn), optional.
    /// </summary>
    public int? ProvinceCode { get; set; }

    public string? Province { get; set; }

    // Legacy (kept for backward compatibility)
    public string? District { get; set; }

    /// <summary>
    /// Vietnam Ward/Commune code (from provinces.open-api.vn), optional.
    /// </summary>
    public int? WardCode { get; set; }

    public string? Ward { get; set; }

    public string? Street { get; set; }

    public string? Area { get; set; }

    public string? FullAddress { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    // Navigation property
    public virtual ParkingLot ParkingLot { get; set; } = null!;
}
