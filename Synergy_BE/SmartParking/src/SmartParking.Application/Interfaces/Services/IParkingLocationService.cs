using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Location;

namespace SmartParking.Application.Interfaces.Services;

public interface IParkingLocationService
{
    Task<ParkingLocationResponseDto> GetByIdAsync(Guid locationId, CancellationToken ct = default);
    Task<ParkingLocationResponseDto?> GetByParkingLotIdAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<IEnumerable<ParkingLocationResponseDto>> GetNearbyAsync(NearbyLocationRequestDto request, CancellationToken ct = default);
    Task<PagedResult<ParkingLocationResponseDto>> SearchAsync(SearchLocationRequestDto request, CancellationToken ct = default);
    Task<ParkingLocationResponseDto> CreateAsync(CreateLocationDto request, Guid userId, CancellationToken ct = default);
    Task<ParkingLocationResponseDto> UpdateAsync(Guid locationId, UpdateLocationDto request, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task DeleteAsync(Guid locationId, Guid userId, bool isAdmin, CancellationToken ct = default);
}
