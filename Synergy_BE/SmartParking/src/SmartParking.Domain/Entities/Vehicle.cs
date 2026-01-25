namespace SmartParking.Domain.Entities;

public partial class Vehicle
{
    public Guid VehicleId { get; set; }
    public Guid UserId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public int VehicleType { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
