using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.ParkingLot;

namespace SmartParking.Application.Interfaces.Services;

public interface IParkingLotService
{
    Task<ParkingLotResponseDto> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<PagedResult<ParkingLotResponseDto>> GetAllAsync(ParkingLotFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<ParkingLotResponseDto>> GetMyParkingLotsAsync(Guid ownerId, CancellationToken ct = default);
    Task<ParkingLotResponseDto> CreateAsync(CreateParkingLotDto request, Guid ownerId, CancellationToken ct = default);
    Task<ParkingLotResponseDto> UpdateAsync(Guid parkingLotId, UpdateParkingLotDto request, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task DeleteAsync(Guid parkingLotId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<ParkingLotResponseDto> ToggleActiveAsync(Guid parkingLotId, Guid userId, bool isAdmin, CancellationToken ct = default);

    /// <summary>
    /// Find nearby parking lots around a given location, sorted by distance (ascending).
    /// Returns only lots that have valid coordinates and are not deleted.
    /// </summary>
    Task<IEnumerable<ParkingLotResponseDto>> GetNearbyAsync(
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        int maxResults,
        CancellationToken ct = default);
}
