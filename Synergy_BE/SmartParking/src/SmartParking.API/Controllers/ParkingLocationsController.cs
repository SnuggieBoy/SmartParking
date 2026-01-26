using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Location;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers;

/// <summary>
/// Parking location management endpoints for map integration.
/// Security: Public read access. Admin manages locations.
/// </summary>
[Route("api/locations")]
public sealed class ParkingLocationsController : BaseApiController
{
    private readonly IParkingLocationService _locationService;

    public ParkingLocationsController(IParkingLocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Find nearby parking locations by GPS coordinates
    /// </summary>
    [AllowAnonymous]
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParkingLocationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParkingLocationResponseDto>>>> GetNearby(
        [FromQuery] decimal lat,
        [FromQuery] decimal lng,
        [FromQuery] double radius = 3000,
        CancellationToken ct = default)
    {
        var request = new NearbyLocationRequestDto(lat, lng, radius);
        var locations = await _locationService.GetNearbyAsync(request, ct);
        return Ok(ApiResponse<IEnumerable<ParkingLocationResponseDto>>.SuccessResponse(locations, "Nearby locations retrieved successfully"));
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Search locations by address
    /// </summary>
    [AllowAnonymous]
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLocationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLocationResponseDto>>>> Search(
        [FromQuery] string? province,
        [FromQuery] string? district,
        [FromQuery] string? ward,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var request = new SearchLocationRequestDto(province, district, ward, search, page, pageSize);
        var result = await _locationService.SearchAsync(request, ct);
        return Ok(ApiResponse<PagedResult<ParkingLocationResponseDto>>.SuccessResponse(result, "Locations retrieved successfully"));
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Get location by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var location = await _locationService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location retrieved successfully"));
    }

    /// <summary>
    /// PUBLIC ENDPOINT: Get location by parking lot ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("parking-lot/{parkingLotId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> GetByParkingLotId(Guid parkingLotId, CancellationToken ct = default)
    {
        var location = await _locationService.GetByParkingLotIdAsync(parkingLotId, ct);
        if (location == null)
        {
            return NotFound(ApiResponse<ParkingLocationResponseDto>.FailureResponse("Location not found for this parking lot"));
        }
        return Ok(ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location retrieved successfully"));
    }

    /// <summary>
    /// SECURITY: Only Admin can create locations
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> Create(
        [FromBody] CreateLocationDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var location = await _locationService.CreateAsync(request, userId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = location.LocationId },
            ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location created successfully")
        );
    }

    /// <summary>
    /// SECURITY: Only Admin can update locations
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateLocationDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var location = await _locationService.UpdateAsync(id, request, userId, isAdmin, ct);
        return Ok(ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location updated successfully"));
    }

    /// <summary>
    /// SECURITY: Only Admin can delete locations
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        await _locationService.DeleteAsync(id, userId, isAdmin, ct);
        return Ok(ApiResponse.SuccessResponse("Location deleted successfully"));
    }
}
