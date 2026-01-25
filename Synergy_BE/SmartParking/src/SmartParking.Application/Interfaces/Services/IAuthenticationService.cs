using SmartParking.Application.DTOs.Auth;

namespace SmartParking.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken ct = default);
}
