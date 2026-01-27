namespace SmartParking.Domain.Constants;

public static class PaymentConstants
{
    public const string VnPayProvider = "VNPay";
    public const string SePayProvider = "SePay";
    public const string CashProvider = "Cash";
    public const string WalletProvider = "Wallet";
    
    public static class VnPayResponseCodes
    {
        public const string Success = "00";
        public const string TransactionFailed = "07";
        public const string InvalidSignature = "97";
        public const string TransactionNotFound = "02";
    }
    
    public static class VnPayTransactionStatus
    {
        public const string Success = "00";
        public const string Failed = "01";
        public const string Processing = "02";
    }
    
    public static class SePayStatus
    {
        public const string Success = "success";
        public const string Failed = "failed";
        public const string Pending = "pending";
        public const string Processing = "processing";
    }
}
