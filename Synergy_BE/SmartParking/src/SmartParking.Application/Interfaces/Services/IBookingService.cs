using SmartParking.Application.DTOs.Booking;

namespace SmartParking.Application.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, CancellationToken ct = default);
    Task<IEnumerable<BookingListDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default);
    Task<BookingDto> CreateAsync(CreateBookingDto request, Guid userId, CancellationToken ct = default);
    Task<BookingDto> UpdateAsync(Guid bookingId, UpdateBookingDto request, Guid userId, CancellationToken ct = default);
    Task CancelAsync(Guid bookingId, Guid userId, CancellationToken ct = default);
}
