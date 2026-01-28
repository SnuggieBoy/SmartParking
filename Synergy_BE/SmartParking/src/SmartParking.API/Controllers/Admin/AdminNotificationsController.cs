using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for notifications management.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/notifications")]
public sealed class AdminNotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> SendNotification(
        [FromBody] SendNotificationDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var notification = await _notificationService.SendNotificationAsync(request, adminId, ct);
        return Ok(ApiResponse<NotificationDto>.SuccessResponse(notification, "Notification sent successfully"));
    }

    /// <summary>
    /// Broadcast notification to all users
    /// </summary>
    [HttpPost("broadcast")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<int>>> BroadcastNotification(
        [FromBody] BroadcastNotificationDto request,
        CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        var count = await _notificationService.BroadcastNotificationAsync(request, adminId, ct);
        return Ok(ApiResponse<int>.SuccessResponse(count, $"Notification broadcasted to {count} users"));
    }
}
