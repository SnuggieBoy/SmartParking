using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [StringLength(100, ErrorMessage = "Mật khẩu không được quá 100 ký tự")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
