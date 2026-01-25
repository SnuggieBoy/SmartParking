using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IParkingLotRepository
{
    Task<ParkingLot?> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<IEnumerable<ParkingLot>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<IEnumerable<ParkingLot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task<ParkingLot> CreateAsync(ParkingLot parkingLot, CancellationToken ct = default);
    Task UpdateAsync(ParkingLot parkingLot, CancellationToken ct = default);
    Task DeleteAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<bool> HasAvailableSlotsAsync(Guid parkingLotId, CancellationToken ct = default);
    Task UpdateOccupancyAsync(Guid parkingLotId, int change, CancellationToken ct = default);
}
