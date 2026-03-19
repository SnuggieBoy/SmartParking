namespace SmartParking.Domain.Entities;

/// <summary>
/// Lưu Expo Push Token theo user để gửi push notification qua Expo Push API.
/// Một user có thể có nhiều thiết bị (nhiều token).
/// </summary>
public partial class DevicePushToken
{
    public Guid DevicePushTokenId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Expo Push Token (ExponentPushToken[xxx] hoặc https://exp.host/...)
    /// </summary>
    public string ExpoPushToken { get; set; } = null!;

    /// <summary>
    /// Platform: ios, android, web
    /// </summary>
    public string? Platform { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastUsedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual User User { get; set; } = null!;
}
