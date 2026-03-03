namespace SmartParking.API.Models.Owner;

/// <summary>
/// Request body model when a User wants to upgrade to Owner.
/// </summary>
public sealed class CreateOwnerUpgradeRequestModel
{
    public string ParkingLotName { get; set; } = null!;
    public string ParkingLotAddress { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string PlanType { get; set; } = "Monthly"; // default
    public Guid? PaymentTransactionId { get; set; }
    /// <summary>Optional image URL (from Cloudinary upload).</summary>
    public string? ImageUrl { get; set; }
    /// <summary>Optional description of the parking lot.</summary>
    public string? Description { get; set; }
}

