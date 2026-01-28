using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for booking management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/bookings")]
public sealed class AdminBookingsController : BaseApiController
{
    private readonly IBookingService _bookingService;

    public AdminBookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Get all bookings (admin view - can filter by status, user, parking lot)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingListDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? parkingLotId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _bookingService.GetAllBookingsAsync(status, userId, parkingLotId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<BookingListDto>>.SuccessResponse(result, "Bookings retrieved successfully"));
    }

    /// <summary>
    /// Get booking by ID (admin view)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var booking = await _bookingService.GetByIdAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, "Booking retrieved successfully"));
    }

    /// <summary>
    /// Update booking (admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Update(
        Guid id,
        [FromBody] UpdateBookingDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var booking = await _bookingService.UpdateAsync(id, request, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, Messages.Booking.UpdateSuccess));
    }

    /// <summary>
    /// Cancel booking (admin)
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        await _bookingService.CancelAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Booking.CancelSuccess));
    }

    /// <summary>
    /// Check-in booking (admin)
    /// </summary>
    [HttpPost("{id:guid}/check-in")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckInResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookingCheckInResponseDto>>> CheckIn(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _bookingService.BookingCheckInAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<BookingCheckInResponseDto>.SuccessResponse(result, Messages.Booking.CheckInSuccess));
    }

    /// <summary>
    /// Check-out booking (admin)
    /// </summary>
    [HttpPost("{id:guid}/check-out")]
    [ProducesResponseType(typeof(ApiResponse<BookingCheckOutResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookingCheckOutResponseDto>>> CheckOut(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _bookingService.BookingCheckOutAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<BookingCheckOutResponseDto>.SuccessResponse(result, Messages.Booking.CheckOutSuccess));
    }
}
