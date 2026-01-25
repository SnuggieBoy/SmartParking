using Microsoft.EntityFrameworkCore;
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

    public async Task<ParkingLot?> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        return await _context.ParkingLots
            .Include(p => p.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId, ct);
    }

    public async Task<IEnumerable<ParkingLot>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var query = _context.ParkingLots.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(p => p.Status == "Active");
        }

        return await query
            .Include(p => p.Owner)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ParkingLot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.ParkingLots
            .Where(p => p.OwnerId == ownerId)
            .AsNoTracking()
            .OrderBy(p => p.Name)
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
        _context.ParkingLots.Update(parkingLot);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(new object[] { parkingLotId }, ct);
        if (parkingLot != null)
        {
            _context.ParkingLots.Remove(parkingLot);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> HasAvailableSlotsAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId, ct);

        return parkingLot != null && parkingLot.CurrentOccupancy < parkingLot.TotalCapacity;
    }

    public async Task UpdateOccupancyAsync(Guid parkingLotId, int change, CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots.FindAsync(new object[] { parkingLotId }, ct);
        if (parkingLot != null)
        {
            parkingLot.CurrentOccupancy += change;
            if (parkingLot.CurrentOccupancy < 0) parkingLot.CurrentOccupancy = 0;
            if (parkingLot.CurrentOccupancy > parkingLot.TotalCapacity) parkingLot.CurrentOccupancy = parkingLot.TotalCapacity;
            
            await _context.SaveChangesAsync(ct);
        }
    }
}
