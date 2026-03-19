using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Owner;

/// <summary>
/// Request body model when a User wants to upgrade to Owner.
/// </summary>
public sealed class CreateOwnerUpgradeRequestModel
{
    [Required(ErrorMessage = "Tên bãi xe là bắt buộc")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên bãi xe phải từ 2 đến 200 ký tự")]
    public string ParkingLotName { get; set; } = null!;

    [Required(ErrorMessage = "Địa chỉ bãi xe là bắt buộc")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 500 ký tự")]
    public string ParkingLotAddress { get; set; } = null!;

    [Range(-90, 90, ErrorMessage = "Vĩ độ phải từ -90 đến 90")]
    public decimal? Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh độ phải từ -180 đến 180")]
    public decimal? Longitude { get; set; }

    [Required(ErrorMessage = "Loại gói là bắt buộc")]
    [StringLength(20, ErrorMessage = "Loại gói không hợp lệ")]
    [RegularExpression(@"^(Monthly|Yearly)$", ErrorMessage = "Loại gói phải là Tháng hoặc Năm")]
    public string PlanType { get; set; } = "Monthly";

    public Guid? PaymentTransactionId { get; set; }

    [StringLength(500, ErrorMessage = "URL ảnh không được quá 500 ký tự")]
    public string? ImageUrl { get; set; }

    [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
    public string? Description { get; set; }
}

