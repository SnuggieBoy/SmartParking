using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly SmartParkingDBContext _context;

    public BookingRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct = default)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.ParkingLot)
            .Include(b => b.Vehicle)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == bookingId, ct);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Bookings
            .Include(b => b.ParkingLot)
            .Include(b => b.Vehicle)
            .Where(b => b.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Booking>> GetByParkingLotIdAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Where(b => b.ParkingLotId == parkingLotId)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByParkingLotIdPagedAsync(
        Guid parkingLotId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Where(b => b.ParkingLotId == parkingLotId)
            .AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);
        return booking;
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> HasConflictingBookingAsync(
        Guid parkingLotId, 
        DateTime startTime, 
        DateTime endTime, 
        Guid? excludeBookingId = null, 
        CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Where(b => b.ParkingLotId == parkingLotId 
                && b.Status != "Cancelled" 
                && b.Status != "Completed"
                && ((b.StartTime <= endTime && b.EndTime >= startTime)));

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.BookingId != excludeBookingId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
