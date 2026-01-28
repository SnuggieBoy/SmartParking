using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.Common.Models;

namespace SmartParking.Application.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<PagedResult<BookingListDto>> GetMyBookingsAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<BookingDto> CreateAsync(CreateBookingDto request, Guid userId, CancellationToken ct = default);
    Task<BookingDto> UpdateAsync(Guid bookingId, UpdateBookingDto request, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task CancelAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);

    Task<BookingCheckInResponseDto> BookingCheckInAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<BookingCheckOutResponseDto> BookingCheckOutAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<PagedResult<ParkingLotBookingDto>> GetBookingsByParkingLotAsync(Guid parkingLotId, Guid userId, bool isAdmin, int page, int pageSize, CancellationToken ct = default);
    
    /// <summary>
    /// Get all bookings (admin only - can filter by status, userId, parkingLotId)
    /// </summary>
    Task<PagedResult<BookingListDto>> GetAllBookingsAsync(
        string? status,
        Guid? userId,
        Guid? parkingLotId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
