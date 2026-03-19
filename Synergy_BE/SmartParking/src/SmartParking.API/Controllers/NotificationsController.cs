using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// User notifications endpoints.
/// </summary>
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly IDevicePushTokenService _devicePushTokenService;

    public NotificationsController(INotificationService notificationService, IDevicePushTokenService devicePushTokenService)
    {
        _notificationService = notificationService;
        _devicePushTokenService = devicePushTokenService;
    }

    /// <summary>
    /// Get user's notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _notificationService.GetUserNotificationsAsync(userId, isRead, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(result, "Notifications retrieved successfully"));
    }

    /// <summary>
    /// Get notification summary (unread count)
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<NotificationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetSummary(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var summary = await _notificationService.GetNotificationSummaryAsync(userId, ct);
        return Ok(ApiResponse<NotificationSummaryDto>.SuccessResponse(summary, "Notification summary retrieved"));
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _notificationService.MarkAsReadAsync(id, userId, ct);
        return Ok(ApiResponse.SuccessResponse("Notification marked as read"));
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _notificationService.MarkAllAsReadAsync(userId, ct);
        return Ok(ApiResponse.SuccessResponse("All notifications marked as read"));
    }

    /// <summary>
    /// Đăng ký Expo Push Token cho thiết bị hiện tại.
    /// Gọi sau khi đăng nhập để nhận push notification.
    /// </summary>
    [HttpPost("device")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request, CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _devicePushTokenService.RegisterTokenAsync(userId, request.ExpoPushToken, request.Platform, ct);
        return Ok(ApiResponse.SuccessResponse("Đã đăng ký thiết bị nhận thông báo"));
    }
}
