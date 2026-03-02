using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.Payment;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.Text;

namespace SmartParking.API.Controllers;

/// <summary>
/// Payment processing endpoints.
/// Security: Users process own payments. VNPay callback is public but hash-validated.
/// </summary>
[Route("api/payments")]
public sealed class PaymentController : BaseApiController
{
    private readonly IVnPayService _vnPayService;
    private readonly ISePayService _sePayService;
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IVnPayService vnPayService,
        ISePayService sePayService,
        IPaymentService paymentService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _vnPayService = vnPayService;
        _sePayService = sePayService;
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// SECURITY: Only authenticated users can create payments for their own bookings.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromToken();

        var dto = new CreatePaymentRequestDto(
            request.BookingId,
            request.Amount,
            request.Description
        );

        var response = await _vnPayService.CreatePaymentUrlAsync(dto, userId, ct);
        return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(response, Messages.Payment.CreateSuccess));
    }

    /// <summary>
    /// SECURITY: Only authenticated users can pay for their owner upgrade requests.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrOwnerOrAdmin)]
    [HttpPost("owner-subscription/vnpay")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreateOwnerSubscriptionPayment(
        [FromBody] CreateOwnerSubscriptionPaymentRequest request,
        CancellationToken ct)
    {
        var userId = GetUserIdFromToken();

        var dto = new CreateOwnerSubscriptionPaymentDto(
            request.OwnerUpgradeRequestId,
            request.PlanType,
            request.Description
        );

        var response = await _vnPayService.CreateOwnerSubscriptionPaymentUrlAsync(dto, userId, ct);
        return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(response, "Owner subscription payment URL created"));
    }

    /// <summary>
    /// VNPAY CALLBACK: Public endpoint called by VNPay after payment.
    /// SECURITY CRITICAL:
    /// - [AllowAnonymous] required (VNPay cannot send JWT)
    /// - SecureHash MUST be validated to prevent tampering
    /// - Idempotency check prevents duplicate processing
    /// - Never trust callback data without hash validation
    /// </summary>
    [AllowAnonymous]
    [HttpGet("vnpay-callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> VnPayCallback([FromQuery] VnPayCallbackDto callback, CancellationToken ct)
    {
        var isSuccess = await _vnPayService.ProcessCallbackAsync(callback, ct);

        if (isSuccess)
        {
            return Redirect($"/payment-success?txnRef={callback.vnp_TxnRef}");
        }

        return Redirect($"/payment-failed?txnRef={callback.vnp_TxnRef}");
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

            // SECURITY: Kiểm tra Secret Key trong Header - SePay gửi: Authorization: Apikey {API_KEY}
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var expectedSecret = _configuration["SePay:WebhookSecret"] ?? Environment.GetEnvironmentVariable("SEPAY_WEBHOOK_SECRET");
            if (string.IsNullOrEmpty(expectedSecret))
            {
                _logger.LogError("SePay WebhookSecret chưa cấu hình");
                return StatusCode(500, new { success = false, message = "Server not configured" });
            }

            var isValidAuth = !string.IsNullOrEmpty(authHeader) &&
                authHeader.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(authHeader["Apikey ".Length..].Trim(), expectedSecret.Trim(), StringComparison.Ordinal);

            if (!isValidAuth)
            {
                _logger.LogWarning("SePay webhook: Authorization FAILED. Header present: {HasHeader}", !string.IsNullOrEmpty(authHeader));
                return Unauthorized(new { success = false, message = "Invalid API Key" });
            }

            var isSuccess = await _sePayService.ProcessWebhookAsync(webhook, ct);

            if (isSuccess)
            {
                _logger.LogInformation("SePay webhook processed successfully. RequestId: {RequestId}", requestId);
                return Ok(new { success = true, message = "Webhook processed successfully", requestId });
            }

            _logger.LogWarning("SePay webhook processing failed. RequestId: {RequestId}", requestId);
            return BadRequest(new { success = false, message = "Webhook processing failed", requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SePay webhook exception. RequestId: {RequestId}, ClientIP: {ClientIP}", requestId, clientIp);
            return Ok(new { success = false, message = "Webhook received but processing failed", requestId });
        }
    }

    #endregion
}

