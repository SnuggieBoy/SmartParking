using System.ComponentModel.DataAnnotations;

namespace SmartParking.API.Models.Payment;

/// <summary>
/// Request model for creating owner subscription payments.
/// </summary>
public record CreateOwnerSubscriptionPaymentRequest(
    [Required] Guid OwnerUpgradeRequestId,
    [Required] string PlanType,
    string? Description = null
);
