using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.User;
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

    /// <summary>
    /// Get booking history (completed bookings only)
    /// </summary>
    Task<PagedResult<BookingHistoryDto>> GetBookingHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Get invoice for a completed booking
    /// </summary>
    Task<InvoiceDto> GetInvoiceAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);

    /// <summary>
    /// Extend booking time (when user is still parked)
    /// </summary>
    Task<BookingDto> ExtendBookingAsync(Guid bookingId, DateTime newEndTime, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// OWNER: Get all bookings for all parking lots owned by this user
    /// </summary>
    Task<PagedResult<ParkingLotBookingDto>> GetOwnerBookingsAsync(
        Guid ownerId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// OWNER: Approve a booking (Set status to Confirmed)
    /// </summary>
    Task<BookingDto> ApproveBookingAsync(Guid bookingId, Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// OWNER: Reject a booking
    /// </summary>
    Task RejectBookingAsync(Guid bookingId, Guid ownerId, string reason, CancellationToken ct = default);
}
