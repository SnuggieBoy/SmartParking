using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class ParkingLotRepository : IParkingLotRepository
{
    private readonly SmartParkingDBContext _context;

    public ParkingLotRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<ParkingLot?> GetByIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.ParkingLots
            .Include(p => p.Owner)
            .ThenInclude(o => o.Role)
            .AsNoTracking();

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        return await query.FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId, ct);
    }

    public async Task<PagedResult<ParkingLot>> GetAllAsync(
        string? searchTerm,
        bool? isActive,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.ParkingLots
            .Include(p => p.Owner)
            .ThenInclude(o => o.Role)
            .Where(p => !p.IsDeleted) // Soft delete filter
            .AsQueryable();

        // Search by name or address
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.Address.Contains(searchTerm));
        }

        // Filter by IsActive
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        // Filter by Status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<ParkingLot>(items, page, pageSize, totalCount);
    }

    public async Task<IEnumerable<ParkingLot>> GetByOwnerIdAsync(Guid ownerId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.ParkingLots
            .Where(p => p.OwnerId == ownerId);

        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ParkingLot> CreateAsync(ParkingLot parkingLot, CancellationToken ct = default)
    {
        _context.ParkingLots.Add(parkingLot);
        await _context.SaveChangesAsync(ct);
        return parkingLot;
    }

    public async Task UpdateAsync(ParkingLot parkingLot, CancellationToken ct = default)
    {
        parkingLot.UpdatedAt = DateTime.UtcNow;
        _context.ParkingLots.Update(parkingLot);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid parkingLotId, Guid deletedBy, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);
            
        if (parkingLot != null)
        {
            parkingLot.IsDeleted = true;
            parkingLot.DeletedAt = DateTime.UtcNow;
            parkingLot.DeletedBy = deletedBy;
            parkingLot.IsActive = false; // Also mark as inactive
            
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> HasAvailableSlotsAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);

        return parkingLot != null && 
               parkingLot.IsActive && 
               parkingLot.CurrentOccupancy < parkingLot.TotalCapacity;
    }

    public async Task UpdateOccupancyAsync(Guid parkingLotId, int change, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);
            
        if (parkingLot != null)
        {
            parkingLot.CurrentOccupancy += change;
            
            // Ensure within valid range
            if (parkingLot.CurrentOccupancy < 0) 
                parkingLot.CurrentOccupancy = 0;
            if (parkingLot.CurrentOccupancy > parkingLot.TotalCapacity) 
                parkingLot.CurrentOccupancy = parkingLot.TotalCapacity;
            
            parkingLot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IEnumerable<ParkingLot>> GetAllWithLocationAsync(bool onlyActive = true, CancellationToken ct = default)
    {
        var query = _context.ParkingLots
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                        p.Latitude.HasValue &&
                        p.Longitude.HasValue);

        if (onlyActive)
        {
            query = query.Where(p => p.IsActive && p.Status == "Approved");
        }

        return await query.ToListAsync(ct);
    }
}
