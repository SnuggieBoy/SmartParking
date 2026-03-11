using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.ParkingLot;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

/// <summary>
/// Parking lot management endpoints.
/// Security: Public read access. Owners manage own lots. Admin has full access.
/// </summary>
[Route("api/parking-lots")]
public sealed class ParkingLotsController : BaseApiController
{
    private readonly IParkingLotService _parkingLotService;
    private readonly IBookingService _bookingService;

    public ParkingLotsController(IParkingLotService parkingLotService, IBookingService bookingService)
    {
        _parkingLotService = parkingLotService;
        _bookingService = bookingService;
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Find nearby parking lots around a given location.
    /// This is used by mobile app to show closest lots and then open navigation.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParkingLotResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParkingLotResponseDto>>>> GetNearby(
        [FromQuery] decimal lat,
        [FromQuery] decimal lng,
        [FromQuery] decimal radiusKm = 5,
        [FromQuery] int maxResults = 20,
        CancellationToken ct = default)
    {
        var lots = await _parkingLotService.GetNearbyAsync(lat, lng, radiusKm, maxResults, ct);
        return Ok(ApiResponse<IEnumerable<ParkingLotResponseDto>>.SuccessResponse(lots, "Nearby parking lots retrieved successfully"));
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Anyone can view available parking lots (paginated, filterable)
    /// </summary>
    [AllowAnonymous]
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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        // Public listing: allow client to control filtering completely
        // Previously defaulted to Approved/Active, but this hid data if DB used different conventions (e.g. "Active" vs "Approved")
        var effectiveIsActive = isActive; 
        var effectiveStatus = status;

        var filter = new ParkingLotFilterDto(search, effectiveIsActive, effectiveStatus, provinceCode, province, wardCode, ward, null, page, pageSize);
        var result = await _parkingLotService.GetAllAsync(filter, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotResponseDto>>.SuccessResponse(result, "Parking lots retrieved successfully"));
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Anyone can view parking lot details.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot retrieved successfully"));
    }

    /// <summary>
    /// SECURITY: Owner OR Admin can view their parking lots.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParkingLotResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParkingLotResponseDto>>>> GetMyParkingLots(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var parkingLots = await _parkingLotService.GetMyParkingLotsAsync(userId, ct);
        return Ok(ApiResponse<IEnumerable<ParkingLotResponseDto>>.SuccessResponse(parkingLots, "My parking lots retrieved successfully"));
    }

    /// <summary>
    /// SECURITY: Owner OR Admin can create parking lots.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Create(
        [FromBody] CreateParkingLotDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.CreateAsync(request, userId, isAdmin: false, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = parkingLot.ParkingLotId },
            ApiResponse<ParkingLotResponseDto>.SuccessResponse(
                parkingLot,
                "Parking lot created. Waiting for admin approval.")
        );
    }

    /// <summary>
    /// SECURITY: Only lot owner OR Admin can update.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateParkingLotDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var parkingLot = await _parkingLotService.UpdateAsync(id, request, userId, isAdmin, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, Messages.ParkingLot.UpdateSuccess));
    }

    /// <summary>
    /// SECURITY: Toggle parking lot active status
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpPatch("{id:guid}/toggle-active")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotResponseDto>>> ToggleActive(
        Guid id,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var parkingLot = await _parkingLotService.ToggleActiveAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse<ParkingLotResponseDto>.SuccessResponse(parkingLot, "Parking lot status updated successfully"));
    }

    /// <summary>
    /// SECURITY: Only lot owner OR Admin can delete.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        await _parkingLotService.DeleteAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.ParkingLot.DeleteSuccess));
    }

    /// <summary>
    /// SECURITY: Only lot owner OR Admin can view bookings of a parking lot.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
    [HttpGet("{id:guid}/bookings")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLotBookingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLotBookingDto>>>> GetBookings(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = User.IsInRole(AuthConstants.Roles.Admin);
        var result = await _bookingService.GetBookingsByParkingLotAsync(id, userId, isAdmin, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ParkingLotBookingDto>>.SuccessResponse(result, Messages.Common.Success));
    }
}
