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
        try
        {
            var response = await _httpClient.GetFromJsonAsync<GoogleTokenInfoResponse>(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={googleToken}", ct);

            if (response == null || response.aud != _settings.ClientId)
            {
                return null;
            }

            return new GoogleUserInfo(
                Email: response.email ?? string.Empty,
                Name: response.name ?? string.Empty,
                GoogleUserId: response.sub ?? string.Empty
            );
        }
        catch
        {
            return null;
        }
    }

    private sealed record GoogleTokenInfoResponse(
        string? sub,
        string? email,
        string? name,
        string? aud
    );
}
