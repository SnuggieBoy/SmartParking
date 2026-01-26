namespace SmartParking.Application.DTOs.Auth;

/// <summary>
/// DTO for resetting password with OTP verification
/// </summary>
public sealed record ResetPasswordRequestDto(
    string Email,
    string OtpCode,
    string NewPassword
);
