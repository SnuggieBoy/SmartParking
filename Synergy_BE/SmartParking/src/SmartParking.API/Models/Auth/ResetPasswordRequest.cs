using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

/// <summary>
/// Request model for resetting password with OTP
/// </summary>
public sealed record ResetPasswordRequest
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được quá 256 ký tự")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Mã OTP là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải đúng 6 chữ số")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số")]
    public required string OtpCode { get; init; }
    
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [StringLength(256, ErrorMessage = "Mật khẩu không được quá 256 ký tự")]
    public required string NewPassword { get; init; }
    
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public required string ConfirmPassword { get; init; }
}
