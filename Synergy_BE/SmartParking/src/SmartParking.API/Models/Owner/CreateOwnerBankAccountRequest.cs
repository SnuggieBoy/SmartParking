using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Owner;

public sealed class CreateOwnerBankAccountRequest
{
    [Required(ErrorMessage = "Mã ngân hàng là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã ngân hàng không được quá 20 ký tự")]
    public string BankCode { get; set; } = null!;

    [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên ngân hàng không được quá 100 ký tự")]
    public string BankName { get; set; } = null!;

    [Required(ErrorMessage = "Số tài khoản là bắt buộc")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Số tài khoản phải từ 8 đến 50 ký tự")]
    [RegularExpression(@"^\d{8,20}$", ErrorMessage = "Số tài khoản phải là chữ số từ 8 đến 20 ký tự")]
    public string AccountNumber { get; set; } = null!;

    [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên chủ tài khoản phải từ 2 đến 100 ký tự")]
    public string AccountHolderName { get; set; } = null!;

    public bool IsDefault { get; set; } = true;
}

