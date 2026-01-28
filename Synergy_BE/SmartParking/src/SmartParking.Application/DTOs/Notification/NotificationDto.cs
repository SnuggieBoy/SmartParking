using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Notification;

/// <summary>
/// Notification response DTO
/// </summary>
public sealed record NotificationDto(
    Guid NotificationId,
    string Title,
    string Message,
    string Type,
    string? Data,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt
);

/// <summary>
/// Send notification request (admin)
/// </summary>
public sealed record SendNotificationDto(
    Guid? UserId, // null = broadcast to all

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    string Title,

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000)]
    string Message,

    string Type = "Info", // Info, Success, Warning, Error, Booking, Payment, System

    string? Data = null
);

/// <summary>
/// Broadcast notification request (admin)
/// </summary>
public sealed record BroadcastNotificationDto(
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    string Title,

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000)]
    string Message,

    string Type = "System",

    string? Data = null
);

/// <summary>
/// Notification summary
/// </summary>
public sealed record NotificationSummaryDto(
    int TotalCount,
    int UnreadCount,
    IEnumerable<NotificationDto> RecentNotifications
);
