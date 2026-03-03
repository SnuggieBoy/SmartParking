namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// Payment status by orderId (SePay txn ref). Used for polling after bank transfer.
/// </summary>
public sealed record PaymentStatusByOrderDto(
    string OrderId,
    string Status,
    decimal Amount,
    string PaymentType,
    DateTime? PaidAt
);
