using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[Authorize]
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
        var booking = await _bookingService.GetByIdAsync(id, userId, ct);
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
        var booking = await _bookingService.UpdateAsync(id, request, userId, ct);
        return Ok(ApiResponse<BookingDto>.SuccessResponse(booking, Messages.Booking.UpdateSuccess));
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _bookingService.CancelAsync(id, userId, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Booking.CancelSuccess));
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(userIdClaim!);
    }
}
