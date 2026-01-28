using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Notification;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for managing notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Get user's notifications
    /// </summary>
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool? isRead, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Get notification summary (count unread)
    /// </summary>
    Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Send notification to a user
    /// </summary>
    Task<NotificationDto> SendNotificationAsync(SendNotificationDto request, Guid? senderId, CancellationToken ct = default);

    /// <summary>
    /// Broadcast notification to all users
    /// </summary>
    Task<int> BroadcastNotificationAsync(BroadcastNotificationDto request, Guid adminId, CancellationToken ct = default);

    /// <summary>
    /// Internal: Create booking-related notification
    /// </summary>
    Task CreateBookingNotificationAsync(Guid userId, string title, string message, Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// Internal: Create payment-related notification
    /// </summary>
    Task CreatePaymentNotificationAsync(Guid userId, string title, string message, Guid paymentId, CancellationToken ct = default);
}
