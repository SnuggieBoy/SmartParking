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
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IVnPayService vnPayService,
        ISePayService sePayService,
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _vnPayService = vnPayService;
        _sePayService = sePayService;
        _paymentService = paymentService;
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
    /// SEPAY WEBHOOK: Public endpoint called by SePay after bank transfer.
    /// 
    /// CRITICAL DESIGN PRINCIPLE:
    /// - This webhook is the SINGLE SOURCE OF TRUTH for payment confirmation
    /// - ReturnUrl (user redirect) is NEVER used for payment status updates
    /// - Only webhook can mark payment as successful
    /// 
    /// SECURITY CRITICAL:
    /// - [AllowAnonymous] required (SePay cannot send JWT)
    /// - Signature MUST be validated to prevent tampering
    /// - HTTPS enforcement in production
    /// - IP whitelist validation (optional)
    /// - Idempotency check prevents duplicate processing
    /// - Amount validation prevents fraud
    /// - Never trust webhook data without signature verification
    /// </summary>
    [AllowAnonymous]
    [HttpPost("sepay/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDto webhook, CancellationToken ct)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        try
        {
            _logger.LogInformation(
                "SePay webhook request received. RequestId: {RequestId}, OrderId: {OrderId}, ClientIP: {ClientIP}, IsHttps: {IsHttps}",
                requestId, webhook.OrderId, clientIp, Request.IsHttps);

            // SECURITY: HTTPS enforcement in production
            if (!Request.IsHttps && !HttpContext.Request.Host.Host.Contains("localhost"))
            {
                _logger.LogWarning(
                    "SePay webhook: Non-HTTPS request rejected. RequestId: {RequestId}, ClientIP: {ClientIP}",
                    requestId, clientIp);
                return StatusCode(StatusCodes.Status426UpgradeRequired, 
                    new { message = "HTTPS required" });
            }

            // Get raw body for signature verification
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawPayload = await reader.ReadToEndAsync(ct);

            // Get signature from header
            var signature = Request.Headers["X-SePay-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning(
                    "SePay webhook: Missing signature header. RequestId: {RequestId}, OrderId: {OrderId}",
                    requestId, webhook.OrderId);
                return Unauthorized(new { message = "Missing signature", requestId });
            }

            // Process webhook (SINGLE SOURCE OF TRUTH)
            var isSuccess = await _sePayService.ProcessWebhookAsync(webhook, signature, rawPayload, ct);

            if (isSuccess)
            {
                _logger.LogInformation(
                    "SePay webhook processed successfully. RequestId: {RequestId}, OrderId: {OrderId}",
                    requestId, webhook.OrderId);
                return Ok(new { message = "Webhook processed successfully", requestId });
            }

            _logger.LogWarning(
                "SePay webhook processing failed. RequestId: {RequestId}, OrderId: {OrderId}",
                requestId, webhook.OrderId);
            return BadRequest(new { message = "Webhook processing failed", requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "SePay webhook: Unhandled exception. RequestId: {RequestId}, OrderId: {OrderId}, ClientIP: {ClientIP}",
                requestId, webhook.OrderId, clientIp);

            // Return 200 to prevent SePay from retrying (we've logged the error)
            // SePay will not retry if we return 200
            return Ok(new { message = "Webhook received but processing failed", requestId });
        }
    }

    #endregion
}

