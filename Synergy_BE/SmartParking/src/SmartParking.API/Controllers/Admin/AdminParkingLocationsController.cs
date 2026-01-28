using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Location;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for parking location management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/locations")]
public sealed class AdminParkingLocationsController : BaseApiController
{
    private readonly IParkingLocationService _locationService;

    public AdminParkingLocationsController(IParkingLocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Get all parking locations (admin view)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLocationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLocationResponseDto>>>> GetAll(
        [FromQuery] string? province,
        [FromQuery] string? district,
        [FromQuery] string? ward,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var request = new SearchLocationRequestDto(province, district, ward, search, page, pageSize);
        var result = await _locationService.SearchAsync(request, ct);
        return Ok(ApiResponse<PagedResult<ParkingLocationResponseDto>>.SuccessResponse(result, "Locations retrieved successfully"));
    }

    /// <summary>
    /// Get location by ID (admin view)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var location = await _locationService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location retrieved successfully"));
    }

    /// <summary>
    /// Create parking location (admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> Create(
        [FromBody] CreateLocationDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var location = await _locationService.CreateAsync(request, adminId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = location.LocationId },
            ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location created successfully")
        );
    }

    /// <summary>
    /// Update parking location (admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLocationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLocationResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateLocationDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var location = await _locationService.UpdateAsync(id, request, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<ParkingLocationResponseDto>.SuccessResponse(location, "Location updated successfully"));
    }

    /// <summary>
    /// Delete parking location (admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        await _locationService.DeleteAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse.SuccessResponse("Location deleted successfully"));
    }
}
