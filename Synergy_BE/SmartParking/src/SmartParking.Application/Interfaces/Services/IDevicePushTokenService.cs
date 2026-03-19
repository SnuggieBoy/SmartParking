namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service đăng ký và quản lý Expo Push Token theo user.
/// </summary>
public interface IDevicePushTokenService
{
    /// <summary>
    /// Đăng ký hoặc cập nhật Expo Push Token cho user.
    /// Nếu token đã tồn tại thì cập nhật LastUsedAt.
    /// </summary>
    Task RegisterTokenAsync(Guid userId, string expoPushToken, string? platform = null, CancellationToken ct = default);

    /// <summary>
    /// Lấy danh sách token còn active của user để gửi push.
    /// </summary>
    Task<IReadOnlyList<string>> GetActiveTokensForUserAsync(Guid userId, CancellationToken ct = default);
}
