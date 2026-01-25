using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed class GoogleLoginRequest
{
    [Required(ErrorMessage = "Google token is required")]
    public string GoogleToken { get; init; } = string.Empty;
}
