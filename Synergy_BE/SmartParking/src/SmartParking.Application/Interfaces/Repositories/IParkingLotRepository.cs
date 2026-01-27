using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IParkingLotRepository
{
    Task<ParkingLot?> GetByIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default);
    Task<PagedResult<ParkingLot>> GetAllAsync(
        string? searchTerm, 
        bool? isActive, 
        string? status,
        int page, 
        int pageSize, 
        CancellationToken ct = default);
    Task<IEnumerable<ParkingLot>> GetByOwnerIdAsync(Guid ownerId, bool includeDeleted = false, CancellationToken ct = default);
    Task<ParkingLot> CreateAsync(ParkingLot parkingLot, CancellationToken ct = default);
    Task UpdateAsync(ParkingLot parkingLot, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid parkingLotId, Guid deletedBy, CancellationToken ct = default);
    Task<bool> HasAvailableSlotsAsync(Guid parkingLotId, CancellationToken ct = default);
    Task UpdateOccupancyAsync(Guid parkingLotId, int change, CancellationToken ct = default);

    /// <summary>
    /// Returns all parking lots that have valid coordinates.
    /// Used for nearby search (distance calculated at service layer).
    /// </summary>
    Task<IEnumerable<ParkingLot>> GetAllWithLocationAsync(bool onlyActive = true, CancellationToken ct = default);
}
