using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; init; } = string.Empty;
}
