using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
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
[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<BookingListDto>>>> GetMyBookings(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var bookings = await _bookingService.GetMyBookingsAsync(userId, ct);
        return Ok(ApiResponse<IEnumerable<BookingListDto>>.SuccessResponse(bookings, "Bookings retrieved successfully"));
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
    /// SECURITY: Extracts UserId from JWT claims (NOT from request body).
    /// Never trust userId from client input - always extract from validated JWT.
    /// </summary>
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// SECURITY: Checks if current user has Admin role.
    /// Admin role bypasses ownership checks in service layer.
    /// </summary>
    private bool IsAdmin()
    {
        return User.IsInRole(AuthConstants.Roles.Admin);
    }
}
