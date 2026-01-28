using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Admin;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for system settings.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/settings")]
public sealed class AdminSettingsController : BaseApiController
{
    private readonly IAdminService _adminService;

    public AdminSettingsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get current system settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> GetSettings(CancellationToken ct = default)
    {
        var settings = await _adminService.GetSettingsAsync(ct);
        return Ok(ApiResponse<SystemSettingsDto>.SuccessResponse(settings, "System settings retrieved successfully"));
    }

    /// <summary>
    /// Update system settings (currently read-only from appsettings.json)
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<SystemSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> UpdateSettings(
        [FromBody] UpdateSystemSettingsDto request,
        CancellationToken ct = default)
    {
        var settings = await _adminService.UpdateSettingsAsync(request, ct);
        return Ok(ApiResponse<SystemSettingsDto>.SuccessResponse(settings, "System settings updated successfully"));
    }
}
