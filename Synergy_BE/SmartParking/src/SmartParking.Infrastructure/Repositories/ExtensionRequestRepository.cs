using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class ExtensionRequestRepository : IExtensionRequestRepository
{
    private readonly SmartParkingDBContext _context;

    public ExtensionRequestRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<ExtensionRequest> CreateAsync(ExtensionRequest request, CancellationToken ct = default)
    {
        _context.ExtensionRequests.Add(request);
        await _context.SaveChangesAsync(ct);
        return request;
    }

    public async Task<ExtensionRequest?> GetByIdAsync(Guid extensionRequestId, CancellationToken ct = default)
    {
        return await _context.ExtensionRequests
            .Include(e => e.Booking)
            .ThenInclude(b => b!.ParkingLot)
            .Include(e => e.Booking)
            .ThenInclude(b => b!.User)
            .Include(e => e.Booking)
            .ThenInclude(b => b!.Vehicle)
            .FirstOrDefaultAsync(e => e.ExtensionRequestId == extensionRequestId, ct);
    }

    public async Task<IEnumerable<ExtensionRequest>> GetPendingByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.ExtensionRequests
            .Include(e => e.Booking)
            .ThenInclude(b => b!.ParkingLot)
            .Include(e => e.Booking)
            .ThenInclude(b => b!.User)
            .Include(e => e.Booking)
            .ThenInclude(b => b!.Vehicle)
            .Where(e => e.Status == "Pending"
                && e.Booking != null
                && !e.Booking.IsDeleted
                && e.Booking.ParkingLot != null
                && e.Booking.ParkingLot.OwnerId == ownerId)
            .OrderByDescending(e => e.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(ExtensionRequest request, CancellationToken ct = default)
    {
        _context.ExtensionRequests.Update(request);
        await _context.SaveChangesAsync(ct);
    }
}
