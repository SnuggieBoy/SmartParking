using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.ParkingLot;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

[ApiController]
[Route("api/parking-lots")]
public sealed class ParkingLotsController : ControllerBase
{
    private readonly IParkingLotService _parkingLotService;

    public ParkingLotsController(IParkingLotService parkingLotService)
    {
        _parkingLotService = parkingLotService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParkingLotDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParkingLotDto>>>> GetAll(
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var parkingLots = await _parkingLotService.GetAllAsync(activeOnly, ct);
        return Ok(ApiResponse<IEnumerable<ParkingLotDto>>.SuccessResponse(parkingLots, "Parking lots retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotDto>>> GetById(Guid id, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<ParkingLotDto>.SuccessResponse(parkingLot, "Parking lot retrieved successfully"));
    }

    [Authorize(Roles = $"{AuthConstants.Roles.Owner},{AuthConstants.Roles.Admin}")]
    [HttpGet("my-parking-lots")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParkingLotDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ParkingLotDto>>>> GetMyParkingLots(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var parkingLots = await _parkingLotService.GetMyParkingLotsAsync(userId, ct);
        return Ok(ApiResponse<IEnumerable<ParkingLotDto>>.SuccessResponse(parkingLots, "My parking lots retrieved successfully"));
    }

    [Authorize(Roles = $"{AuthConstants.Roles.Owner},{AuthConstants.Roles.Admin}")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ParkingLotDto>>> Create(
        [FromBody] CreateParkingLotDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.CreateAsync(request, userId, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = parkingLot.ParkingLotId },
            ApiResponse<ParkingLotDto>.SuccessResponse(parkingLot, Messages.ParkingLot.CreateSuccess)
        );
    }

    [Authorize(Roles = $"{AuthConstants.Roles.Owner},{AuthConstants.Roles.Admin}")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotDto>>> Update(
        Guid id,
        [FromBody] UpdateParkingLotDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var parkingLot = await _parkingLotService.UpdateAsync(id, request, userId, ct);
        return Ok(ApiResponse<ParkingLotDto>.SuccessResponse(parkingLot, Messages.ParkingLot.UpdateSuccess));
    }

    [Authorize(Roles = $"{AuthConstants.Roles.Owner},{AuthConstants.Roles.Admin}")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _parkingLotService.DeleteAsync(id, userId, ct);
        return Ok(ApiResponse.SuccessResponse(Messages.ParkingLot.DeleteSuccess));
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(userIdClaim!);
    }
}
