using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class ParkingLocationRepository : IParkingLocationRepository
{
    private readonly SmartParkingDBContext _context;

    public ParkingLocationRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<ParkingLocation?> GetByIdAsync(Guid locationId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.ParkingLocations
            .Include(l => l.ParkingLot)
            .ThenInclude(p => p.Owner)
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(l => !l.IsDeleted);
        }

        return await query
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LocationId == locationId, ct);
    }

    public async Task<ParkingLocation?> GetByParkingLotIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.ParkingLocations
            .Include(l => l.ParkingLot)
            .Where(l => l.ParkingLotId == parkingLotId)
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(l => !l.IsDeleted);
        }

        return await query
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<ParkingLocation>> GetNearbyAsync(
        decimal centerLat,
        decimal centerLon,
        double radiusMeters,
        CancellationToken ct = default)
    {
        // Pre-filter using bounding box (SQL optimization)
        // Approximate degrees per meter
        const double metersPerDegreeLat = 111320.0;
        var latRad = (double)centerLat * Math.PI / 180.0;
        var metersPerDegreeLon = 111320.0 * Math.Cos(latRad);

        var latDelta = radiusMeters / metersPerDegreeLat;
        var lonDelta = radiusMeters / metersPerDegreeLon;

        var minLat = (double)centerLat - latDelta;
        var maxLat = (double)centerLat + latDelta;
        var minLon = (double)centerLon - lonDelta;
        var maxLon = (double)centerLon + lonDelta;

        // Get all locations within bounding box
        var locations = await _context.ParkingLocations
            .Include(l => l.ParkingLot)
            .ThenInclude(p => p.Owner)
            .Where(l => !l.IsDeleted
                && l.ParkingLot.IsActive
                && !l.ParkingLot.IsDeleted
                && (double)l.Latitude >= minLat
                && (double)l.Latitude <= maxLat
                && (double)l.Longitude >= minLon
                && (double)l.Longitude <= maxLon)
            .AsNoTracking()
            .ToListAsync(ct);

        // Calculate exact distance using Haversine and filter by radius
        var nearbyLocations = locations
            .Where(l =>
            {
                var distance = GeoDistanceHelper.CalculateDistanceInMeters(
                    centerLat, centerLon,
                    l.Latitude, l.Longitude);
                return distance <= radiusMeters;
            })
            .OrderBy(l => GeoDistanceHelper.CalculateDistanceInMeters(
                centerLat, centerLon,
                l.Latitude, l.Longitude))
            .ToList();

        return nearbyLocations;
    }

    public async Task<PagedResult<ParkingLocation>> SearchAsync(
        string? province,
        string? district,
        string? ward,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.ParkingLocations
            .Include(l => l.ParkingLot)
            .ThenInclude(p => p.Owner)
            .Where(l => !l.IsDeleted && l.ParkingLot.IsActive && !l.ParkingLot.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(province))
        {
            query = query.Where(l => l.Province != null && l.Province.Contains(province));
        }

        if (!string.IsNullOrWhiteSpace(district))
        {
            query = query.Where(l => l.District != null && l.District.Contains(district));
        }

        if (!string.IsNullOrWhiteSpace(ward))
        {
            query = query.Where(l => l.Ward != null && l.Ward.Contains(ward));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l =>
                (l.FullAddress != null && l.FullAddress.Contains(searchTerm)) ||
                (l.Street != null && l.Street.Contains(searchTerm)) ||
                (l.ParkingLot.Name.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(l => l.ParkingLot.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<ParkingLocation>(items, page, pageSize, totalCount);
    }

    public async Task<Dictionary<Guid, ParkingLocation>> GetByParkingLotIdsAsync(IEnumerable<Guid> parkingLotIds, CancellationToken ct = default)
    {
        var ids = parkingLotIds.ToList();
        if (ids.Count == 0) return new Dictionary<Guid, ParkingLocation>();

        var locations = await _context.ParkingLocations
            .AsNoTracking()
            .Where(l => !l.IsDeleted && ids.Contains(l.ParkingLotId))
            .ToListAsync(ct);

        return locations.GroupBy(l => l.ParkingLotId)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public async Task<ParkingLocation> CreateAsync(ParkingLocation location, CancellationToken ct = default)
    {
        _context.ParkingLocations.Add(location);
        await _context.SaveChangesAsync(ct);
        return location;
    }

    public async Task UpdateAsync(ParkingLocation location, CancellationToken ct = default)
    {
        location.UpdatedAt = DateTime.UtcNow;
        _context.ParkingLocations.Update(location);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid locationId, Guid deletedBy, CancellationToken ct = default)
    {
        var location = await _context.ParkingLocations
            .FirstOrDefaultAsync(l => l.LocationId == locationId && !l.IsDeleted, ct);

        if (location != null)
        {
            location.IsDeleted = true;
            location.DeletedAt = DateTime.UtcNow;
            location.DeletedBy = deletedBy;
            location.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }
    }
}
