using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.Owner;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers;

/// <summary>
/// Endpoints for Users who want to upgrade to become Owners (P2P host).
/// </summary>
[Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
[Route("api/owners")]
public sealed class OwnersController : BaseApiController
{
    private readonly IOwnerUpgradeService _ownerUpgradeService;

    public OwnersController(IOwnerUpgradeService ownerUpgradeService)
    {
        _ownerUpgradeService = ownerUpgradeService;
    }

    /// <summary>
    /// PUBLIC (authenticated): Get available owner subscription plans (monthly/yearly).
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OwnerPlanDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OwnerPlanDto>>>> GetPlans(CancellationToken ct = default)
    {
        var plans = await _ownerUpgradeService.GetPlansAsync(ct);
        return Ok(ApiResponse<IEnumerable<OwnerPlanDto>>.SuccessResponse(plans, "Owner plans retrieved successfully"));
    }

    /// <summary>
    /// USER: Create a new request to upgrade from User to Owner.
    /// </summary>
    [HttpPost("upgrade-request")]
    [ProducesResponseType(typeof(ApiResponse<OwnerUpgradeRequestResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OwnerUpgradeRequestResponseDto>>> CreateUpgradeRequest(
        [FromBody] CreateOwnerUpgradeRequestModel model,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();

        var dto = new CreateOwnerUpgradeRequestDto(
            ParkingLotName: model.ParkingLotName,
            ParkingLotAddress: model.ParkingLotAddress,
            Latitude: model.Latitude,
            Longitude: model.Longitude,
            PlanType: model.PlanType,
            PaymentTransactionId: model.PaymentTransactionId,
            ImageUrl: model.ImageUrl,
            Description: model.Description);

        var result = await _ownerUpgradeService.CreateRequestAsync(userId, dto, ct);

        return CreatedAtAction(
            nameof(GetMyLatestUpgradeRequest),
            null,
            ApiResponse<OwnerUpgradeRequestResponseDto>.SuccessResponse(
                result,
                "Owner upgrade request created successfully"));
    }

    /// <summary>
    /// USER: Get latest upgrade request for the current user (if any).
    /// </summary>
    [HttpGet("upgrade-request/my")]
    [ProducesResponseType(typeof(ApiResponse<OwnerUpgradeRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerUpgradeRequestResponseDto>>> GetMyLatestUpgradeRequest(
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var request = await _ownerUpgradeService.GetMyLatestRequestAsync(userId, ct);

        if (request is null)
        {
            return NotFound(ApiResponse<OwnerUpgradeRequestResponseDto>.FailureResponse("No upgrade request found for current user"));
        }

        return Ok(ApiResponse<OwnerUpgradeRequestResponseDto>.SuccessResponse(
            request,
            "Owner upgrade request retrieved successfully"));
    }
}

