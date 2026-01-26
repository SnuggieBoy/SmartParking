using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed record ResendOtpRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; init; }
}
