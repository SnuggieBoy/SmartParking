using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.ParkingLocation;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.ParkingLocation;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers;

/// <summary>
/// Parking location management and map-based search endpoints.
/// Security: Users can search nearby. Admin manages locations.
/// </summary>
[Route("api/parking-locations")]
public sealed class ParkingLocationsController : BaseApiController
{
    private readonly IParkingLocationService _service;

    public ParkingLocationsController(IParkingLocationService service)
    {
        _service = service;
    }

    /// <summary>
    /// ADMIN ONLY: Retrieves all parking locations (including inactive).
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLocationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLocationResponseDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ParkingLocationResponseDto>>.SuccessResponse(
            result,
            "Parking locations retrieved successfully"));
    }

    /// <summary>
    /// USER + ADMIN: Finds nearby parking locations based on current coordinates.
    /// Returns active locations with available slots within specified radius.
    /// Uses Haversine formula for distance calculation.
    /// </summary>
    /// <param name="lat">User's current latitude (-90 to 90)</param>
    /// <param name="lng">User's current longitude (-180 to 180)</param>
    /// <param name="radius">Search radius in meters (default: 3000m = 3km)</param>
    [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ParkingLocationResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<ParkingLocationResponseDto>>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 3000,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _service.GetNearbyAsync(lat, lng, radius, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ParkingLocationResponseDto>>.SuccessResponse(
            result,
            $"Found {result.TotalCount} parking location(s) within {radius}m (Page {result.Page}/{result.TotalPages})"));
    }

    /// <summary>
    /// ADMIN ONLY: Creates a new parking location.
    /// Validates coordinates, address details, and slot availability.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> Create(
        [FromBody] CreateParkingLocationRequest request,
        CancellationToken ct = default)
    {
        // Additional validation: AvailableSlots <= TotalSlots
        if (request.AvailableSlots > request.TotalSlots)
        {
            return BadRequest(ApiResponse.FailureResponse(
                "Available slots cannot exceed total slots",
                new[] { "AvailableSlots must be less than or equal to TotalSlots" }));
        }

        var dto = new ParkingLocationCreateDto(
            Name: request.Name,
            Description: request.Description,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            Province: request.Province,
            District: request.District,
            Ward: request.Ward,
            Street: request.Street,
            Area: request.Area,
            FullAddress: request.FullAddress,
            TotalSlots: request.TotalSlots,
            AvailableSlots: request.AvailableSlots,
            PricePerHour: request.PricePerHour
        );

        await _service.CreateAsync(dto, ct);

        return CreatedAtAction(
            nameof(GetAll),
            null,
            ApiResponse.SuccessResponse("Parking location created successfully"));
    }
}
