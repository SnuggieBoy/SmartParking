using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid vehicleId, CancellationToken ct = default);
    Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken ct = default);
    Task<IEnumerable<Vehicle>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken ct = default);
    Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken ct = default);
    Task UpdateAsync(Vehicle vehicle, CancellationToken ct = default);
    Task DeleteAsync(Guid vehicleId, CancellationToken ct = default);
}
