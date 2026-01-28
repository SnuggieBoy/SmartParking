namespace SmartParking.Domain.Entities;

/// <summary>
/// User reviews for parking lots (after completed booking)
/// </summary>
public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid UserId { get; set; }

    public Guid ParkingLotId { get; set; }

    public Guid BookingId { get; set; }

    public int Rating { get; set; } // 1-5 stars

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Soft delete (for admin moderation)
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual ParkingLot ParkingLot { get; set; } = null!;

    public virtual Booking Booking { get; set; } = null!;
}
