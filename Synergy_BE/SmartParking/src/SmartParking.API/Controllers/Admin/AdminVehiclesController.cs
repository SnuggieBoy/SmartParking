using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Vehicle;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for vehicle management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/vehicles")]
public sealed class AdminVehiclesController : BaseApiController
{
    private readonly IVehicleService _vehicleService;

    public AdminVehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Get vehicle by ID (admin view)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var vehicle = await _vehicleService.GetByIdAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Vehicle retrieved successfully"));
    }

    /// <summary>
    /// Get vehicles by user ID (admin view)
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VehicleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetByUserId(
        Guid userId,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var vehicles = await _vehicleService.GetMyVehiclesAsync(userId, activeOnly, ct);
        return Ok(ApiResponse<IEnumerable<VehicleDto>>.SuccessResponse(vehicles, "Vehicles retrieved successfully"));
    }

    /// <summary>
    /// Update vehicle (admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> Update(
        Guid id,
        [FromBody] UpdateVehicleDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var vehicle = await _vehicleService.UpdateAsync(id, request, adminId, isAdmin: true, ct);
        return Ok(ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Vehicle updated successfully"));
    }

    /// <summary>
    /// Delete vehicle (admin)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        await _vehicleService.DeleteAsync(id, adminId, isAdmin: true, ct);
        return Ok(ApiResponse.SuccessResponse("Vehicle deleted successfully"));
    }
}
