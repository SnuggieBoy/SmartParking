namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Gửi push notification qua Expo Push API (https://exp.host/--/api/v2/push/send).
/// </summary>
public interface IExpoPushService
{
    /// <summary>
    /// Gửi push notification đến một hoặc nhiều Expo Push Token.
    /// Không throw - log lỗi và tiếp tục.
    /// </summary>
    Task SendPushAsync(IReadOnlyList<string> expoPushTokens, string title, string body, object? data = null, CancellationToken ct = default);
}
