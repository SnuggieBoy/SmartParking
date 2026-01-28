namespace SmartParking.Domain.Entities;

/// <summary>
/// User's favorite parking lots
/// </summary>
public partial class Favorite
{
    public Guid FavoriteId { get; set; }

    public Guid UserId { get; set; }

    public Guid ParkingLotId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual ParkingLot ParkingLot { get; set; } = null!;
}
