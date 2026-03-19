namespace SmartParking.API.Models.Auth;

/// <summary>
/// RefreshToken is optional when using httpOnly cookies (Web sends refresh via cookie).
/// Mobile sends refreshToken in body.
/// </summary>
public sealed class RefreshTokenRequest
{
    public string? RefreshToken { get; init; }
}
