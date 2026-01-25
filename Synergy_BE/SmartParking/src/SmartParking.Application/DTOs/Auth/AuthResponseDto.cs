namespace SmartParking.Application.DTOs.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User
);

public sealed record UserInfoDto(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    string Role,
    bool IsActive
);
