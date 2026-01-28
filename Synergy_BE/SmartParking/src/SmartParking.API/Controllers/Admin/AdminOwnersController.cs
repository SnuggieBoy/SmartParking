using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for managing User -> Owner upgrade requests.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/owners")]
public sealed class AdminOwnersController : BaseApiController
{
    private readonly IOwnerUpgradeService _ownerUpgradeService;

    public AdminOwnersController(IOwnerUpgradeService ownerUpgradeService)
    {
        _ownerUpgradeService = ownerUpgradeService;
    }

    /// <summary>
    /// ADMIN: Get paged list of owner upgrade requests.
    /// </summary>
    [HttpGet("upgrade-requests")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OwnerUpgradeRequestResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OwnerUpgradeRequestResponseDto>>>> GetUpgradeRequests(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new OwnerUpgradeRequestFilterDto(status, page, pageSize);
        var result = await _ownerUpgradeService.GetAllAsync(filter, ct);

        return Ok(ApiResponse<PagedResult<OwnerUpgradeRequestResponseDto>>.SuccessResponse(
            result,
            "Owner upgrade requests retrieved successfully"));
    }

    /// <summary>
    /// ADMIN: Approve an owner upgrade request.
    /// </summary>
    [HttpPost("upgrade-requests/{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<OwnerUpgradeRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerUpgradeRequestResponseDto>>> ApproveUpgradeRequest(
        Guid id,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _ownerUpgradeService.ApproveAsync(id, adminId, ct);

        return Ok(ApiResponse<OwnerUpgradeRequestResponseDto>.SuccessResponse(
            result,
            "Owner upgrade request approved successfully"));
    }

    /// <summary>
    /// ADMIN: Reject an owner upgrade request with a reason.
    /// </summary>
    [HttpPost("upgrade-requests/{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<OwnerUpgradeRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerUpgradeRequestResponseDto>>> RejectUpgradeRequest(
        Guid id,
        [FromBody] RejectOwnerUpgradeRequestDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var result = await _ownerUpgradeService.RejectAsync(id, adminId, request, ct);

        return Ok(ApiResponse<OwnerUpgradeRequestResponseDto>.SuccessResponse(
            result,
            "Owner upgrade request rejected"));
    }
}

