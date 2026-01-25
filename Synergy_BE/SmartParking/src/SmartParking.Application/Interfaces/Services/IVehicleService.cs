using SmartParking.Application.DTOs.Vehicle;

namespace SmartParking.Application.Interfaces.Services;

public interface IVehicleService
{
    Task<VehicleDto> GetByIdAsync(Guid vehicleId, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<VehicleDto>> GetMyVehiclesAsync(Guid userId, bool activeOnly = true, CancellationToken ct = default);
    Task<VehicleDto> CreateAsync(CreateVehicleDto request, Guid userId, CancellationToken ct = default);
    Task<VehicleDto> UpdateAsync(Guid vehicleId, UpdateVehicleDto request, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid vehicleId, Guid userId, CancellationToken ct = default);
}
