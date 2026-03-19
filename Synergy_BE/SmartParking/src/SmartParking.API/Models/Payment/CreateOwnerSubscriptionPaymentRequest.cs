using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Payment;

/// <summary>
/// Request model for creating owner subscription payments.
/// </summary>
public record CreateOwnerSubscriptionPaymentRequest(
    [Required(ErrorMessage = "Yêu cầu nâng cấp là bắt buộc")] Guid OwnerUpgradeRequestId,
    [Required(ErrorMessage = "Loại gói là bắt buộc")]
    [StringLength(20, ErrorMessage = "Loại gói không được quá 20 ký tự")]
    [RegularExpression(@"^(Monthly|Yearly)$", ErrorMessage = "Loại gói phải là Tháng hoặc Năm")] string PlanType,
    [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")] string? Description = null
);
