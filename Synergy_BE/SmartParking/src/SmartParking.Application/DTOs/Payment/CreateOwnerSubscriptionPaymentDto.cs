namespace SmartParking.Application.DTOs.Payment;

public record CreateOwnerSubscriptionPaymentDto(
    Guid OwnerUpgradeRequestId,
    string PlanType,
    string? Description = null
);
