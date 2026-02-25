using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Common.Models;

namespace SmartParking.Application.Interfaces.Services;

public interface IBookingService
{
    Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default);
    Task<PagedResult<BookingListDto>> GetMyBookingsAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<BookingDto> CreateAsync(CreateBookingDto request, Guid userId, CancellationToken ct = default);
    Task<BookingDto> UpdateAsync(Guid bookingId, UpdateBookingDto request, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task CancelAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default);

    Task<BookingCheckInResponseDto> BookingCheckInAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default);
    Task<BookingCheckOutPreviewDto?> GetCheckoutPreviewAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default);
    Task<BookingCheckOutResponseDto> BookingCheckOutAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default);
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
    /// User requests extension (creates Pending request, owner must approve)
    /// </summary>
    Task<ExtensionRequestDto> RequestExtensionAsync(Guid bookingId, DateTime newEndTime, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Owner: Get pending extension requests for their parking lots (with available slots)
    /// </summary>
    Task<IEnumerable<ExtensionRequestDto>> GetPendingExtensionRequestsAsync(Guid ownerId, CancellationToken ct = default);

    /// <summary>
    /// Owner: Approve extension request (applies the extend)
    /// </summary>
    Task<BookingDto> ApproveExtensionAsync(Guid extensionRequestId, Guid ownerId, bool isAdmin = false, CancellationToken ct = default);

    /// <summary>
    /// Owner: Reject extension request
    /// </summary>
    Task RejectExtensionAsync(Guid extensionRequestId, Guid ownerId, string reason, bool isAdmin = false, CancellationToken ct = default);

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
    /// OWNER: Approve a booking (Set status to Confirmed). Admin bypasses owner check.
    /// </summary>
    Task<BookingDto> ApproveBookingAsync(Guid bookingId, Guid ownerId, bool isAdmin = false, CancellationToken ct = default);

    /// <summary>
    /// OWNER: Reject a booking. Admin bypasses owner check.
    /// </summary>
    Task RejectBookingAsync(Guid bookingId, Guid ownerId, string reason, bool isAdmin = false, CancellationToken ct = default);
}
