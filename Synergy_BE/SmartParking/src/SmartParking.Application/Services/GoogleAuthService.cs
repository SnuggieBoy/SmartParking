using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.Interfaces.Services;
using System.Net.Http.Json;

namespace SmartParking.Application.Services;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthSettings _settings;

    public GoogleAuthService(HttpClient httpClient, IOptions<GoogleOAuthSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string googleToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(googleToken)) return null;

        // Google tokeninfo: id_token= (from native SDK) hoặc access_token= (từ OAuth flow, e.g. Expo)
        var isLikelyIdToken = googleToken.Contains('.') && googleToken.Count(c => c == '.') == 2;
        var url = isLikelyIdToken
            ? $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(googleToken)}"
            : $"https://oauth2.googleapis.com/tokeninfo?access_token={Uri.EscapeDataString(googleToken)}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<GoogleTokenInfoResponse>(url, ct);
            if (response == null || response.aud != _settings.ClientId)
                return null;
            return new GoogleUserInfo(
                Email: response.email ?? string.Empty,
                Name: response.name ?? string.Empty,
                GoogleUserId: response.sub ?? string.Empty
            );
        }
        catch
        {
            if (isLikelyIdToken) return null;
            try
            {
                var fallback = await _httpClient.GetFromJsonAsync<GoogleTokenInfoResponse>(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(googleToken)}", ct);
                if (fallback == null || fallback.aud != _settings.ClientId) return null;
                return new GoogleUserInfo(
                    Email: fallback.email ?? string.Empty,
                    Name: fallback.name ?? string.Empty,
                    GoogleUserId: fallback.sub ?? string.Empty
                );
            }
            catch
            {
                return null;
            }
        }
    }

    private sealed record GoogleTokenInfoResponse(
        string? sub,
        string? email,
        string? name,
        string? aud
    );
}
