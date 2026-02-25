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

    /// <summary>
    /// Get revenue chart data (day, week, month, year)
    /// </summary>
    [HttpGet("revenue-chart")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RevenueChartDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RevenueChartDto>>>> GetRevenueChart(
        [FromQuery] string period = "week",
        CancellationToken ct = default)
    {
        var data = await _dashboardService.GetRevenueChartAsync(period, ct);
        return Ok(ApiResponse<IEnumerable<RevenueChartDto>>.SuccessResponse(data, "Revenue chart retrieved successfully"));
    }

    /// <summary>
    /// Get top parking lots by revenue
    /// </summary>
    [HttpGet("top-parking-lots")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopParkingLotChartDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TopParkingLotChartDto>>>> GetTopParkingLots(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var data = await _dashboardService.GetTopParkingLotsAsync(limit, ct);
        return Ok(ApiResponse<IEnumerable<TopParkingLotChartDto>>.SuccessResponse(data, "Top parking lots retrieved successfully"));
    }

    /// <summary>
    /// Get user distribution (Driver vs Owner)
    /// </summary>
    [HttpGet("user-distribution")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDistributionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDistributionDto>>>> GetUserDistribution(CancellationToken ct = default)
    {
        var data = await _dashboardService.GetUserDistributionAsync(ct);
        return Ok(ApiResponse<IEnumerable<UserDistributionDto>>.SuccessResponse(data, "User distribution retrieved successfully"));
    }

    /// <summary>
    /// Get recent reviews for dashboard
    /// </summary>
    [HttpGet("recent-reviews")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecentReviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RecentReviewDto>>>> GetRecentReviews(
        [FromQuery] int limit = 5,
        CancellationToken ct = default)
    {
        var data = await _dashboardService.GetRecentReviewsAsync(limit, ct);
        return Ok(ApiResponse<IEnumerable<RecentReviewDto>>.SuccessResponse(data, "Recent reviews retrieved successfully"));
    }
}
