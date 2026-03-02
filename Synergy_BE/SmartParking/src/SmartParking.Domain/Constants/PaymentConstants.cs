namespace SmartParking.Domain.Constants;

public static class PaymentConstants
{
    public const string SePayProvider = "SePay";
    public const string CashProvider = "Cash";
    public const string WalletProvider = "Wallet";
    
    public static class SePayStatus
    {
        public const string Success = "success";
        public const string Failed = "failed";
        public const string Pending = "pending";
        public const string Processing = "processing";
    }

    /// <summary>Tỷ lệ hoàn tiền khi checkout sớm (70% thời gian chưa dùng).</summary>
    public const decimal EarlyCheckoutRefundRate = 0.70m;
}
