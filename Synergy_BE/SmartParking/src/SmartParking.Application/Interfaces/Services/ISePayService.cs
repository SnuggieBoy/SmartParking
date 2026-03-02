using SmartParking.Application.DTOs.Payment;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for SePay payment integration
/// </summary>
public interface ISePayService
{
    /// <summary>
    /// Creates SePay payment request and returns QR code + bank transfer info
    /// </summary>
    Task<SePayPaymentResponseDto> CreatePaymentAsync(
        CreateSePayPaymentDto request,
        Guid userId,
        CancellationToken ct = default);

    Task<SePayPaymentResponseDto> CreateOwnerSubscriptionPaymentAsync(
        CreateOwnerSubscriptionPaymentDto request,
        Guid userId,
        CancellationToken ct = default);

    Task<SePayPaymentResponseDto> CreateWalletTopUpPaymentAsync(
        WalletTopUpRequestDto request,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Processes SePay webhook callback.
    /// SECURITY: API Key verification done tại Controller trước khi gọi.
    /// Returns (success, errorReason) - errorReason only when success=false.
    /// </summary>
    Task<(bool Success, string? ErrorReason)> ProcessWebhookAsync(SePayWebhookDto webhook, CancellationToken ct = default);
}
