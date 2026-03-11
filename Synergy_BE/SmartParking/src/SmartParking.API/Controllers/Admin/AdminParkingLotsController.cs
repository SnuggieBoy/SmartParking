using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.ParkingLot;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for parking lot management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/parking-lots")]
public sealed class AdminParkingLotsController : BaseApiController
{
    private readonly IParkingLotService _parkingLotService;
    private readonly IBookingService _bookingService;

    public AdminParkingLotsController(IParkingLotService parkingLotService, IBookingService bookingService)
    {
        _parkingLotService = parkingLotService;
        _bookingService = bookingService;
    }

    /// <summary>
    /// Get all parking lots (admin view - includes inactive and by any status)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLotResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLotResponseDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] string? status,
        [FromQuery] int? provinceCode,
        [FromQuery] string? province,
        [FromQuery] int? wardCode,
        [FromQuery] string? ward,
        [FromQuery] Guid? ownerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new ParkingLotFilterDto(search, isActive, status, provinceCode, province, wardCode, ward, ownerId, page, pageSize);
        var result = await _parkingLotService.GetAllAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotResponseDto>>.SuccessResponse(result, "Parking lots retrieved successfully"));
    }

    /// <summary>
    /// Get all parking lots that are pending approval.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLotResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLotResponseDto>>>> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new ParkingLotFilterDto(
            SearchTerm: null,
            IsActive: null,
            Status: ParkingLotStatus.PendingApproval,
            ProvinceCode: null,
            Province: null,
            WardCode: null,
            Ward: null,
            OwnerId: null,
            Page: page,
            PageSize: pageSize);

        var result = await _parkingLotService.GetAllAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotResponseDto>>.SuccessResponse(result, "Pending parking lots retrieved successfully"));
    }

    /// <summary>
    /// Get parking lot by ID (admin view)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot retrieved successfully"));
    }

    /// <summary>
    /// Create parking lot (admin can create for any owner)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Create(
        [FromBody] CreateParkingLotDto request,
        [FromQuery] Guid? ownerId,
        CancellationToken ct = default)
    {
        // Admin can specify ownerId, otherwise use admin's own ID
        var adminId = GetUserIdFromToken();
        var targetOwnerId = ownerId ?? adminId;

        var parkingLot = await _parkingLotService.CreateAsync(request, targetOwnerId, isAdmin: true, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = parkingLot.ParkingLotId },
            ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, Messages.ParkingLot.CreateSuccess)
        );
    }

    /// <summary>
    /// Update parking lot (admin can update any lot)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateParkingLotDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.UpdateAsync(id, request, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, Messages.ParkingLot.UpdateSuccess));
    }

    /// <summary>
    /// Toggle parking lot active status (admin)
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> ToggleActive(
        Guid id,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.ToggleActiveAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot status updated successfully"));
    }

    /// <summary>
    /// Approve a parking lot so it appears in public search.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Approve(
        Guid id,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.ApproveAsync(id, adminId, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot approved successfully"));
    }

    /// <summary>
    /// Reject a parking lot registration with a reason.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Reject(
        Guid id,
        [FromBody] RejectParkingLotDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.RejectAsync(id, adminId, request.Reason, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot rejected"));
    }

    /// <summary>
    /// Delete parking lot (admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        await _parkingLotService.DeleteAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.ParkingLot.DeleteSuccess));
    }

    /// <summary>
    /// Get bookings for a parking lot (admin view)
    /// </summary>
    [HttpGet("{id:guid}/bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLotBookingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLotBookingDto>>>> GetBookings(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _bookingService.GetBookingsByParkingLotAsync(id, adminId, isAdmin: true, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotBookingDto>>.SuccessResponse(result, Messages.Common.Success));
    }
}
