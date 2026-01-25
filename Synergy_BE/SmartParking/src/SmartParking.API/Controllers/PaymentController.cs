using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Models.Payment;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartParking.API.Controllers;

/// <summary>
/// Payment processing endpoints.
/// Security: Users process own payments. VNPay callback is public but hash-validated.
/// </summary>
[ApiController]
[Route("api/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly IPaymentService _paymentService;

    public PaymentController(IVnPayService vnPayService, IPaymentService paymentService)
    {
        _vnPayService = vnPayService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// SECURITY: Only authenticated users can create payments for their own bookings.
    /// Ownership validated in service layer.
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<PaymentResponseDto>.FailureResponse(Messages.Common.Unauthorized));
        }

        var dto = new CreatePaymentRequestDto(
            request.BookingId,
            request.Amount,
            request.Description
        );

        var response = await _vnPayService.CreatePaymentUrlAsync(dto, userId, ct);
        return Ok(ApiResponse<PaymentResponseDto>.SuccessResponse(response, Messages.Payment.CreateSuccess));
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
    [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    [HttpGet("booking/{bookingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentStatusDto>>> GetPaymentStatusByBooking(Guid bookingId, CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<PaymentStatusDto>.FailureResponse(Messages.Common.Unauthorized));
        }

        var isAdmin = User.IsInRole(AuthConstants.Roles.Admin);
        var dto = await _paymentService.GetPaymentStatusByBookingAsync(bookingId, userId, isAdmin, ct);
        return Ok(ApiResponse<PaymentStatusDto>.SuccessResponse(dto, Messages.Payment.PaymentStatusRetrieved));
    }
}

