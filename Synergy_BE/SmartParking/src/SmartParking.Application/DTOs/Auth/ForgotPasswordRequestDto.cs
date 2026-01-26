namespace SmartParking.Application.DTOs.Auth;

/// <summary>
/// DTO for forgot password request (sends OTP to email)
/// </summary>
public sealed record ForgotPasswordRequestDto(string Email);
