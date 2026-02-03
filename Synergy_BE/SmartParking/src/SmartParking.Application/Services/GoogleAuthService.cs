using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.Application.Services;

/// <summary>
/// Google Authentication Service - Uses Google.Apis.Auth SDK to validate ID tokens
/// (Same pattern as eduprompt project)
/// </summary>
public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleOAuthSettings _settings;

    public GoogleAuthService(IOptions<GoogleOAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string googleToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(googleToken)) return null;

        try
        {
            // Use Google.Apis.Auth SDK to validate ID token (same as eduprompt)
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, validationSettings);
            
            if (payload == null)
            {
                return null;
            }

            return new GoogleUserInfo(
                Email: payload.Email ?? string.Empty,
                Name: payload.Name ?? string.Empty,
                GoogleUserId: payload.Subject ?? string.Empty
            );
        }
        catch (InvalidJwtException)
        {
            // Invalid ID token
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
