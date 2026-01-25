using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Payment;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace SmartParking.Infrastructure.Services;

public sealed class VnPayService : IVnPayService
{
    private readonly VnPaySettings _settings;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;

    public VnPayService(
        IOptions<VnPaySettings> settings,
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository)
    {
        _settings = settings.Value;
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<PaymentResponseDto> CreatePaymentUrlAsync(
        CreatePaymentRequestDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate booking ownership (Admin check would be done at controller level)
        if (booking.UserId != userId)
        {
            throw new ForbiddenException(Messages.Common.Forbidden);
        }

        var txnRef = $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = request.BookingId,
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = PaymentConstants.VnPayProvider,
            PaymentStatus = nameof(PaymentStatus.Pending),
            VnpTxnRef = txnRef,
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.CreateAsync(payment, ct);

        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_Version", _settings.Version },
            { "vnp_Command", _settings.Command },
            { "vnp_TmnCode", _settings.TmnCode },
            { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", _settings.CurrencyCode },
            { "vnp_IpAddr", "127.0.0.1" },
            { "vnp_Locale", _settings.Locale },
            { "vnp_OrderInfo", request.Description },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _settings.ReturnUrl },
            { "vnp_TxnRef", txnRef }
        };

        var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();
        var queryString = string.Join("&", sortedParams.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}"));
        var signData = queryString;
        var secureHash = HmacSHA512(_settings.HashSecret, signData);

        var paymentUrl = $"{_settings.PaymentUrl}?{queryString}&vnp_SecureHash={secureHash}";

        return new PaymentResponseDto(paymentUrl, txnRef);
    }

    /// <summary>
    /// SECURITY CRITICAL: Processes VNPay payment callback.
    /// Implements:
    /// 1. Idempotency check (prevents duplicate processing)
    /// 2. SecureHash validation (prevents tampering)
    /// 3. Transaction logging (audit trail)
    /// </summary>
    public async Task<bool> ProcessCallbackAsync(VnPayCallbackDto callback, CancellationToken ct = default)
    {
        // Step 1: Find payment transaction
        var payment = await _paymentRepository.GetByTxnRefAsync(callback.vnp_TxnRef, ct);
        if (payment == null)
        {
            return false;
        }

        // SECURITY: Idempotency check - prevent duplicate processing
        // If payment already processed (status is not Pending), reject callback
        if (payment.PaymentStatus != nameof(PaymentStatus.Pending))
        {
            // Log the duplicate callback attempt for security monitoring
            var duplicateLog = new PaymentLog
            {
                LogId = Guid.NewGuid(),
                PaymentId = payment.PaymentId,
                RawData = $"DUPLICATE_CALLBACK_REJECTED: {JsonSerializer.Serialize(callback)}",
                CreatedAt = DateTime.UtcNow
            };
            await _paymentRepository.CreateLogAsync(duplicateLog, ct);
            
            // Return success if original was successful (idempotent response)
            return payment.PaymentStatus == nameof(PaymentStatus.Success);
        }

        // Step 2: Build params for hash validation (exclude vnp_SecureHash)
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_TmnCode", callback.vnp_TmnCode },
            { "vnp_Amount", callback.vnp_Amount },
            { "vnp_BankCode", callback.vnp_BankCode },
            { "vnp_BankTranNo", callback.vnp_BankTranNo },
            { "vnp_CardType", callback.vnp_CardType },
            { "vnp_PayDate", callback.vnp_PayDate },
            { "vnp_OrderInfo", callback.vnp_OrderInfo },
            { "vnp_TransactionNo", callback.vnp_TransactionNo },
            { "vnp_ResponseCode", callback.vnp_ResponseCode },
            { "vnp_TransactionStatus", callback.vnp_TransactionStatus },
            { "vnp_TxnRef", callback.vnp_TxnRef },
            { "vnp_SecureHashType", callback.vnp_SecureHashType }
        };

        // Step 3: SECURITY - Validate SecureHash to prevent tampering
        var sortedParams = vnpParams.OrderBy(x => x.Key).ToList();
        var signData = string.Join("&", sortedParams.Select(x => $"{x.Key}={x.Value}"));
        var secureHash = HmacSHA512(_settings.HashSecret, signData);

        if (!secureHash.Equals(callback.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
        {
            // Log hash validation failure for security monitoring
            var invalidHashLog = new PaymentLog
            {
                LogId = Guid.NewGuid(),
                PaymentId = payment.PaymentId,
                RawData = $"INVALID_HASH_REJECTED: {JsonSerializer.Serialize(callback)}",
                CreatedAt = DateTime.UtcNow
            };
            await _paymentRepository.CreateLogAsync(invalidHashLog, ct);
            return false;
        }

        // Step 4: Update payment status
        payment.VnpTransactionNo = callback.vnp_TransactionNo;
        payment.VnpResponseCode = callback.vnp_ResponseCode;
        payment.PaymentStatus = callback.vnp_ResponseCode == PaymentConstants.VnPayResponseCodes.Success 
            ? nameof(PaymentStatus.Success) 
            : nameof(PaymentStatus.Failed);

        await _paymentRepository.UpdateAsync(payment, ct);

        // Step 5: Log callback for audit trail
        var log = new PaymentLog
        {
            LogId = Guid.NewGuid(),
            PaymentId = payment.PaymentId,
            RawData = JsonSerializer.Serialize(callback),
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.CreateLogAsync(log, ct);

        // Step 6: Update booking status if payment successful
        if (callback.vnp_ResponseCode == PaymentConstants.VnPayResponseCodes.Success)
        {
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, ct);
            if (booking != null && booking.Status == nameof(BookingStatus.Pending))
            {
                booking.Status = nameof(BookingStatus.Confirmed);
                await _bookingRepository.UpdateAsync(booking, ct);
            }
        }

        return callback.vnp_ResponseCode == PaymentConstants.VnPayResponseCodes.Success;
    }

    private static string HmacSHA512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
