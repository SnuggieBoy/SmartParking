using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được quá 256 ký tự")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [StringLength(256, ErrorMessage = "Mật khẩu không được quá 256 ký tự")]
    public string Password { get; init; } = string.Empty;
}
