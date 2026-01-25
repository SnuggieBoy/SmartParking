namespace SmartParking.Domain.Interfaces;

/// <summary>
/// Interface for entities that support soft delete.
/// Records are marked as deleted instead of being physically removed.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}
