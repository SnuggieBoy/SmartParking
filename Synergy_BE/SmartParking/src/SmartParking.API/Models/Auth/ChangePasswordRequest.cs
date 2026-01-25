using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed record ChangePasswordRequest
{
    [Required(ErrorMessage = "Old password is required")]
    public required string OldPassword { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
    public required string NewPassword { get; init; }

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirm password do not match")]
    public required string ConfirmPassword { get; init; }
}
