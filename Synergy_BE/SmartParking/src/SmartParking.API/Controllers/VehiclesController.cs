using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Vehicle;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("my-vehicles")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<VehicleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetMyVehicles(
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var vehicles = await _vehicleService.GetMyVehiclesAsync(userId, activeOnly, ct);
        return Ok(ApiResponse<IEnumerable<VehicleDto>>.SuccessResponse(vehicles, "Vehicles retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var vehicle = await _vehicleService.GetByIdAsync(id, userId, ct);
        return Ok(ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Vehicle retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> Create(
        [FromBody] CreateVehicleDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var vehicle = await _vehicleService.CreateAsync(request, userId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = vehicle.VehicleId },
            ApiResponse<VehicleDto>.SuccessResponse(vehicle, Messages.Vehicle.CreateSuccess)
        );
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> Update(
        Guid id,
        [FromBody] UpdateVehicleDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var vehicle = await _vehicleService.UpdateAsync(id, request, userId, ct);
        return Ok(ApiResponse<VehicleDto>.SuccessResponse(vehicle, Messages.Vehicle.UpdateSuccess));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _vehicleService.DeleteAsync(id, userId, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.Vehicle.DeleteSuccess));
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(userIdClaim!);
    }
}
