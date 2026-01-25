namespace SmartParking.Domain.Interfaces;

/// <summary>
/// Interface for entities that track creation and modification metadata.
/// Automatically populated via DbContext SaveChangesAsync override.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}
