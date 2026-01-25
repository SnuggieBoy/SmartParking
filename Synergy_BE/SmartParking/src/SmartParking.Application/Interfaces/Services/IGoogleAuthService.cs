namespace SmartParking.Application.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string googleToken, CancellationToken ct = default);
}

public sealed record GoogleUserInfo(
    string Email,
    string Name,
    string GoogleUserId
);
