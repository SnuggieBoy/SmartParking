namespace SmartParking.Application.DTOs.Auth;

/// <summary>
/// DTO for registration request - triggers OTP sending
/// </summary>
public sealed record RegisterRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Password
);

/// <summary>
/// DTO for OTP verification after registration request
/// </summary>
public sealed record VerifyOtpRequestDto(
    string Email,
    string OtpCode
);

/// <summary>
/// DTO for resending OTP if expired or not received
/// </summary>
public sealed record ResendOtpRequestDto(
    string Email
);
