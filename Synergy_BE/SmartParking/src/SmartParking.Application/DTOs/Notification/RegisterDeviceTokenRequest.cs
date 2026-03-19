using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Notification;

/// <summary>
/// Request đăng ký Expo Push Token cho thiết bị hiện tại.
/// </summary>
public sealed record RegisterDeviceTokenRequest(
    [Required(ErrorMessage = "Token thiết bị là bắt buộc")]
    [MaxLength(500)]
    string ExpoPushToken,

    [MaxLength(20)]
    string? Platform = null // ios, android, web
);
