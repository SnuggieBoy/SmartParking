using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

/// <summary>
/// Request model for forgot password (sends OTP to email)
/// </summary>
public sealed record ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; init; }
}
