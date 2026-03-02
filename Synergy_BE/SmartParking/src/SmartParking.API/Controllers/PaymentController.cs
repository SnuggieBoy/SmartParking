using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.Payment;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;

namespace SmartParking.API.Controllers;

/// <summary>
/// Payment processing endpoints.
/// Security: Users process own payments. SePay webhook is public but API key validated.
/// </summary>
[Route("api/payments")]
public sealed class PaymentController : BaseApiController
{
    private readonly ISePayService _sePayService;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        ISePayService sePayService,
        IPaymentService paymentService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _sePayService = sePayService;
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// SECURITY: Only booking owner OR Admin can query payment status.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpGet("booking/{bookingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentStatusDto>>> GetPaymentStatusByBooking(Guid bookingId, CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var isAdmin = IsAdmin();
        var isOwner = IsOwner();
        var dto = await _paymentService.GetPaymentStatusByBookingAsync(bookingId, userId, isAdmin, isOwner, ct);
        return Ok(ApiResponse<PaymentStatusDto>.SuccessResponse(dto, Messages.Payment.PaymentStatusRetrieved));
    }

    /// <summary>
    /// Get payment history for current user
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentHistoryDto>>>> GetPaymentHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _paymentService.GetPaymentHistoryAsync(userId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<PaymentHistoryDto>>.SuccessResponse(result, "Payment history retrieved successfully"));
    }

    #region SePay Integration

    /// <summary>
    /// SEPAY: Create payment with bank transfer QR code.
    /// SECURITY: Only authenticated users can create payments for their own bookings.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("sepay/create")]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SePayPaymentResponseDto>>> CreateSePayPayment(
        [FromBody] CreateSePayPaymentDto request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromToken();
        var response = await _sePayService.CreatePaymentAsync(request, userId, ct);
        return Ok(ApiResponse<SePayPaymentResponseDto>.SuccessResponse(response, "SePay payment created successfully"));
    }

    /// <summary>
    /// SEPAY: Create owner subscription payment with bank transfer QR code.
    /// SECURITY: Only authenticated users can pay for their owner upgrade requests.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("owner-subscription/sepay")]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SePayPaymentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SePayPaymentResponseDto>>> CreateOwnerSubscriptionSePayPayment(
        [FromBody] CreateOwnerSubscriptionPaymentRequest request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromToken();

        var dto = new CreateOwnerSubscriptionPaymentDto(
            request.OwnerUpgradeRequestId,
            request.PlanType,
            request.Description
        );

        var response = await _sePayService.CreateOwnerSubscriptionPaymentAsync(dto, userId, ct);
        return Ok(ApiResponse<SePayPaymentResponseDto>.SuccessResponse(response, "Owner subscription SePay payment created"));
    }

    /// <summary>
    /// SEPAY WEBHOOK: Endpoint nhận IPN từ SePay khi có chuyển khoản vào.
    /// SECURITY: Kiểm tra Authorization header "Apikey {SecretKey}" - theo tài liệu SePay.
    /// SePay yêu cầu response: HTTP 200/201 + {"success": true}
    /// </summary>
    [AllowAnonymous]
    [HttpPost("sepay/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDto webhook, CancellationToken ct)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            _logger.LogInformation(
                "SePay webhook received. RequestId: {RequestId}, Id: {Id}, Content: {Content}, ClientIP: {ClientIP}",
                requestId, webhook.Id, webhook.Content, clientIp);

            // SECURITY: SePay gửi header "Authorization: Apikey {API_KEY}" - dùng ApiKey hoặc WebhookSecret
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var apiKey = (_configuration["SePay:ApiKey"] ?? Environment.GetEnvironmentVariable("SEPAY_API_KEY") ?? "").Trim();
            var webhookSecret = (_configuration["SePay:WebhookSecret"] ?? Environment.GetEnvironmentVariable("SEPAY_WEBHOOK_SECRET") ?? "").Trim();
            var expectedSecret = !string.IsNullOrEmpty(webhookSecret) ? webhookSecret : apiKey;
            if (string.IsNullOrEmpty(expectedSecret))
            {
                _logger.LogError("SePay ApiKey/WebhookSecret chưa cấu hình");
                return StatusCode(500, new { success = false, message = "Server not configured" });
            }

            var receivedKey = authHeader?.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase) == true
                ? authHeader["Apikey ".Length..].Trim()
                : null;
            var isValidAuth = !string.IsNullOrEmpty(receivedKey) &&
                string.Equals(receivedKey, expectedSecret, StringComparison.Ordinal);

            if (!isValidAuth)
            {
                _logger.LogWarning("SePay webhook: Authorization FAILED. HasHeader: {HasHeader}, Content: {Content}", !string.IsNullOrEmpty(authHeader), webhook?.Content ?? "null");
                return Unauthorized(new { success = false, message = "Invalid API Key" });
            }

            var (isSuccess, errorReason) = await _sePayService.ProcessWebhookAsync(webhook, ct);

            if (isSuccess)
            {
                _logger.LogInformation("SePay webhook processed successfully. RequestId: {RequestId}", requestId);
                return Ok(new { success = true, message = "Webhook processed successfully", requestId });
            }

            _logger.LogWarning("SePay webhook failed. RequestId: {RequestId}, Reason: {Reason}", requestId, errorReason);
            return BadRequest(new { success = false, message = "Webhook processing failed", reason = errorReason ?? "Unknown", requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SePay webhook exception. RequestId: {RequestId}, Content: {Content}, ClientIP: {ClientIP}", requestId, webhook?.Content ?? "null", clientIp);
            return StatusCode(500, new { success = false, message = "Webhook processing error - SePay will retry", requestId });
        }
    }

    #endregion
}

