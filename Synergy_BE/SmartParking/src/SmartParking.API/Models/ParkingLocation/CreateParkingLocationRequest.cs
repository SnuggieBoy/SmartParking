using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.ParkingLocation;

/// <summary>
/// API model for creating a new parking location.
/// Includes validation attributes for input validation.
/// </summary>
public sealed class CreateParkingLocationRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Latitude is required")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Province is required")]
    [StringLength(100, ErrorMessage = "Province cannot exceed 100 characters")]
    public string Province { get; set; } = null!;

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    public string District { get; set; } = null!;

    [Required(ErrorMessage = "Ward is required")]
    [StringLength(100, ErrorMessage = "Ward cannot exceed 100 characters")]
    public string Ward { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters")]
    public string? Street { get; set; }

    [StringLength(100, ErrorMessage = "Area cannot exceed 100 characters")]
    public string? Area { get; set; }

    [StringLength(500, ErrorMessage = "Full address cannot exceed 500 characters")]
    public string? FullAddress { get; set; }

    [Required(ErrorMessage = "Total slots is required")]
    [Range(1, 10000, ErrorMessage = "Total slots must be between 1 and 10000")]
    public int TotalSlots { get; set; }

    [Required(ErrorMessage = "Available slots is required")]
    [Range(0, 10000, ErrorMessage = "Available slots must be between 0 and 10000")]
    public int AvailableSlots { get; set; }

    [Required(ErrorMessage = "Price per hour is required")]
    [Range(0, 1000000, ErrorMessage = "Price per hour must be between 0 and 1,000,000")]
    public decimal PricePerHour { get; set; }
}
