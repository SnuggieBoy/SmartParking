using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for ParkingLocation entity.
/// Provides data access methods for parking location management.
/// </summary>
public interface IParkingLocationRepository
{
    /// <summary>
    /// Adds a new parking location to the database.
    /// </summary>
    Task AddAsync(ParkingLocation entity, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all parking locations (including inactive).
    /// </summary>
    Task<IEnumerable<ParkingLocation>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a parking location by ID.
    /// </summary>
    Task<ParkingLocation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves only active parking locations with available slots.
    /// Used for nearby search to show only bookable locations.
    /// </summary>
    Task<IEnumerable<ParkingLocation>> GetActiveWithSlotsAsync(CancellationToken ct = default);
}
