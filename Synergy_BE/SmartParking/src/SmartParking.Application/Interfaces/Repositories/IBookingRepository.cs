using SmartParking.Application.Common.Models;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, bool includeDeleted = false, CancellationToken ct = default);
    Task<PagedResult<Booking>> GetByUserIdAsync(
        Guid userId, 
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<PagedResult<Booking>> GetByOwnerIdAsync(
        Guid ownerId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IEnumerable<Booking>> GetByParkingLotIdAsync(Guid parkingLotId, bool includeDeleted = false, CancellationToken ct = default);
    Task<PagedResult<Booking>> GetByParkingLotIdPagedAsync(
        Guid parkingLotId, 
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid bookingId, Guid deletedBy, CancellationToken ct = default);
    Task<bool> HasConflictingBookingAsync(
        Guid parkingLotId, 
        DateTime startTime, 
        DateTime endTime, 
        Guid? excludeBookingId = null, 
        CancellationToken ct = default);
    Task<int> CountActiveBookingsByParkingLotAsync(Guid parkingLotId, CancellationToken ct = default);
    
    /// <summary>
    /// Get all bookings with filters (admin only)
    /// </summary>
    Task<PagedResult<Booking>> GetAllAsync(
        string? status,
        Guid? userId,
        Guid? parkingLotId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
