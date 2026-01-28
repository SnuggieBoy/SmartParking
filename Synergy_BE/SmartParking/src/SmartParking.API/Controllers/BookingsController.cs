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
[Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
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
        var booking = await _bookingService.GetByIdAsync(id, userId, isAdmin, ct);
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
    /// SECURITY: Only booking owner OR Admin can check-in.
    /// Booking must be in Confirmed status to proceed.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    [HttpPost("{id:guid}/check-in")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingCheckInResponseDto>>> CheckIn(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = User.IsInRole(AuthConstants.Roles.Admin);
        var result = await _bookingService.BookingCheckInAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse<BookingCheckInResponseDto>.SuccessResponse(result, Messages.Booking.CheckInSuccess));
    }

    /// <summary>
    /// SECURITY: Only booking owner OR Admin can check-out.
    /// Booking must be in InProgress status to proceed.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    [HttpPost("{id:guid}/check-out")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingCheckOutResponseDto>>> CheckOut(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = User.IsInRole(AuthConstants.Roles.Admin);
        var result = await _bookingService.BookingCheckOutAsync(id, userId, isAdmin, ct);
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
    /// Extend booking time (for confirmed or in-progress bookings)
    /// </summary>
    [HttpPut("{id:guid}/extend")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> ExtendBooking(
        Guid id,
        [FromBody] ExtendBookingDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var booking = await _bookingService.ExtendBookingAsync(id, request.NewEndTime, userId, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, "Booking extended successfully"));
    }
}
