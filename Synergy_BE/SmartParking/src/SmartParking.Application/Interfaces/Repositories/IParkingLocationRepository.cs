using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IParkingLocationRepository
{
    Task<ParkingLocation?> GetByIdAsync(Guid locationId, bool includeDeleted = false, CancellationToken ct = default);
    Task<ParkingLocation?> GetByParkingLotIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default);
    Task<Dictionary<Guid, ParkingLocation>> GetByParkingLotIdsAsync(IEnumerable<Guid> parkingLotIds, CancellationToken ct = default);
    Task<IEnumerable<ParkingLocation>> GetNearbyAsync(
        decimal centerLat,
        decimal centerLon,
        double radiusMeters,
        CancellationToken ct = default);
    Task<PagedResult<ParkingLocation>> SearchAsync(
        string? province,
        string? district,
        string? ward,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<ParkingLocation> CreateAsync(ParkingLocation location, CancellationToken ct = default);
    Task UpdateAsync(ParkingLocation location, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid locationId, Guid deletedBy, CancellationToken ct = default);
}
