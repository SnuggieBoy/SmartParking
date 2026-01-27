using System.Text.Json.Serialization;

namespace SmartParking.Application.DTOs.Payment;

/// <summary>
/// SePay webhook payload
/// Received when user completes bank transfer
/// </summary>
public sealed record SePayWebhookDto
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; init; } = string.Empty;

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("bank_code")]
    public string? BankCode { get; init; }

    [JsonPropertyName("bank_account")]
    public string? BankAccount { get; init; }

    [JsonPropertyName("transfer_content")]
    public string? TransferContent { get; init; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
