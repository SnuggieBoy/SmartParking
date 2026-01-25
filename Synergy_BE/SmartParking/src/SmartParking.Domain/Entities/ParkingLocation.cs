using SmartParking.Domain.Interfaces;

namespace SmartParking.Domain.Entities;

/// <summary>
/// Parking location entity for map-based parking lot discovery.
/// Stores geolocation data and address details for spatial queries.
/// </summary>
public partial class ParkingLocation : IAuditable, ISoftDeletable
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Latitude coordinate (valid range: -90 to 90)
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate (valid range: -180 to 180)
    /// </summary>
    public double Longitude { get; set; }

    public string Province { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public string? Street { get; set; }

    public string? Area { get; set; }

    public string? FullAddress { get; set; }

    public int TotalSlots { get; set; }

    public int AvailableSlots { get; set; }

    public decimal PricePerHour { get; set; }

    public bool IsActive { get; set; }

    // IAuditable properties
    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    // ISoftDeletable properties
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}
