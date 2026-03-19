using Microsoft.Extensions.Configuration;

namespace SmartParking.API.Helpers;

/// <summary>
/// Helper for httpOnly cookie auth (Web). Mobile continues using Bearer in body.
/// </summary>
public static class AuthCookieHelper
{
    public const string AccessTokenCookieName = "accessToken";
    public const string RefreshTokenCookieName = "refreshToken";
    public const string UseCookiesHeader = "X-Use-Cookies";

    public static void SetAuthCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken,
        IConfiguration configuration)
    {
        var accessExpiry = configuration.GetValue("JwtSettings:AccessTokenExpiryMinutes", 15);
        var refreshExpiry = configuration.GetValue("JwtSettings:RefreshTokenExpiryDays", 7);
        var accessOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(accessExpiry)
        };
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.FromDays(refreshExpiry)
        };
        response.Cookies.Append(AccessTokenCookieName, accessToken, accessOptions);
        response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshOptions);
    }

    public static void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Append(AccessTokenCookieName, "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.Zero,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
        response.Cookies.Append(RefreshTokenCookieName, "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            MaxAge = TimeSpan.Zero,
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }
}
