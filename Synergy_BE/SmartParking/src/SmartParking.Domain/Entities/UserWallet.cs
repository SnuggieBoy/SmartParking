namespace SmartParking.Domain.Entities;

/// <summary>
/// Ví tiền của từng User/Owner - mỗi user có 1 ví.
/// Dùng cho: thanh toán booking, gia hạn, nạp tiền (giả lập).
/// </summary>
public class UserWallet
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
