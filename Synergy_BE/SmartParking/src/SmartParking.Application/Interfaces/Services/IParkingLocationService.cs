using SmartParking.Application.DTOs.ParkingLocation;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service interface for parking location management and map-based search.
/// Contains business logic for geospatial queries.
/// </summary>
public interface IParkingLocationService
{
    /// <summary>
    /// Creates a new parking location (Admin only).
    /// Validates coordinates and slot availability.
    /// </summary>
    Task CreateAsync(ParkingLocationCreateDto dto, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all parking locations (Admin only).
    /// Includes inactive locations.
    /// </summary>
    Task<IEnumerable<ParkingLocationResponseDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Finds nearby parking locations based on user's coordinates.
    /// Returns active locations with available slots, sorted by distance.
    /// </summary>
    /// <param name="latitude">User's latitude coordinate</param>
    /// <param name="longitude">User's longitude coordinate</param>
    /// <param name="radiusInMeters">Search radius in meters (default: 3000m = 3km)</param>
    Task<IEnumerable<ParkingLocationResponseDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters = 3000,
        CancellationToken ct = default);
}
