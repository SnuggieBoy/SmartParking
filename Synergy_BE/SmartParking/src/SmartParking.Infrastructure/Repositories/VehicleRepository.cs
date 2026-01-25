using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly SmartParkingDBContext _context;

    public VehicleRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(Guid vehicleId, CancellationToken ct = default)
    {
        return await _context.Vehicles
            .Include(v => v.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, ct);
    }

    public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken ct = default)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, ct);
    }

    public async Task<IEnumerable<Vehicle>> GetByUserIdAsync(Guid userId, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = _context.Vehicles.Where(v => v.UserId == userId);

        if (activeOnly)
        {
            query = query.Where(v => v.IsActive == true);
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(ct);
        return vehicle;
    }

    public async Task UpdateAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        _context.Vehicles.Update(vehicle);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _context.Vehicles.FindAsync(new object[] { vehicleId }, ct);
        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync(ct);
        }
    }
}
