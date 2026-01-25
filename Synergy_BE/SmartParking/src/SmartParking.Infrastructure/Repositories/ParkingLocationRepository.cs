using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ParkingLocation entity.
/// Handles all database operations for parking locations.
/// </summary>
public sealed class ParkingLocationRepository : IParkingLocationRepository
{
    private readonly SmartParkingDBContext _context;

    public ParkingLocationRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ParkingLocation entity, CancellationToken ct = default)
    {
        _context.ParkingLocations.Add(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ParkingLocation>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ParkingLocations
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ParkingLocation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ParkingLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IEnumerable<ParkingLocation>> GetActiveWithSlotsAsync(CancellationToken ct = default)
    {
        return await _context.ParkingLocations
            .Where(p => p.IsActive && p.AvailableSlots > 0)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ParkingLocation>> GetActiveWithinBoundsAsync(
        double minLat, double maxLat,
        double minLon, double maxLon,
        CancellationToken ct = default)
    {
        return await _context.ParkingLocations
            .AsNoTracking()
            .Where(p => p.IsActive 
                && p.AvailableSlots > 0
                && p.Latitude >= minLat && p.Latitude <= maxLat
                && p.Longitude >= minLon && p.Longitude <= maxLon)
            .ToListAsync(ct);
    }
}
