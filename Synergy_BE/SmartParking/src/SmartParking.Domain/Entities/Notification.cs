namespace SmartParking.Domain.Entities;

/// <summary>
/// User notifications
/// </summary>
public partial class Notification
{
    public Guid NotificationId { get; set; }

    public Guid? UserId { get; set; } // null = broadcast to all

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error, Booking, Payment, System

    public string? Data { get; set; } // JSON data for action (e.g., bookingId, parkingLotId)

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; } // Admin who sent broadcast

    // Navigation properties
    public virtual User? User { get; set; }
}
