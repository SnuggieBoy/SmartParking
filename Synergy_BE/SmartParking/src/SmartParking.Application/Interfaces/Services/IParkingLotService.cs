using SmartParking.Application.DTOs.ParkingLot;

namespace SmartParking.Application.Interfaces.Services;

public interface IParkingLotService
{
    Task<ParkingLotDto> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<IEnumerable<ParkingLotDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<IEnumerable<ParkingLotDto>> GetMyParkingLotsAsync(Guid ownerId, CancellationToken ct = default);
    Task<ParkingLotDto> CreateAsync(CreateParkingLotDto request, Guid ownerId, CancellationToken ct = default);
    Task<ParkingLotDto> UpdateAsync(Guid parkingLotId, UpdateParkingLotDto request, Guid ownerId, CancellationToken ct = default);
    Task DeleteAsync(Guid parkingLotId, Guid ownerId, CancellationToken ct = default);
}
