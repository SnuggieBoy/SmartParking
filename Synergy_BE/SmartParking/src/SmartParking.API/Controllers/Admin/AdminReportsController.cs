using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Admin;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for reports and analytics.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/reports")]
public sealed class AdminReportsController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminReportsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get revenue report
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(ApiResponse<RevenueReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RevenueReportDto>>> GetRevenueReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string period = "monthly", // daily, weekly, monthly
        CancellationToken ct = default)
    {
        var report = await _adminService.GetRevenueReportAsync(fromDate, toDate, period, ct);
        return Ok(ApiResponse<RevenueReportDto>.SuccessResponse(report, "Revenue report retrieved successfully"));
    }

    /// <summary>
    /// Get bookings report
    /// </summary>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(ApiResponse<BookingsReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BookingsReportDto>>> GetBookingsReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var report = await _adminService.GetBookingsReportAsync(fromDate, toDate, ct);
        return Ok(ApiResponse<BookingsReportDto>.SuccessResponse(report, "Bookings report retrieved successfully"));
    }
}
