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
    public string? Province { get; set; }

    public string? District { get; set; }

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
