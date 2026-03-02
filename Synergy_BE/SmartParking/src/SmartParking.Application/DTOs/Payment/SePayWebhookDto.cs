using System.Text.Json.Serialization;

namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// SePay webhook payload - theo tài liệu chính thức: https://developer.sepay.vn/sepay-webhooks/tich-hop-webhook
/// SePay gửi POST với JSON này khi có giao dịch chuyển khoản vào.
/// </summary>
public sealed record SePayWebhookDto
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("gateway")]
    public string Gateway { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; init; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>Nội dung chuyển khoản - chứa mã đơn hàng (vd: SMARTPARKING SP_20250223_ABC12345)</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>in = tiền vào, out = tiền ra</summary>
    [JsonPropertyName("transferType")]
    public string TransferType { get; init; } = string.Empty;

    [JsonPropertyName("transferAmount")]
    public decimal TransferAmount { get; init; }

    [JsonPropertyName("accumulated")]
    public decimal Accumulated { get; init; }

    [JsonPropertyName("subAccount")]
    public string? SubAccount { get; init; }

    [JsonPropertyName("referenceCode")]
    public string ReferenceCode { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
