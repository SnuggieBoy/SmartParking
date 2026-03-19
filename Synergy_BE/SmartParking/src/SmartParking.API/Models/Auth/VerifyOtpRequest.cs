using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Auth;

public sealed record VerifyOtpRequest
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được quá 256 ký tự")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Mã OTP là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải đúng 6 chữ số")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số")]
    public required string OtpCode { get; init; }
}
