using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Dashboard;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// Admin dashboard endpoints for statistics and activities
/// SECURITY: Only Admin can access these endpoints
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public AdminDashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get users summary (total users, hosts, drivers, new users today)
    /// </summary>
    [HttpGet("users-summary")]
    [ProducesResponseType(typeof(ApiResponse<UsersSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UsersSummaryDto>>> GetUsersSummary(CancellationToken ct = default)
    {
        var summary = await _dashboardService.GetUsersSummaryAsync(ct);
        return Ok(ApiResponse<UsersSummaryDto>.SuccessResponse(summary, "Users summary retrieved successfully"));
    }

    /// <summary>
    /// Get parking lots summary (active/inactive counts)
    /// </summary>
    [HttpGet("parking-lots-summary")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ParkingLotsSummaryDto>>> GetParkingLotsSummary(CancellationToken ct = default)
    {
        var summary = await _dashboardService.GetParkingLotsSummaryAsync(ct);
        return Ok(ApiResponse<ParkingLotsSummaryDto>.SuccessResponse(summary, "Parking lots summary retrieved successfully"));
    }

    /// <summary>
    /// Get revenue and commission statistics for today
    /// </summary>
    [HttpGet("revenue-today")]
    [ProducesResponseType(typeof(ApiResponse<RevenueTodayDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueTodayDto>>> GetRevenueToday(CancellationToken ct = default)
    {
        var revenue = await _dashboardService.GetRevenueTodayAsync(ct);
        return Ok(ApiResponse<RevenueTodayDto>.SuccessResponse(revenue, "Revenue today retrieved successfully"));
    }

    /// <summary>
    /// Get bookings summary (active, pending, completed today)
    /// </summary>
    [HttpGet("bookings-summary")]
    [ProducesResponseType(typeof(ApiResponse<BookingsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingsSummaryDto>>> GetBookingsSummary(CancellationToken ct = default)
    {
        var summary = await _dashboardService.GetBookingsSummaryAsync(ct);
        return Ok(ApiResponse<BookingsSummaryDto>.SuccessResponse(summary, "Bookings summary retrieved successfully"));
    }

    /// <summary>
    /// Get recent activities (bookings, payments, user registrations, etc.)
    /// </summary>
    [HttpGet("recent-activities")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecentActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RecentActivityDto>>>> GetRecentActivities(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var activities = await _dashboardService.GetRecentActivitiesAsync(limit, ct);
        return Ok(ApiResponse<IEnumerable<RecentActivityDto>>.SuccessResponse(activities, "Recent activities retrieved successfully"));
    }
}
