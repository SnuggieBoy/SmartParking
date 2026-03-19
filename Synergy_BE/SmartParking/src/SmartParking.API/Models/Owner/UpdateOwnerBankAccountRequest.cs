using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Owner;

public sealed class UpdateOwnerBankAccountRequest
{
    [StringLength(20, ErrorMessage = "Mã ngân hàng không được quá 20 ký tự")]
    public string? BankCode { get; set; }

    [StringLength(100, ErrorMessage = "Tên ngân hàng không được quá 100 ký tự")]
    public string? BankName { get; set; }

    [StringLength(50, ErrorMessage = "Số tài khoản không được quá 50 ký tự")]
    [RegularExpression(@"^\d{8,20}$", ErrorMessage = "Số tài khoản phải từ 8 đến 20 chữ số")]
    public string? AccountNumber { get; set; }

    [StringLength(100, ErrorMessage = "Tên chủ tài khoản không được quá 100 ký tự")]
    public string? AccountHolderName { get; set; }

    public bool? IsDefault { get; set; }
}

