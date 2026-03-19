using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

/// <summary>
/// RefreshToken is optional when using httpOnly cookies (Web sends refresh via cookie).
/// Mobile sends refreshToken in body.
/// </summary>
public sealed class RefreshTokenRequest
{
    [MaxLength(512, ErrorMessage = "Refresh token không được quá 512 ký tự")]
    public string? RefreshToken { get; init; }
}
