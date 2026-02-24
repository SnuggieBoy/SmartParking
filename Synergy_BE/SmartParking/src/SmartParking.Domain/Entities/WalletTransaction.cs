namespace SmartParking.Domain.Entities;

/// <summary>
/// Lịch sử giao dịch ví: TopUp, BookingPayment, ExtensionPayment, Refund.
/// </summary>
public class WalletTransaction
{
    public Guid WalletTransactionId { get; set; }
    public Guid UserId { get; set; }
    /// <summary>+ nạp tiền, - thanh toán</summary>
    public decimal Amount { get; set; }
    public string Type { get; set; } = "TopUp"; // TopUp, BookingPayment, ExtensionPayment, Refund
    public decimal BalanceAfter { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
