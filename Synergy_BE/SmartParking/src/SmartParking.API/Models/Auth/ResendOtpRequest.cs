using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed record ResendOtpRequest
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được quá 256 ký tự")]
    public required string Email { get; init; }
}
