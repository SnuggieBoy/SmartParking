using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

/// <summary>
/// Booking management endpoints.
/// Security: Users manage own bookings. Admin has full access.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
[Route("api/bookings")]
public sealed class BookingsController : BaseApiController
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingListDto>>>> GetMyBookings(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _bookingService.GetMyBookingsAsync(userId, status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<BookingListDto>>.SuccessResponse(result, "Bookings retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var isOwner = IsOwner();
        var booking = await _bookingService.GetByIdAsync(id, userId, isAdmin, isOwner, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, "Booking retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Create(
        [FromBody] CreateBookingDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var booking = await _bookingService.CreateAsync(request, userId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = booking.BookingId },
            ApiResponse<BookingDto>.SuccessResponse(booking, Messages.Booking.CreateSuccess)
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Update(
        Guid id,
        [FromBody] UpdateBookingDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var booking = await _bookingService.UpdateAsync(id, request, userId, isAdmin, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, Messages.Booking.UpdateSuccess));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        await _bookingService.CancelAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Booking.CancelSuccess));
    }

    /// <summary>
    /// SECURITY: Booking owner (User), Parking lot Owner, or Admin can check-in.
    /// Booking must be in Confirmed status. Owner có thể giả lập check-in cho khách tại bãi của mình.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("{id:guid}/check-in")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingCheckInResponseDto>>> CheckIn(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var isOwner = IsOwner();
        var result = await _bookingService.BookingCheckInAsync(id, userId, isAdmin, isOwner, ct);
        return Ok(ApiResponse<BookingCheckInResponseDto>.SuccessResponse(result, Messages.Booking.CheckInSuccess));
    }

    /// <summary>
    /// Preview checkout: tính tiền theo thời gian thực tế, refund nếu checkout sớm.
    /// Dùng để hiển thị màn hình xác nhận trước khi checkout.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpGet("{id:guid}/check-out/preview")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutPreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingCheckOutPreviewDto>>> GetCheckoutPreview(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var isOwner = IsOwner();
        var result = await _bookingService.GetCheckoutPreviewAsync(id, userId, isAdmin, isOwner, ct);
        if (result == null)
            return NotFound(ApiResponse<BookingCheckOutPreviewDto>.FailureResponse("Booking không tồn tại hoặc không thể checkout"));
        return Ok(ApiResponse<BookingCheckOutPreviewDto>.SuccessResponse(result, "Preview checkout"));
    }

    /// <summary>
    /// SECURITY: Booking owner (User), Parking lot Owner, or Admin can check-out.
    /// Booking must be in InProgress status. Owner có thể giả lập check-out cho khách.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("{id:guid}/check-out")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingCheckOutResponseDto>>> CheckOut(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var isOwner = IsOwner();
        var result = await _bookingService.BookingCheckOutAsync(id, userId, isAdmin, isOwner, ct);
        return Ok(ApiResponse<BookingCheckOutResponseDto>.SuccessResponse(result, Messages.Booking.CheckOutSuccess));
    }

    /// <summary>
    /// Get booking history (completed bookings only)
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingHistoryDto>>>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _bookingService.GetBookingHistoryAsync(userId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<BookingHistoryDto>>.SuccessResponse(result, "Booking history retrieved successfully"));
    }

    /// <summary>
    /// Get invoice for a completed booking
    /// </summary>
    [HttpGet("{id:guid}/invoice")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetInvoice(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var invoice = await _bookingService.GetInvoiceAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse<InvoiceDto>.SuccessResponse(invoice, "Invoice retrieved successfully"));
    }

    /// <summary>
    /// User requests extension (owner must approve)
    /// </summary>
    [HttpPost("{id:guid}/extension-request")]
    [ProducesResponseType(typeof(ApiResponse<ExtensionRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExtensionRequestDto>>> RequestExtension(
        Guid id,
        [FromBody] ExtendBookingDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _bookingService.RequestExtensionAsync(id, request.NewEndTime, userId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<ExtensionRequestDto>.SuccessResponse(result, "Extension request submitted. Waiting for owner approval.")
        );
    }

    /// <summary>
    /// OWNER: Get pending extension requests (with available slots)
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpGet("owner/extension-requests")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExtensionRequestDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExtensionRequestDto>>>> GetExtensionRequests(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _bookingService.GetPendingExtensionRequestsAsync(userId, ct);
        return Ok(ApiResponse<IEnumerable<ExtensionRequestDto>>.SuccessResponse(result, "Extension requests retrieved successfully"));
    }

    /// <summary>
    /// OWNER: Approve extension request
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPost("extension-requests/{extensionRequestId:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> ApproveExtension(
        Guid extensionRequestId,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var booking = await _bookingService.ApproveExtensionAsync(extensionRequestId, userId, isAdmin, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, "Extension approved successfully"));
    }

    /// <summary>
    /// OWNER: Reject extension request
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPost("extension-requests/{extensionRequestId:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RejectExtension(
        Guid extensionRequestId,
        [FromBody] RejectStartDto? request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var reason = request?.Reason ?? "Rejected by owner";
        await _bookingService.RejectExtensionAsync(extensionRequestId, userId, reason, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse("Extension rejected successfully"));
    }
    /// <summary>
    /// OWNER: Get all bookings for all my parking lots
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpGet("owner/my-bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLotBookingDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLotBookingDto>>>> GetOwnerBookings(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _bookingService.GetOwnerBookingsAsync(userId, status, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotBookingDto>>.SuccessResponse(result, "Owner bookings retrieved successfully"));
    }

    /// <summary>
    /// OWNER: Approve a pending booking
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> ApproveBooking(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var booking = await _bookingService.ApproveBookingAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, "Booking approved successfully"));
    }

    /// <summary>
    /// OWNER: Reject a pending booking
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RejectBooking(
        Guid id, 
        [FromBody] RejectStartDto? request, // Optional reason
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var reason = request?.Reason ?? "Rejected by owner"; 
        
        await _bookingService.RejectBookingAsync(id, userId, reason, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse("Booking rejected successfully"));
    }
}

public sealed record RejectStartDto(string Reason);
