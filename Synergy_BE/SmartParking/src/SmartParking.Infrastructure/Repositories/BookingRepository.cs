using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly SmartParkingDBContext _context;

    public BookingRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid bookingId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.ParkingLot)
            .ThenInclude(p => p.Owner)
            .Include(b => b.Vehicle)
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(b => !b.IsDeleted);
        }

        return await query
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == bookingId, ct);
    }

    public async Task<PagedResult<Booking>> GetByUserIdAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Include(b => b.ParkingLot)
            .Include(b => b.Vehicle)
            .Where(b => b.UserId == userId && !b.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(b => b.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Booking>(items, page, pageSize, totalCount);
    }

    public async Task<IEnumerable<Booking>> GetByParkingLotIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Where(b => b.ParkingLotId == parkingLotId)
            .AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(b => !b.IsDeleted);
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Booking>> GetByParkingLotIdPagedAsync(
        Guid parkingLotId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
            .Where(b => b.ParkingLotId == parkingLotId && !b.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(b => b.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Booking>(items, page, pageSize, totalCount);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);
        return booking;
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        booking.UpdatedAt = DateTime.UtcNow;
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid bookingId, Guid deletedBy, CancellationToken ct = default)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId && !b.IsDeleted, ct);
            
        if (booking != null)
        {
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;
            booking.DeletedBy = deletedBy;
            booking.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);
        }
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
                && !b.IsDeleted
                && b.Status != nameof(BookingStatus.Cancelled)
                && b.Status != nameof(BookingStatus.Completed)
                && b.Status != nameof(BookingStatus.Expired)
                && ((b.StartTime <= endTime && b.EndTime >= startTime)));

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.BookingId != excludeBookingId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<int> CountActiveBookingsByParkingLotAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        return await _context.Bookings
            .CountAsync(b => b.ParkingLotId == parkingLotId
                && !b.IsDeleted
                && b.Status != nameof(BookingStatus.Cancelled)
                && b.Status != nameof(BookingStatus.Completed)
                && b.Status != nameof(BookingStatus.Expired), ct);
    }
}
