using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct = default);
    Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Booking>> GetByParkingLotIdAsync(Guid parkingLotId, CancellationToken ct = default);
    Task<(IReadOnlyList<Booking> Items, int TotalCount)> GetByParkingLotIdPagedAsync(Guid parkingLotId, int skip, int take, CancellationToken ct = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
    Task<bool> HasConflictingBookingAsync(Guid parkingLotId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null, CancellationToken ct = default);
}
