using SmartParking.Application.DTOs.Auth;

namespace SmartParking.Application.Interfaces.Services;

public interface IAuthenticationService
{
    // Original registration (kept for backward compatibility, but should use RegisterRequestOtpAsync)
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    
    // New OTP-based registration flow
    Task RegisterRequestOtpAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> VerifyOtpAndRegisterAsync(VerifyOtpRequestDto request, CancellationToken ct = default);
    Task ResendOtpAsync(ResendOtpRequestDto request, CancellationToken ct = default);
    
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);
}
