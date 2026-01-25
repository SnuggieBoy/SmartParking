namespace SmartParking.Application.DTOs.Auth;

public sealed record ChangePasswordRequestDto(
    string OldPassword,
    string NewPassword
);
