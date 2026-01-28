using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// Owner dashboard endpoints for statistics, earnings, and subscription management.
/// SECURITY: Only Owner or Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.OwnerOrAdmin)]
[Route("api/owners")]
public sealed class OwnerDashboardController : BaseApiController
{
    private readonly IOwnerDashboardService _dashboardService;

    public OwnerDashboardController(IOwnerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get owner dashboard summary (parking lots, bookings, earnings)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<OwnerDashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OwnerDashboardDto>>> GetDashboard(CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var dashboard = await _dashboardService.GetDashboardAsync(ownerId, ct);
        return Ok(ApiResponse<OwnerDashboardDto>.SuccessResponse(dashboard, "Owner dashboard retrieved successfully"));
    }

    /// <summary>
    /// Get owner earnings summary
    /// </summary>
    [HttpGet("earnings")]
    [ProducesResponseType(typeof(ApiResponse<OwnerEarningsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OwnerEarningsDto>>> GetEarnings(CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var earnings = await _dashboardService.GetEarningsAsync(ownerId, ct);
        return Ok(ApiResponse<OwnerEarningsDto>.SuccessResponse(earnings, "Owner earnings retrieved successfully"));
    }

    /// <summary>
    /// Get owner earnings history (monthly breakdown)
    /// </summary>
    [HttpGet("earnings/history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OwnerEarningsHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OwnerEarningsHistoryDto>>>> GetEarningsHistory(
        [FromQuery] int months = 12,
        CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var history = await _dashboardService.GetEarningsHistoryAsync(ownerId, months, ct);
        return Ok(ApiResponse<IEnumerable<OwnerEarningsHistoryDto>>.SuccessResponse(history, "Owner earnings history retrieved successfully"));
    }

    /// <summary>
    /// Get owner payout history
    /// </summary>
    [HttpGet("payouts")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OwnerPayoutDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OwnerPayoutDto>>>> GetPayouts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var payouts = await _dashboardService.GetPayoutsAsync(ownerId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<OwnerPayoutDto>>.SuccessResponse(payouts, "Owner payouts retrieved successfully"));
    }

    /// <summary>
    /// Get owner subscription status
    /// </summary>
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(ApiResponse<OwnerSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OwnerSubscriptionDto>>> GetSubscription(CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var subscription = await _dashboardService.GetSubscriptionAsync(ownerId, ct);
        
        if (subscription == null)
        {
            return NotFound(ApiResponse<OwnerSubscriptionDto>.FailureResponse("No active subscription found"));
        }

        return Ok(ApiResponse<OwnerSubscriptionDto>.SuccessResponse(subscription, "Owner subscription retrieved successfully"));
    }

    /// <summary>
    /// Renew owner subscription
    /// </summary>
    [HttpPost("subscription/renew")]
    [ProducesResponseType(typeof(ApiResponse<OwnerSubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OwnerSubscriptionDto>>> RenewSubscription(
        [FromBody] RenewSubscriptionDto request,
        CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var subscription = await _dashboardService.RenewSubscriptionAsync(ownerId, request, ct);
        return Ok(ApiResponse<OwnerSubscriptionDto>.SuccessResponse(subscription, "Subscription renewed successfully"));
    }

    /// <summary>
    /// Get statistics for a specific parking lot
    /// </summary>
    [HttpGet("parking-lots/{parkingLotId:guid}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotStatisticsDto>>> GetParkingLotStatistics(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var ownerId = GetUserIdFromToken();
        var statistics = await _dashboardService.GetParkingLotStatisticsAsync(parkingLotId, ownerId, ct);
        return Ok(ApiResponse<ParkingLotStatisticsDto>.SuccessResponse(statistics, "Parking lot statistics retrieved successfully"));
    }
}
