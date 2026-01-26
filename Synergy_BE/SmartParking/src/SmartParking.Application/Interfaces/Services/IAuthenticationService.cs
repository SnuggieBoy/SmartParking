using SmartParking.Application.DTOs.Auth;

namespace SmartParking.Application.Interfaces.Services;

public interface IAuthenticationService
{
    // OTP-based registration flow (required)
    Task RegisterRequestOtpAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> VerifyOtpAndRegisterAsync(VerifyOtpRequestDto request, CancellationToken ct = default);
    Task ResendOtpAsync(ResendOtpRequestDto request, CancellationToken ct = default);
    
    // Authentication
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    
    // Password Reset with OTP (replaces change-password)
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
}
