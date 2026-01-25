namespace SmartParking.Application.Common.Settings;

public sealed class VnPaySettings
{
    public string TmnCode { get; init; } = string.Empty;
    public string HashSecret { get; init; } = string.Empty;
    public string PaymentUrl { get; init; } = string.Empty;
    public string ReturnUrl { get; init; } = string.Empty;
    public string Version { get; init; } = "2.1.0";
    public string Command { get; init; } = "pay";
    public string CurrencyCode { get; init; } = "VND";
    public string Locale { get; init; } = "vn";
}
